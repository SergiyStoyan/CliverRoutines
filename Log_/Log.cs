//********************************************************************************************
//Author: Sergey Stoyan
//        sergey.stoyan@gmail.com
//        sergey.stoyan@hotmail.com
//        stoyan@cliversoft.com
//        http://www.cliversoft.com
//********************************************************************************************

using System;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace Cliver
{
    public static partial class Log
    {
        static readonly object lockObject = new object();

        /// <summary>
        /// Shuts down the log engine and re-initializes it. Optional.
        /// </summary>
        /// <param name="mode">log configuration</param>
        /// <param name="primaryBaseDirs">log directories ordered by preference</param>
        /// <param name="deleteLogsOlderThanDays">old logs that are older than the number of days will be deleted</param>
        public static void Initialize(Mode mode, List<string> primaryBaseDirs = null, int deleteLogsOlderThanDays = 10)
        {
            lock (lockObject)
            {
                Log.CloseAll();
                Log.mode = mode;
                Log.primaryBaseDirs = primaryBaseDirs;
                Log.deleteLogsOlderThanDays = deleteLogsOlderThanDays;
            }
        }
        static List<string> primaryBaseDirs = null;
        static int deleteLogsOlderThanDays = 10;
        static Mode mode = Mode.SAME_FOLDER;

        /// <summary>
        /// Log level which is passed to each log as default.
        /// </summary>
        public static Level DefaultLevel = Level.ALL;

        /// <summary>
        /// Maximum log file length in bytes which is passed to each log as default.
        /// If negative than no effect.
        /// </summary>
        public static int DefaultMaxFileSize = -1;

        /// <summary>
        /// Pattern of time recorded before a log message. See DateTime.ToString() format.
        /// </summary>
        public static string TimePattern = "[dd-MM-yy HH:mm:ss] ";

        /// <summary>
        /// Whether thread log indexes of closed logs should be reused.
        /// </summary>
        public static bool ReuseThreadLogIndexes = false;

        /// <summary>
        /// Extension of log files.
        /// </summary>
        public static string FileExtension = "log";

        /// <summary>
        /// Log configuration.
        /// </summary>
        public enum Mode
        {
            /// <summary>
            /// Each session creates its own folder.
            /// </summary>
            FOLDER_PER_SESSION,
            /// <summary>
            /// <summary>
            /// No session folder is created. Log files are in the same folder.
            /// </summary>
            SAME_FOLDER
        }

        /// <summary>
        /// Head session which is created by default.
        /// </summary>
        public static Session Head
        {
            get
            {
                if (headSession == null)
                    headSession = Session.Get(HEAD_SESSION_NAME);
                return headSession;
            }
        }
        static Session headSession = null;
        public const string HEAD_SESSION_NAME = "";

        /// <summary>
        /// Default log of the head session. 
        /// Depending on condition THREAD_LOG_IS_DEFAULT, it is either Main log or Thread log.
        /// </summary>
        public static Writer Default
        {
            get
            {
                return Head.Default;
            }
        }

        /// <summary>
        /// Main log of the head session.
        /// </summary>
        public static Session.NamedWriter Main
        {
            get
            {
                return Head.Main;
            }
        }

        /// <summary>
        /// Thread log of the head session.
        /// </summary>
        public static Session.ThreadWriter Thread
        {
            get
            {
                return Head.Thread;
            }
        }

        /// <summary>
        /// Message importance levels.
        /// </summary>
        public enum Level
        {
            NONE,
            ERROR,
            WARNING,
            INFORM,
            ALL
        }

        /// <summary>
        /// Message types.
        /// </summary>
        public enum MessageType
        {
            LOG,
            DEBUG,
            INFORM,
            WARNING,
            ERROR,
            EXIT,
            TRACE,
            //INFORM2 = 11,
            //WARNING2 = 21,
            //ERROR2 = 31,
            //EXIT2 = 41,
        }

        /// <summary>
        /// Clear all existing sessions and close all the logs.
        /// </summary>
        public static void CloseAll()
        {
            lock (lockObject)
            {
                Session.CloseAll();
                workDir = null;
                headSession = null;

                GC.Collect();
            }
        }

        /// <summary>
        ///Parent log directory.
        /// </summary>
        public static string WorkDir
        {
            get
            {
                if (workDir == null)
                    setWorkDir(DefaultLevel > Level.NONE);
                return workDir;
            }
        }
        static string workDir = null;
        public const string WorkDirNameSuffix = @"_Sessions";
        static Thread deletingOldLogsThread = null;
        public static Func<string, bool> DeleteOldLogsDialog = null;

        static void setWorkDir(bool create)
        {
            lock (lockObject)
            {
                if (workDir != null)
                {
                    if (!create)
                        return;
                    if (Directory.Exists(workDir))
                        return;
                }
                List<string> baseDirs = new List<string> {
                                Log.AppDir,
                                CompanyUserDataDir,
                                CompanyCommonDataDir,
                                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                                System.IO.Path.GetTempPath() + System.IO.Path.DirectorySeparatorChar + CompanyName + System.IO.Path.DirectorySeparatorChar,
                                };
                if (Log.primaryBaseDirs != null)
                    baseDirs.InsertRange(0, Log.primaryBaseDirs);
                foreach (string baseDir in baseDirs)
                {
                    workDir = baseDir + System.IO.Path.DirectorySeparatorChar + Log.ProcessName + WorkDirNameSuffix;
                    if (create)
                        try
                        {
                            if (!Directory.Exists(workDir))
                                FileSystemRoutines.CreateDirectory(workDir);
                            string testFile = workDir + System.IO.Path.DirectorySeparatorChar + "test";
                            File.WriteAllText(testFile, "test");
                            File.Delete(testFile);
                            Log.BaseDir = baseDir;
                            break;
                        }
                        catch //(Exception e)
                        {
                            workDir = null;
                        }
                }
                if (workDir == null)
                    throw new Exception("Could not access any log directory.");
                workDir = PathRoutines.GetNormalizedPath(workDir, false);
                if (Directory.Exists(workDir) && deleteLogsOlderThanDays >= 0)
                    deletingOldLogsThread = ThreadRoutines.Start(() => { Log.DeleteOldLogs(deleteLogsOlderThanDays, DeleteOldLogsDialog); });//to avoid a concurrent loop while accessing the log file from the same thread 
                else
                    throw new Exception("Could not create log folder!");
            }
            // deletingOldLogsThread?.Join();      
        }

        static public string BaseDir { get; private set; }
    }

    //public class TerminatingException : Exception
    //{
    //    public TerminatingException(string message)
    //        : base(message)
    //    {
    //        LogMessage.Exit(message);
    //    }
    //}

    /// <summary>
    /// Trace info for such Exception is not logged. Used for foreseen errors.
    /// </summary>
    public class Exception2 : Exception
    {
        public Exception2(string message)
            : base(message)
        {
        }

        public Exception2(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}


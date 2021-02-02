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
        /// <param name="parentDirs">parent log directories ordered by preference</param>
        /// <param name="deleteLogsOlderThanDays">old logs that are older than the number of days will be deleted</param>
        public static void Initialize(Mode? mode = null, List<string> parentDirs = null, int deleteLogsOlderThanDays = 10)
        {
            lock (lockObject)
            {
                Log.CloseAll();
                if (mode != null)
                    Log.mode = (Mode)mode;
                Log.parentDirs = parentDirs;
                Log.deleteLogsOlderThanDays = deleteLogsOlderThanDays;
            }
        }
        static List<string> parentDirs = null;
        static int deleteLogsOlderThanDays = 10;
        static Mode mode = Mode.ONE_FOLDER | Mode.NAMED_DEFAULT_LOG;

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
        /// Suffix to the base log folder name.
        /// </summary>
        public static string BaseDirNameSuffix = @"_Logs";

        /// <summary>
        /// Log configuration.
        /// </summary>
        public enum Mode : uint
        {
            /// <summary>
            /// No session folder is created. Log files are in one folder.
            /// It is default option if not FOLDER_PER_SESSION, otherwise, ignored.
            /// </summary>
            ONE_FOLDER = 1,//0001
            /// <summary>
            /// Each session creates its own folder.
            /// </summary>
            FOLDER_PER_SESSION = 2,//0010
            /// <summary>
            /// Default log is named log.
            /// It is default option if not THREAD_DEFAULT_LOG, otherwise, ignored.
            /// </summary>
            NAMED_DEFAULT_LOG = 4,//0100
            /// <summary>
            /// Default log is thread log.
            /// </summary>
            THREAD_DEFAULT_LOG = 8,//1000
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
                baseDirs = null;
                headSession = null;

                GC.Collect();
            }
        }

        /// <summary>
        ///Base log directory.
        /// </summary>
        public static string BaseDir
        {
            get
            {
                if (baseDirs == null)
                    setBaseDir(DefaultLevel > Level.NONE);
                return baseDirs;
            }
        }
        static string baseDirs = null;
        static Thread deletingOldLogsThread = null;
        public static Func<string, bool> DeleteOldLogsDialog = null;

        static void setBaseDir(bool create)
        {
            lock (lockObject)
            {
                if (baseDirs != null)
                {
                    if (!create)
                        return;
                    if (Directory.Exists(baseDirs))
                        return;
                }
                List<string> parentDirs = new List<string> {
                                Log.AppDir,
                                CompanyUserDataDir,
                                CompanyCommonDataDir,
                                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                                Path.GetTempPath() + Path.DirectorySeparatorChar + CompanyName + Path.DirectorySeparatorChar,
                                };
                if (Log.parentDirs != null)
                    parentDirs.InsertRange(0, Log.parentDirs);
                foreach (string parentDir in parentDirs)
                {
                    baseDirs = parentDir + Path.DirectorySeparatorChar + Log.ProcessName + BaseDirNameSuffix;
                    if (create)
                        try
                        {
                            if (!Directory.Exists(baseDirs))
                                FileSystemRoutines.CreateDirectory(baseDirs);
                            string testFile = baseDirs + Path.DirectorySeparatorChar + "test";
                            File.WriteAllText(testFile, "test");
                            File.Delete(testFile);
                            break;
                        }
                        catch //(Exception e)
                        {
                            baseDirs = null;
                        }
                }
                if (baseDirs == null)
                    throw new Exception("Could not access any log directory.");
                baseDirs = PathRoutines.GetNormalizedPath(baseDirs, false);
                if (Directory.Exists(baseDirs) && deleteLogsOlderThanDays >= 0)
                    deletingOldLogsThread = ThreadRoutines.Start(() => { Log.DeleteOldLogs(deleteLogsOlderThanDays, DeleteOldLogsDialog); });//to avoid a concurrent loop while accessing the log file from the same thread 
            }
        }
    }

    /// <summary>
    /// Trace info for such Exception is not logged. 
    /// It is intended for foreseen errors.
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


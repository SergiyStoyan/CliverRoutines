//********************************************************************************************
//Author: Sergiy Stoyan
//        s.y.stoyan@gmail.com, sergiy.stoyan@outlook.com, stoyan@cliversoft.com
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
        /// <param name="baseDirs">directories for logging, ordered by preference. When NULL or not usable, the built-in directory list is used.</param>
        /// <param name="deleteLogsOlderThanDays">old logs that are older than the number of days will be deleted. When negative, no clean-up is performed.</param>
        /// <param name="rootDirName">RootDir folder name</param>
        public static void Initialize(Mode? mode = null, List<string> baseDirs = null, int deleteLogsOlderThanDays = 10, string rootDirName = null)
        {
            initialize(mode, baseDirs, true, deleteLogsOlderThanDays, rootDirName);
        }
        static void initialize(Mode? mode, List<string> baseDirs, bool useDefaultBaseDirs, int deleteLogsOlderThanDays, string rootDirName)
        {
            lock (lockObject)
            {
                Log.CloseAll();
                if (mode != null)
                    Log.mode = (Mode)mode;
                Log.baseDirs = baseDirs;
                Log.useDefaultBaseDirs = useDefaultBaseDirs;
                Log.deleteLogsOlderThanDays = deleteLogsOlderThanDays;
                Log.rootDirName = rootDirName != null ? rootDirName : Log.ProgramName;
            }
        }
        static List<string> baseDirs = null;
        static bool useDefaultBaseDirs = true;
        static int deleteLogsOlderThanDays = 10;
        static Mode mode = Mode.ONE_FOLDER | Mode.DEFAULT_NAMED_LOG;
        static string rootDirName;// { get; private set; }

        /// <summary>
        /// Shuts down the log engine and re-initializes it. Optional. Must be used when only 1 explicite baseDir must be set.
        /// </summary>
        /// <param name="baseDir">the definite directory for logging. NULL triggers exception.</param>
        /// <param name="mode">log configuration</param>
        /// <param name="deleteLogsOlderThanDays">old logs that are older than the number of days will be deleted. When negative, no clean-up is performed.</param>
        /// <param name="rootDirName">RootDir folder name</param>
        public static void Initialize(string baseDir, Mode? mode = null, int deleteLogsOlderThanDays = 10, string rootDirName = null)
        {
            if (baseDir == null)
                throw new Exception("Log base dir must be specified.");
            initialize(mode, new List<string> { baseDir }, false, deleteLogsOlderThanDays, rootDirName);
        }

        /// <summary>
        /// Log level which is passed to each log as default.
        /// </summary>
        public static Level DefaultLevel = Level.INFORM;


        //public static Event<WritingHandlerArguments> DefaultWritingEvent = null;
        //public class WritingHandlerArguments
        //{
        //    public string LogWriterName;
        //    public Log.MessageType MessageType;
        //    public string Message;
        //    public string Details;
        //}
        //public static void ClearWritingEventHandlersAll()
        //{
        //    lock (lockObject)
        //    {
        //        Session.ClearWritingEventHandlersAll();
        //        NamedWriter.ClearWritingEventHandlersAll();
        //    }
        //}

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
        /// Whether thread log indexes of closed logs can be reused.
        /// </summary>
        public static bool ReuseThreadLogIndexes = false;

        /// <summary>
        /// Extension of log files.
        /// </summary>
        public static string FileExtension = "log";

        /// <summary>
        /// Suffix to the RootDir folder name.
        /// </summary>
        public static string RootDirNameSuffix = @"_Logs";

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
            DEFAULT_NAMED_LOG = 4,//0100
            /// <summary>
            /// Default log is thread log.
            /// </summary>
            DEFAULT_THREAD_LOG = 8,//1000
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

        ///// <summary>
        ///// Close Head session.
        ///// </summary>
        //static public void Close(bool reuse)
        //{
        //    Head.Close(reuse);
        //}

        ///// <summary>
        ///// The head session's directory.
        ///// </summary>
        //public static string Dir
        //{
        //    get
        //    {
        //        return Head.Dir;
        //    }
        //}

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
            /// <summary>
            /// Message with no label.
            /// </summary>
            LOG,
            DEBUG,
            INFORM,
            WARNING,
            ERROR,
            /// <summary>
            /// (!)The app exits right after logging this message.
            /// </summary>
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
                NamedWriter.CloseAll();
                rootDir = null;
                BaseDir = null;
                headSession = null;
            }
        }

        /// <summary>
        ///Directory where logs and log sessions are written.
        /// </summary>
        public static string RootDir
        {
            get
            {
                if (rootDir == null)
                    setRootDir();
                return rootDir;
            }
        }
        static string rootDir = null;
        static Thread deletingOldLogsThread = null;
        /// <summary>
        /// Optional handler used to ask user for the permission to delete old logs.
        /// </summary>
        public static Func<string, bool> DeleteOldLogsDialog = null;
        /// <summary>
        /// Can be checked in the custom code before accessing auto-created objects like Log.Head, Log.Head.Main to prevent creating a root dir when no log is actually in use yet.
        /// It is usually needed before Initialize() or any writing method.
        /// </summary>
        public static bool IsRootDirSet
        {
            get
            {
                return rootDir != null;
            }
        }
        static void setRootDir()
        {
            lock (lockObject)
            {
                bool setBaseDir(string baseDir)
                {
                    BaseDir = baseDir;
                    rootDir = BaseDir + Path.DirectorySeparatorChar + rootDirName + RootDirNameSuffix;
                    try
                    {
                        FileSystemRoutines.CreateDirectory(rootDir);
                        string testFile = rootDir + Path.DirectorySeparatorChar + "test";
                        File.WriteAllText(testFile, "test");
                        File.Delete(testFile);
                        return true;
                    }
                    catch //(Exception e)
                    {
                        rootDir = null;
                        BaseDir = null;
                        return false;
                    }
                }
                if (Log.baseDirs != null)
                    foreach (string baseDir in Log.baseDirs)
                        if (setBaseDir(baseDir))
                            break;
                if (rootDir == null)
                    if (!useDefaultBaseDirs
                        || !setBaseDir(CompanyUserDataDir)
                        || !setBaseDir(CompanyCommonDataDir)
                        || !setBaseDir(Log.AppDir)
                        || !setBaseDir(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory))
                        || !setBaseDir(Path.GetTempPath() + Path.DirectorySeparatorChar + CompanyName + Path.DirectorySeparatorChar)
                        )
                        throw new Exception("Could not write to any of the log base directories.");

                rootDir = PathRoutines.GetNormalizedPath(rootDir, false);
                if (Directory.Exists(rootDir) && deleteLogsOlderThanDays >= 0 && deletingOldLogsThread?.IsAlive != true)
                    deletingOldLogsThread = ThreadRoutines.Start(() => { Log.DeleteOldLogs(deleteLogsOlderThanDays, DeleteOldLogsDialog); });//to avoid a concurrent loop while accessing the log file from the same thread 
            }
        }


        /// <summary>
        ///Actual base directory where RootDir is created.
        /// </summary>
        public static string BaseDir { get; private set; } = null;

        /// <summary>
        /// Creates or retrieves a session-less log writer which allows continuous writing to the same log file in Log.RootDir. 
        /// </summary>
        /// <param name="name">log name</param>
        /// <returns>wirter</returns>
        static public NamedWriter Get(string name)
        {
            return NamedWriter.Get(name);
        }
    }
}


//********************************************************************************************
//Author: Sergey Stoyan
//        sergey.stoyan@gmail.com
//        sergey.stoyan@hotmail.com
//        stoyan@cliversoft.com
//        http://www.cliversoft.com
//********************************************************************************************
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cliver
{
    public static partial class Log
    {
        /// <summary>
        /// Log session.
        /// </summary>
        public partial class Session
        {
            Session(string name)
            {
                this.name = name;
                CreatedTime = DateTime.MinValue;
                timeMark = null;
                string dir = Dir;//initialize
            }

            string getDir(string name)
            {
                lock (this.names2NamedWriter)
                {
                    string dir;
                    if (Log.mode.HasFlag(Mode.FOLDER_PER_SESSION))
                    {
                        string dir0 = BaseDir + System.IO.Path.DirectorySeparatorChar + NamePrefix + "_" + TimeMark + (string.IsNullOrWhiteSpace(name) ? "" : "_" + name);
                        dir = dir0;
                        for (int count = 1; Directory.Exists(dir); count++)
                            dir = dir0 + "_" + count.ToString();
                    }
                    else //if (Log.mode.HasFlag(Mode.ONE_FOLDER))//default
                    {
                        dir = BaseDir;
                    }
                    return dir;
                }
            }

            /// <summary>
            /// Session name prefix in the session directory.
            /// </summary>
            public static string NamePrefix = "Session";

            /// <summary>
            /// Session name.
            /// </summary>
            public string Name
            {
                get
                {
                    lock (this.names2NamedWriter)//this lock is needed if Session::Close(string new_name) is being performed
                    {
                        return name;
                    }
                }
            }
            string name;

            /// <summary>
            /// Session directory.
            /// </summary>
            public string Dir
            {
                get
                {
                    lock (this.names2NamedWriter)//this lock is needed if Session::Close(string new_name) is being performed
                    {
                        if (dir == null)
                            dir = getDir(name);
                        return dir;
                    }
                }
            }
            string dir;

            /// <summary>
            /// Time when the session was created.
            /// </summary>
            public DateTime CreatedTime { get; protected set; }

            /// <summary>
            /// Time mark in the session directory of log names.
            /// </summary>
            public string TimeMark
            {
                get
                {
                    lock (names2NamedWriter)//this lock is needed if Session::Close(string new_name) is being performed
                    {
                        if (timeMark == null)
                        {
                            CreatedTime = DateTime.Now;
                            timeMark = CreatedTime.ToString("yyMMddHHmmss");
                        }
                        return timeMark;
                    }
                }
            }
            string timeMark = null;

            /// <summary>
            /// Default log of the session.
            /// Depending on Mode, it is either Main log or Thread log.
            /// </summary>
            public Writer Default
            {
                get
                {
                    if (mode.HasFlag(Mode.THREAD_DEFAULT_LOG))
                        return Thread;
                    else
                        return Main;
                }
            }

            /// <summary>
            /// Close all log files in the session.  
            /// Nevertheless the session can be re-used after.
            /// </summary>
            /// <param name="newName">new name</param>
            /// <param name="tryMaxCount">number of attempts if the session foldr is locked</param>
            /// <param name="tryDelayMss">time span between attempts</param>
            public void Rename(string newName, int tryMaxCount = 10, int tryDelayMss = 50)
            {
                lock (this.names2NamedWriter)
                {
                    if (newName == Name)
                        return;

                    Write("Renaming session...: '" + Name + "' to '" + newName + "'");

                    for (int tryCount = 1; ; tryCount++)
                        try
                        {
                            Close(true);
                            string newDir = getDir(newName);
                            if (Log.mode.HasFlag(Mode.FOLDER_PER_SESSION))
                            {
                                if (Directory.Exists(dir))
                                    Directory.Move(dir, newDir);
                                dir = newDir;
                                foreach (Writer w in names2NamedWriter.Values.Select(a => (Writer)a).Concat(threadIds2TreadWriter.Values))
                                    w.SetFile();
                            }
                            else //if (Log.mode.HasFlag(Mode.ONE_FOLDER))//default
                            {
                                dir = newDir;
                                foreach (Writer w in names2NamedWriter.Values.Select(a => (Writer)a).Concat(threadIds2TreadWriter.Values))
                                {
                                    string file0 = w.File;
                                    w.SetFile();
                                    if (File.Exists(file0))
                                        File.Move(file0, w.File);
                                }
                            }
                            lock (names2Session)
                            {
                                names2Session.Remove(name);
                                name = newName;
                                names2Session[name] = this;
                            }
                            return;
                        }
                        catch (Exception e)//if another thread calls a log in this session then it locks the folder against renaming
                        {
                            if (tryCount >= tryMaxCount)
                                throw new Exception("Could not rename log session.", e);
                            Error(e);
                            System.Threading.Thread.Sleep(tryDelayMss);
                        }
                }
            }

            /// <summary>
            /// Close all log files in the session. 
            /// </summary>
            /// <param name="reuse">if true, the same session folder can be used again, otherwise a new folder will be created for this session</param>
            public void Close(bool reuse)
            {
                lock (names2NamedWriter)
                {
                    if (names2NamedWriter.Values.FirstOrDefault(a => !a.IsClosed) == null && threadIds2TreadWriter.Values.FirstOrDefault(a => !a.IsClosed) == null)
                        return;

                    Write("Closing the log session...");

                    foreach (NamedWriter nw in names2NamedWriter.Values)
                        nw.Close();
                    //names2NamedWriter.Clear(); !!! clearing writers will bring to duplicating them if they are referenced in the custom code.

                    lock (threadIds2TreadWriter)
                    {
                        foreach (ThreadWriter tw in threadIds2TreadWriter.Values)
                            tw.Close();
                        //threadIds2TreadWriter.Clear(); !!!clearing writers will bring to duplicating them if they are referenced in the custom code.
                    }

                    if (!reuse)
                    {
                        dir = null;
                        CreatedTime = DateTime.MinValue;
                        timeMark = null;
                    }
                }
            }
        }
    }
}
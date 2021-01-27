using System;
using System.Threading;
using Cliver;

namespace Example
{
    class LogExample
    {
        public static void Run()
        {
            try
            {
                Log.Initialize(Log.Mode.EACH_SESSION_IS_IN_OWN_FORLDER);//if permissions allow it, log will be in the bin directory

                Log.Inform("test");

                Log.Session s1 = Log.Session.Get("Name1");//create if no session "Name1"
                Log.Writer nl = s1["Name"];//create if no log "Name"
                nl.Error("to log 'Name'");
                s1.Trace("to the main log of session 'Name1'");
                s1.Thread.Inform("to the thread log of session 'Name1'");
                s1.Rename("Name2");

                //writting thread logs to the default session
                ThreadRoutines.Start(task);
                ThreadRoutines.Start(task);

                //writting thread logs to session Game1
                Log.Session g1 = Log.Session.Get("Game1");
                ThreadRoutines.Start(() => { task2(g1); });
                ThreadRoutines.Start(() => { task2(g1); });
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        static void task()
        {
            try
            {
                Log.Inform0("to default log");
                Log.Thread.Inform0("to thread log");
                throw new Exception2("test exception2");
            }
            catch (Exception e)
            {
                Log.Thread.Error("to thread log", e);
            }
        }

        static void task2(Log.Session logSession)
        {
            try
            {
                logSession.Inform0("to default log");
                logSession.Thread.Inform0("to thread log");
                throw new Exception2("test exception2");
            }
            catch (Exception e)
            {
                logSession.Thread.Error("to thread log", e);
            }
        }
    }
}

using System;
using System.Threading;
using Cliver;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            //optional; initialize log
            Log.Initialize(Log.Mode.EACH_SESSION_IS_IN_OWN_FORLDER);//if permissions allow it, log will be in the bin directory

            try
            {
                //initialize settings
                Config.Reload();

                LogExample.Run();

                ConfigExample.Run();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public static void Email(string host, int port, string password, string message)
        {
            Log.Inform("sent message:\r\n" + message);
        }
    }
}

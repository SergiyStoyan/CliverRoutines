using System;
using System.Threading;
using Cliver;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //SleepRoutines.WaitForCondition(()=> { return false; }, 2000, 1000, true,3);


                Log.Inform("\r\n" + Log.GetAssembliesInfo(nameof(Cliver)));

                LogExample.Run();

                ConfigExample.Run();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}

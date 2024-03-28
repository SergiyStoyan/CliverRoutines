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
                //SleepRoutines.WaitForCondition2(()=> { return false; }, 2, 1000, true);


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

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

                string s = Log.GetAssembliesInfo(AssemblyRoutines.TypeLoadMode.MustLoad, nameof(Cliver)/*, nameof(Example)*/);
                Log.Inform("\r\n" + s);

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

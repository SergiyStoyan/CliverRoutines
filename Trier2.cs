//********************************************************************************************
//Author: Sergiy Stoyan
//        s.y.stoyan@gmail.com, sergiy.stoyan@outlook.com, stoyan@cliversoft.com
//        http://www.cliversoft.com
//********************************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cliver
{
    public class Trier2
    {
        virtual protected bool retryOnException(Exception e)
        {
            throw new NotImplementedException();

            List<Type> retriableExceptionTypes = new List<Type>();
            for (; e != null; e = e.InnerException)
                if (retriableExceptionTypes?.Find(a => e.GetType() == a) != null)
                    return true;
            return false;
        }

        virtual public int DefaultTryMaxNumber { get; } = 3;
        virtual public int DefaultRetryDelayMss { get; } = 10000;

        virtual public T Run<T>(string logMessage, Func<T> function, int maxTryNumber = -1, int retryDelayMss = -1) where T : class
        {
            if (maxTryNumber < 0)
                maxTryNumber = DefaultTryMaxNumber;
            if (retryDelayMss < 0)
                retryDelayMss = DefaultRetryDelayMss;
            if (logMessage != null)
                Log.Inform(logMessage);
            T o = SleepRoutines.WaitForObject(
                () =>
                {
                    try
                    {
                        return function();
                    }
                    catch (Exception e)
                    {
                        if (retryOnException(e))
                            return null;
                        throw;
                    }
                },
                0, retryDelayMss, false, maxTryNumber
            );
            if (o == null)
            {
                string m = logMessage != null ? Regex.Replace(logMessage, @"\.\.\.", "") : nameof(Trier2) + "." + nameof(Run) + "()";
                throw new Exception2("Failed: " + m);
            }
            return o;
        }

        virtual public T Run<T>(Func<T> function, int maxTryNumber = -1, int retryDelayMss = -1) where T : class
        {
            return Run(null, function, maxTryNumber, retryDelayMss);
        }

        virtual public void Run(string logMessage, Action action, int maxTryNumber = -1, int retryDelayMss = -1)
        {
            Run(logMessage, () => { action(); return new Object(); }, maxTryNumber, retryDelayMss);
        }

        virtual public void Run(Action action, int maxTryNumber = -1, int retryDelayMss = -1)
        {
            Run(null, action, maxTryNumber, retryDelayMss);
        }
    }
}


//********************************************************************************************
//Author: Sergiy Stoyan
//        s.y.stoyan@gmail.com, sergiy.stoyan@outlook.com, stoyan@cliversoft.com
//        http://www.cliversoft.com
//********************************************************************************************
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cliver
{
    public class Trier3
    {
        public virtual int DefaultMaxTryNumber { get; } = 3;

        public virtual int DefaultRetryDelayMss { get; } = 10000;

        public virtual bool DefaultExceptionOnTimeout { get; } = true;

        protected virtual bool retryOnException(Exception e)
        {
            return false;
        }

        protected virtual void onStart(string message) { }

        protected virtual void onTimeout<T>(ref T result, string message, bool exceptionOnTimeout) where T : class
        {
            if (exceptionOnTimeout)
                throw new Exception2("Timeout [" + message + "] by " + Log.GetThisMethodInfo());
        }

        virtual public T Run<T>(string message, Func<T> function, bool? exceptionOnTimeout = null, int maxTryNumber = -1, int retryDelayMss = -1) where T : class
        {
            if (maxTryNumber < 0)
                maxTryNumber = DefaultMaxTryNumber;
            if (retryDelayMss < 0)
                retryDelayMss = DefaultRetryDelayMss;
            if (exceptionOnTimeout == null)
                exceptionOnTimeout = DefaultExceptionOnTimeout;

            onStart(message);

            T o = null;
            bool r = SleepRoutines.WaitForCondition(
                () =>
                {
                    try
                    {
                        o = function();
                        return true;
                    }
                    catch (Exception e)
                    {
                        if (!retryOnException(e))
                            throw;
                        return false;
                    }
                },
                0, retryDelayMss, pollSpanStartsBeforeConditionCheck: false, maxTryNumber
            );
            if (!r)
                onTimeout(ref o, message, exceptionOnTimeout.Value);
            return o;
        }

        virtual public T Run<T>(Func<T> function, bool? exceptionOnTimeout = null, int maxTryNumber = -1, int retryDelayMss = -1) where T : class
        {
            return Run(null, function, exceptionOnTimeout, maxTryNumber, retryDelayMss);
        }

        virtual public void Run(string message, Action action, bool? exceptionOnTimeout = null, int maxTryNumber = -1, int retryDelayMss = -1)
        {
            Run(message, () => { action(); return (object)null; }, exceptionOnTimeout, maxTryNumber, retryDelayMss);
        }

        virtual public void Run(Action action, bool? exceptionOnTimeout = null, int maxTryNumber = -1, int retryDelayMss = -1)
        {
            Run(null, action, exceptionOnTimeout, maxTryNumber, retryDelayMss);
        }

        virtual public async Task<T> RunAsync<T>(string message, Func<T> function, bool? exceptionOnTimeout = null, int maxTryNumber = -1, int retryDelayMss = -1) where T : class
        {
            Task<T> t = new Task<T>(() => { return Run(message, function, exceptionOnTimeout, maxTryNumber, retryDelayMss); });
            t.Start();
            return await t;
        }

        virtual public async Task<T> RunAsync<T>(Func<T> function, bool? exceptionOnTimeout = null, int maxTryNumber = -1, int retryDelayMss = -1) where T : class
        {
            return await RunAsync(null, function, exceptionOnTimeout, maxTryNumber, retryDelayMss);
        }

        virtual public async Task RunAsync(string message, Action action, bool? exceptionOnTimeout = null, int maxTryNumber = -1, int retryDelayMss = -1)
        {
            await RunAsync(message, () => { action(); return (object)null; }, exceptionOnTimeout, maxTryNumber, retryDelayMss);
        }

        virtual public async Task RunAsync(Action action, bool? exceptionOnTimeout = null, int maxTryNumber = -1, int retryDelayMss = -1)
        {
            await RunAsync(null, action, exceptionOnTimeout, maxTryNumber, retryDelayMss);
        }
    }
}
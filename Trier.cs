//********************************************************************************************
//Author: Sergiy Stoyan
//        s.y.stoyan@gmail.com, sergiy.stoyan@outlook.com, stoyan@cliversoft.com
//        http://www.cliversoft.com
//********************************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Cliver
{
    /// <summary>
    /// The basic trier.
    /// </summary>
    public class TrierBase
    {
        public virtual int MaxTryNumber { get; set; } = 3;

        public virtual int RetryDelayMss { get; set; } = 10000;

        protected virtual bool retryOnException(Exception e)
        {
            return false;
        }

        public virtual bool Success { get; protected set; } = false;

        public virtual TimeSpan Delay { get; set; } = new TimeSpan(0, 0, 0);

        public DateTime NextRunTime { get; protected set; } = new DateTime(0);

        virtual public T Run<T>(Func<T> function) where T : class
        {
            var delay = NextRunTime - DateTime.Now;
            if (delay > TimeSpan.Zero)
                Thread.Sleep(delay);
            T result = null;
            Success = SleepRoutines.WaitForCondition(
                () =>
                {
                    try
                    {
                        result = function();
                        return true;
                    }
                    catch (Exception e)
                    {
                        if (!retryOnException(e))
                            throw;
                        return false;
                    }
                },
                0, RetryDelayMss, pollSpanStartsBeforeConditionCheck: false, MaxTryNumber
            );
            NextRunTime = DateTime.Now.Add(Delay);
            return result;
        }
    }

    /// <summary>
    /// The most versatile trier that can be used either as is or be inherited.
    /// </summary>
    public class Trier : TrierBase
    {
        public string Message { get; protected set; }

        public bool ExceptionOnUnsuccess = true;

        protected virtual void onStart() {/*logging*/ }

        protected virtual void onEnd()
        {/*logging*/
            if (!Success && ExceptionOnUnsuccess)
                throw new UnsuccessException();
        }

        public class UnsuccessException : Exception { }

        virtual public T Run<T>(string message, Func<T> function) where T : class
        {
            Message = message;
            onStart();
            T result = base.Run(function);
            onEnd();
            return result;
        }

        virtual public T Run<T>(string message, Func<Task<T>> functionAsync) where T : class
        {
            return Run(message, () => TaskRoutines.RunSync(functionAsync));
        }

        virtual public async Task<T> RunAsync<T>(string message, Func<T> function) where T : class
        {
            Message = message;
            onStart();
            //T result = await base.RunAsync(function);
            Task<T> t = new Task<T>(() => Run(function));
            t.Start();
            T result = await t;
            onEnd();
            return result;
        }

        virtual public Task<T> RunAsync<T>(string message, Func<Task<T>> functionAsync) where T : class
        {
            return RunAsync(message, () => TaskRoutines.RunSync(functionAsync));
        }

        virtual public T Run<T>(Func<T> function) where T : class
        {
            return Run(null, function);
        }

        virtual public T Run<T>(Func<Task<T>> functionAsync) where T : class
        {
            return Run(null, functionAsync);
        }

        virtual public async Task<T> RunAsync<T>(Func<T> function) where T : class
        {
            return await RunAsync(null, function);
        }

        virtual public Task<T> RunAsync<T>(Func<Task<T>> functionAsync) where T : class
        {
            return RunAsync(null, () => TaskRoutines.RunSync(functionAsync));
        }

        #region Actions
        virtual public void Run(string message, Action action)
        {
            Run(message, () => { action(); return (object)null; });
        }

        virtual public void Run(string message, Func<Task> actionAsync)
        {
            Run(message, () => TaskRoutines.RunSync(actionAsync));
        }

        virtual public async Task RunAsync(string message, Action action)
        {
            await RunAsync(message, () => { action(); return (object)null; });
        }

        virtual public Task RunAsync<T>(string message, Func<Task> actionAsync) where T : class
        {
            return RunAsync(message, () => TaskRoutines.RunSync(actionAsync));
        }

        virtual public void Run(Action action)
        {
            Run(null, action);
        }

        virtual public void Run(Func<Task> actionAsync)
        {
            Run(null, () => TaskRoutines.RunSync(actionAsync));
        }

        virtual public async Task RunAsync(Action action)
        {
            await RunAsync(null, action);
        }

        virtual public Task RunAsync(string message, Func<Task> actionAsync)
        {
            return RunAsync(null, actionAsync);
        }
        #endregion
    }

    /// <summary>
    /// An easy trier that can be configured on fly.
    /// </summary>
    public class Trier2 : Trier
    {
        public Func<Exception, bool> RetryOnException;

        public Action<string> OnMessage;

        public Action<bool> OnEnd;

        protected override bool retryOnException(Exception e)
        {
            return RetryOnException?.Invoke(e) == true;
        }

        protected override void onStart()
        {
            OnMessage?.Invoke(Message);
        }

        protected override void onEnd()
        {
            OnEnd?.Invoke(Success);
        }
    }

    ///// <summary>
    ///// Trier adapted for web requests
    ///// </summary>
    //public class WebTrier : Trier
    //{
    //    virtual public HashSet<System.Net.HttpStatusCode> RetriableHttpCodes { get; } = new HashSet<System.Net.HttpStatusCode> {
    //        System.Net.HttpStatusCode.InternalServerError,
    //        System.Net.HttpStatusCode.Gone,
    //        System.Net.HttpStatusCode.BadRequest,
    //    };

    //    protected override bool retryOnException(Exception e)
    //    {
    //        for (; e != null; e = e.InnerException)
    //            if (e is System.Net.WebException ex && ex.Response is System.Net.HttpWebResponse hr && RetriableHttpCodes.Contains(hr.StatusCode))
    //            {
    //                //Log.Warning2("Retrying...\r\n" + Message, e);
    //                return true;
    //            }
    //        return false;
    //    }

    //    //protected override void onStart()
    //    //{
    //    //    Log.Inform(Message);
    //    //}

    //    //protected override void onEnd()
    //    //{
    //    //    if (!Success && ExceptionOnUnsuccess)
    //    //    {
    //    //        string m = Message != null ? Regex.Replace(Message, @"\.\.\.", "") : nameof(GoogleTrier) + "." + nameof(Run) + "()";
    //    //        throw new Exception2("Failed: " + m);
    //    //    }
    //    //}
    //}
}
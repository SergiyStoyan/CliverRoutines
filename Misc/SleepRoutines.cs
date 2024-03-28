//********************************************************************************************
//Author: Sergiy Stoyan
//        s.y.stoyan@gmail.com, sergiy.stoyan@outlook.com, stoyan@cliversoft.com
//        http://www.cliversoft.com
//********************************************************************************************
using System;
using System.Threading;
using System.Collections.Generic;


namespace Cliver
{
    public static class SleepRoutines
    {
        /// <summary>
        /// Always polls at least 1 time.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="timeoutMss"></param>
        /// <param name="pollSpanMss"></param>
        /// <param name="pollSpanStartsBeforeConditionCheck"></param>
        /// <param name="pollMinNumber"></param>
        /// <returns></returns>
        public static bool WaitForCondition(Func<bool> condition, int timeoutMss, int pollSpanMss, bool pollSpanStartsBeforeConditionCheck = false, int pollMinNumber = -1)
        {
            int pollNumber = 0;
            if (pollSpanStartsBeforeConditionCheck)
                for (DateTime lastDt = DateTime.Now.AddMilliseconds(timeoutMss); ;)
                {
                    DateTime nextPollTime = DateTime.Now.AddMilliseconds(pollSpanMss);
                    if (condition())
                        return true;
                    pollNumber++;
                    if ((DateTime.Now > lastDt || nextPollTime > lastDt)
                        && (pollNumber >= pollMinNumber)
                        )
                        return false;
                    int mss = (int)(nextPollTime - DateTime.Now).TotalMilliseconds;
                    if (mss > 0)
                        Thread.Sleep(mss);
                }
            else
                for (DateTime lastDt = DateTime.Now.AddMilliseconds(timeoutMss); ;)
                {
                    if (condition())
                        return true;
                    pollNumber++;
                    if (DateTime.Now.AddMilliseconds(pollSpanMss) > lastDt
                        && (pollNumber >= pollMinNumber)
                        )
                        return false;
                    Thread.Sleep(pollSpanMss);
                }
        }

        ///// <summary>        
        ///// Always polls at least 1 time.
        ///// </summary>
        ///// <param name="condition"></param>
        ///// <param name="pollNumber"></param>
        ///// <param name="pollSpanMss"></param>
        ///// <param name="pollSpanStartsBeforeConditionCheck"></param>
        ///// <returns></returns>
        //public static bool WaitForCondition2(Func<bool> condition, int pollNumber, int pollSpanMss, bool pollSpanStartsBeforeConditionCheck = false)
        //{
        //    //if (pollNumber < 1)
        //    //    throw new Exception(nameof(pollNumber) + " cannot be < 1.");
        //    if (pollSpanStartsBeforeConditionCheck)
        //        for (int i = 0; ;)
        //        {
        //            DateTime nextPollTime = DateTime.Now.AddMilliseconds(pollSpanMss);
        //            if (condition())
        //                return true;
        //            if (++i >= pollNumber)
        //                return false;
        //            int mss = (int)(nextPollTime - DateTime.Now).TotalMilliseconds;
        //            if (mss > 0)
        //                Thread.Sleep(mss);
        //        }
        //    else
        //        for (int i = 0; ;)
        //        {
        //            if (condition())
        //                return true;
        //            if (++i >= pollNumber)
        //                return false;
        //            Thread.Sleep(pollSpanMss);
        //        }
        //}

        /// <summary>
        /// Always polls at least 1 time.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="getObject"></param>
        /// <param name="timeoutMss"></param>
        /// <param name="pollSpanMss"></param>
        /// <param name="pollSpanStartsBeforeConditionCheck"></param>
        /// <param name="pollMinNumber"></param>
        /// <returns></returns>
        public static T WaitForObject<T>(Func<T> getObject, int timeoutMss, int pollSpanMss, bool pollSpanStartsBeforeConditionCheck = false, int pollMinNumber = -1) where T : class
        {
            T o = null;
            WaitForCondition(() =>
            {
                o = getObject();
                return o != null;
            }, timeoutMss, pollSpanMss, pollSpanStartsBeforeConditionCheck, pollMinNumber);
            return o;
        }

        ///// <summary>
        ///// Always polls at least 1 time.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="getObject"></param>
        ///// <param name="pollNumber"></param>
        ///// <param name="pollSpanMss"></param>
        ///// <param name="pollSpanStartsBeforeConditionCheck"></param>
        ///// <returns></returns>
        //public static T WaitForObject2<T>(Func<T> getObject, int pollNumber, int pollSpanMss, bool pollSpanStartsBeforeConditionCheck = false) where T : class
        //{
        //    T o = null;
        //    WaitForCondition2(() =>
        //    {
        //        o = getObject();
        //        return o != null;
        //    }, pollNumber, pollSpanMss, pollSpanStartsBeforeConditionCheck);
        //    return o;
        //}
    }
}


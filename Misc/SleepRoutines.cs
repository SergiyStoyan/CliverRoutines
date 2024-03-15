﻿//********************************************************************************************
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
        public static bool WaitForCondition(Func<bool> condition, int timeoutMss, int pollSpanMss = 10)
        {
            DateTime dt = DateTime.Now + new TimeSpan(0, 0, 0, 0, timeoutMss);
            for (; ; )
            {
                if (condition())
                    return true;
                if (DateTime.Now > dt)
                    return false;
                Thread.Sleep(pollSpanMss);
            }
        }

        public static T WaitForObject<T>(Func<T> getObject, int timeoutMss, int pollSpanMss = 10) where T : class
        {
            T o = null;
            WaitForCondition(() =>
            {
                o = getObject();
                return o != null;
            }, timeoutMss, pollSpanMss);
            return o;
        }

        //public static void Wait(int mss, int pollSpanMss = 10)
        //{
        //    DateTime dt = DateTime.Now + new TimeSpan(0, 0, 0, 0, mss);
        //    while (DateTime.Now > dt)
        //        Thread.Sleep(pollSpanMss);
        //}
    }
}


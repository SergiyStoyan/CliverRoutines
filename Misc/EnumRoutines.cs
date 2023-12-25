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
    public static class EnumRoutines
    {
        public static IEnumerable<T> GetValues<T>() where T : Enum
        {
            foreach (T v in Enum.GetValues(typeof(T)))
                yield return v;
        }
    }
}


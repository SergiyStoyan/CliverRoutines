//********************************************************************************************
//Author: Sergiy Stoyan
//        s.y.stoyan@gmail.com, sergiy.stoyan@outlook.com, stoyan@cliversoft.com
//        http://www.cliversoft.com
//********************************************************************************************
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Cliver
{
    public static partial class DateTimeRoutines
    {
        /// <summary>
        /// Amount of seconds elapsed between 1970-01-01 00:00:00 and the dateTime.
        /// </summary>
        /// <param name="dateTime">date-time</param>
        /// <returns>seconds</returns>
        public static uint GetSecondsSinceUnixEpoch(this DateTime dateTime)
        {
            //if (dateTime.Kind == DateTimeKind.Local)
            //    dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            return (uint)new DateTimeOffset(dateTime).ToUnixTimeSeconds();
        }

        /// <summary>
        /// Amount of milliseconds elapsed between 1970-01-01 00:00:00 and the dateTime.
        /// </summary>
        /// <param name="dateTime">date-time</param>
        /// <returns>seconds</returns>
        public static ulong GetMillisecondsSinceUnixEpoch(this DateTime dateTime)
        {
            return (ulong)new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// DateTime defined by seconds elapsed between 1970-01-01 00:00:00.
        /// </summary>
        /// <param name="secondsSinceUnixEpoch"></param>
        /// <returns></returns>
        public static DateTimeOffset GetDateTimeFromSecondsSinceUnixEpoch(uint secondsSinceUnixEpoch)
        {
            return new DateTimeOffset(1970, 01, 01, 00, 00, 00, TimeSpan.Zero).AddSeconds(secondsSinceUnixEpoch);
        }

        /// <summary>
        /// DateTime defined by milliseconds elapsed between 1970-01-01 00:00:00.
        /// </summary>
        /// <param name="millisecondsSinceUnixEpoch"></param>
        /// <returns></returns>
        public static DateTimeOffset GetDateTimeFromMillisecondsSinceUnixEpoch(ulong millisecondsSinceUnixEpoch)
        {
            return new DateTimeOffset(1970, 01, 01, 00, 00, 00, TimeSpan.Zero).AddMilliseconds(millisecondsSinceUnixEpoch);
        }
    }
}
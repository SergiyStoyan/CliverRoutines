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
        /// Tries to find date and time within the passed string and return it as DateTime structure. 
        /// </summary>
        /// <param name="str">string that contains date and/or time</param>
        /// <param name="formats">formats to be applied</param>
        /// <param name="dateTime">parsed date-time output</param>
        /// <returns>true if both date and time were found, else false</returns>
        static public bool TryParseDateTime(string str, DateTimeFormat formats, out DateTime dateTime)
        {
            var orderedFormats = getOrderedFormats(formats);
            return TryParseDateTime(str, orderedFormats, out dateTime);
        }

        /// <summary>
        /// Tries to find date and time within the passed string and return it as DateTime structure. 
        /// </summary>
        /// <param name="str">string that contains date and/or time</param>
        /// <param name="orderedFormats">accepted formats in the applicable order</param>
        /// <param name="dateTime">parsed date-time output</param>
        /// <returns>true if both date and time were found, else false</returns>
        static public bool TryParseDateTime(string str, IEnumerable<DateTimeFormat> orderedFormats, out DateTime dateTime)
        {
            ParsedDateTime parsedDateTime;
            if (!TryParseDateTime(str, orderedFormats, out parsedDateTime))
            {
                dateTime = new DateTime(1, 1, 1);
                return false;
            }
            dateTime = parsedDateTime.DateTime;
            return true;
        }

        /// <summary>
        /// Tries to find date and/or time within the passed string and return it as DateTime structure. 
        /// If only date was found, time in the returned DateTime is always 0:0:0.
        /// If only time was found, date in the returned DateTime is DefaultDate.
        /// </summary>
        /// <param name="str">string that contains date and(or) time</param>
        /// <param name="formats">formats to be applied</param>
        /// <param name="dateTime">parsed date-time output</param>
        /// <returns>true if date and/or time was found, else false</returns>
        static public bool TryParseDateOrTime(string str, DateTimeFormat formats, out DateTime dateTime)
        {
            var orderedFormats = getOrderedFormats(formats);
            return TryParseDateOrTime(str, orderedFormats, out dateTime);
        }

        /// <summary>
        /// Tries to find date and/or time within the passed string and return it as DateTime structure. 
        /// If only date was found, time in the returned DateTime is always 0:0:0.
        /// If only time was found, date in the returned DateTime is DefaultDate.
        /// </summary>
        /// <param name="str">string that contains date and(or) time</param>
        /// <param name="orderedFormats">accepted formats in the applicable order</param>
        /// <param name="dateTime">parsed date-time output</param>
        /// <returns>true if date and/or time was found, else false</returns>
        static public bool TryParseDateOrTime(string str, IEnumerable<DateTimeFormat> orderedFormats, out DateTime dateTime)
        {
            ParsedDateTime parsedDateTime;
            if (!TryParseDateOrTime(str, orderedFormats, out parsedDateTime))
            {
                dateTime = new DateTime(1, 1, 1);
                return false;
            }
            dateTime = parsedDateTime.DateTime;
            return true;
        }

        /// <summary>
        /// Tries to find time within the passed string and return it as DateTime structure. 
        /// It recognizes only time while ignoring date, so date in the returned DateTime is always 1/1/1.
        /// </summary>
        /// <param name="str">string that contains time</param>
        /// <param name="formats">formats to be applied</param>
        /// <param name="time">parsed time output</param>
        /// <returns>true if time was found, else false</returns>
        public static bool TryParseTime(string str, DateTimeFormat formats, out DateTime time)
        {
            var orderedFormats = getOrderedFormats(formats);
            return TryParseTime(str, orderedFormats, out time);
        }

        /// <summary>
        /// Tries to find time within the passed string and return it as DateTime structure. 
        /// It recognizes only time while ignoring date, so date in the returned DateTime is always 1/1/1.
        /// </summary>
        /// <param name="str">string that contains time</param>
        /// <param name="orderedFormats">accepted formats in the applicable order</param>
        /// <param name="time">parsed time output</param>
        /// <returns>true if time was found, else false</returns>
        public static bool TryParseTime(string str, IEnumerable<DateTimeFormat> orderedFormats, out DateTime time)
        {
            ParsedDateTime parsedTime;
            if (!TryParseTime(str, orderedFormats, out parsedTime, null))
            {
                time = new DateTime(1, 1, 1);
                return false;
            }
            time = parsedTime.DateTime;
            return true;
        }

        /// <summary>
        /// Tries to find date within the passed string and return it as DateTime structure. 
        /// It recognizes only date while ignoring time, so time in the returned DateTime is always 0:0:0.
        /// If year of the date was not found then it accepts the current year. 
        /// </summary>
        /// <param name="str">string that contains date</param>
        /// <param name="formats">formats to be applied</param>
        /// <param name="date">parsed date output</param>
        /// <returns>true if date was found, else false</returns>
        static public bool TryParseDate(string str, DateTimeFormat formats, out DateTime date)
        {
            var orderedFormats = getOrderedFormats(formats);
            return TryParseDate(str, orderedFormats, out date);
        }

        /// <summary>
        /// Tries to find date within the passed string and return it as DateTime structure. 
        /// It recognizes only date while ignoring time, so time in the returned DateTime is always 0:0:0.
        /// If year of the date was not found then it accepts the current year. 
        /// </summary>
        /// <param name="str">string that contains date</param>
        /// <param name="orderedFormats">accepted formats in the applicable order</param>
        /// <param name="date">parsed date output</param>
        /// <returns>true if date was found, else false</returns>
        static public bool TryParseDate(string str, IEnumerable<DateTimeFormat> orderedFormats, out DateTime date)
        {
            ParsedDateTime parsedDate;
            if (!TryParseDate(str, orderedFormats, out parsedDate))
            {
                date = new DateTime(1, 1, 1);
                return false;
            }
            date = parsedDate.DateTime;
            return true;
        }
    }
}
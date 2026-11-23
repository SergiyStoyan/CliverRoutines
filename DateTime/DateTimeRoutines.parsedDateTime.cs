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
        /// Tries to find date and time within the passed string and return it as ParsedDateTime object. 
        /// </summary>
        /// <param name="str">string that contains date-time</param>
        /// <param name="orderedFormats">accepted formats in the applicable order</param>
        /// <param name="parsedDateTime">parsed date-time output</param>
        /// <returns>true if both date and time were found, else false</returns>
        static public bool TryParseDateTime(string str, IEnumerable<DateTimeFormat> orderedFormats, out ParsedDateTime parsedDateTime)
        {
            if (DateTimeRoutines.TryParseDateOrTime(str, orderedFormats, out parsedDateTime)
                && parsedDateTime.IsDateFound
                && parsedDateTime.IsTimeFound
                )
                return true;

            parsedDateTime = null;
            return false;
        }

        /// <summary>
        /// Tries to find time within the passed string and return it as ParsedDateTime object. 
        /// It recognizes only time while ignoring date, so date in the returned ParsedDateTime is always 1/1/1
        /// </summary>
        /// <param name="str">string that contains date-time</param>
        /// <param name="orderedFormats">accepted formats in the applicable order</param>
        /// <param name="parsedTime">parsed date-time output</param>
        /// <returns>true if time was found, else false</returns>
        static public bool TryParseTime(string str, IEnumerable<DateTimeFormat> orderedFormats, out ParsedDateTime parsedTime)
        {
            return TryParseTime(str, orderedFormats, out parsedTime, null);
        }

        /// <summary>
        /// Tries to find date and/or time within the passed string and return it as ParsedDateTime object. 
        /// If only date was found, time in the returned ParsedDateTime is always 0:0:0.
        /// If only time was found, date in the returned ParsedDateTime is DefaultDate.
        /// </summary>
        /// <param name="str">string that contains date-time</param>
        /// <param name="orderedFormats">accepted formats in the applicable order</param>
        /// <param name="parsedDateTime">parsed date-time output</param>
        /// <returns>true if date or time was found, else false</returns>
        static public bool TryParseDateOrTime(string str, IEnumerable<DateTimeFormat> orderedFormats, out ParsedDateTime parsedDateTime)
        {
            parsedDateTime = null;

            ParsedDateTime parsedTime;
            if (TryParseDate(str, orderedFormats, out ParsedDateTime parsedDate))
            {
                if (parsedDate.Format != null)
                    TryParseTime(str, new List<DateTimeFormat> { parsedDate.Format.Value }, out parsedTime, parsedDate);
                else
                    TryParseTime(str, orderedFormats, out parsedTime, parsedDate);
                if (parsedTime != null)
                {
                    //if (parsedTime.Format != DateTimeFormat._UNDETECTED && parsedDate.Format != parsedTime.Format)
                    //    throw new Exception("");
                    DateTime dateTime = new DateTime(parsedDate.DateTime.Year, parsedDate.DateTime.Month, parsedDate.DateTime.Day, parsedTime.DateTime.Hour, parsedTime.DateTime.Minute, parsedTime.DateTime.Second);
                    parsedDateTime = new ParsedDateTime(parsedDate.IndexOfDate, parsedDate.LengthOfDate, parsedTime.IndexOfTime, parsedTime.LengthOfTime, dateTime, parsedTime.UtcOffset, parsedTime.Format ?? parsedDate.Format);
                }
                else
                    parsedDateTime = new ParsedDateTime(parsedDate.IndexOfDate, parsedDate.LengthOfDate, -1, -1, parsedDate.DateTime, null, parsedDate.Format);
            }
            else
            {
                if (!TryParseTime(str, orderedFormats, out parsedTime, null))
                    return false;

                DateTime dateTime = new DateTime(DefaultDate.Year, DefaultDate.Month, DefaultDate.Day, parsedTime.DateTime.Hour, parsedTime.DateTime.Minute, parsedTime.DateTime.Second);
                parsedDateTime = new ParsedDateTime(-1, -1, parsedTime.IndexOfTime, parsedTime.LengthOfTime, dateTime, parsedTime.UtcOffset, parsedTime.Format);
            }

            return true;
        }
    }
}
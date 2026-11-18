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
    /// <summary>
    /// Miscellaneous and parsing methods for DateTime
    /// </summary>
    public static partial class DateTimeRoutines
    {
        /// <summary>
        /// Defines a substring where date-time was found and result of conversion
        /// </summary>
        public class ParsedDateTime
        {
            /// <summary>
            /// Position of the date substring found
            /// </summary>
            readonly public int IndexOfDate = -1;
            /// <summary>
            /// Length of the date substring found
            /// </summary>
            readonly public int LengthOfDate = -1;
            /// <summary>
            /// Position of the time substring found
            /// </summary>
            readonly public int IndexOfTime = -1;
            /// <summary>
            /// Length of the time substring found
            /// </summary>
            readonly public int LengthOfTime = -1;
            /// <summary>
            /// Position of the substring containing the detected date and/or time
            /// </summary>
            readonly public int IndexOfStart = -1;
            /// <summary>
            /// Length of the substring containing the detected date and/or time
            /// </summary>
            readonly public int LengthOfDateTime = -1;
            /// <summary>
            /// Position of the string remainder behind the detected date and time
            /// </summary>
            readonly public int IndexOfRemainder = -1;
            /// <summary>
            /// DateTime detected
            /// </summary>
            readonly public DateTime DateTime;
            /// <summary>
            /// True if a date was found, False if it was constructed
            /// </summary>
            readonly public bool IsDateFound = false;
            /// <summary>
            /// True if a time was found, False if it was constructed
            /// </summary>
            readonly public bool IsTimeFound = false;
            /// <summary>
            /// UTC offset if it was found
            /// </summary>
            readonly public TimeSpan? UtcOffset = null;
            //public bool IsUtcOffsetFound { get { return UtcOffset != null; } }
            public readonly DateTime? UtcDateTime = null;
            /// <summary>
            /// Detected format
            /// </summary>
            public readonly DateTimeFormat? Format = null;

            internal ParsedDateTime(int indexOfDate, int lengthOfDate, int indexOfTime, int lengthOfTime, DateTime dateTime, TimeSpan? utcOffset, DateTimeFormat? format)
            {
                IndexOfDate = indexOfDate;
                LengthOfDate = lengthOfDate;
                IndexOfTime = indexOfTime;
                LengthOfTime = lengthOfTime;
                IndexOfStart = IndexOfDate < IndexOfTime ? IndexOfDate : IndexOfTime;
                int dl = IndexOfDate + LengthOfDate;
                int tl = IndexOfTime + LengthOfTime;
                IndexOfRemainder = dl < tl ? tl : dl;
                LengthOfDateTime = IndexOfRemainder - IndexOfStart;
                IsDateFound = indexOfDate > -1;
                IsTimeFound = indexOfTime > -1;
                Format = format;

                if (utcOffset != null && Math.Abs(utcOffset.Value.TotalHours) < 12)
                    UtcOffset = utcOffset.Value;
                if (UtcOffset == null)
                {
                    DateTime = dateTime;
                    if (DateTime.Kind == DateTimeKind.Utc)
                        UtcDateTime = DateTime;
                }
                else
                {
                    DateTime = new DateTime(dateTime.Ticks, DateTimeKind.Utc);
                    if (indexOfDate < 0)//to avoid negative date exception when date is undefined
                    {
                        TimeSpan ts = dateTime.TimeOfDay + UtcOffset.Value;
                        if (ts < new TimeSpan(0))
                            UtcDateTime = new DateTime(1, 1, 2) + ts;
                        else
                            UtcDateTime = new DateTime(1, 1, 1) + ts;
                    }
                    else
                        UtcDateTime = dateTime + UtcOffset;
                }
            }
        }

        /// <summary>
        /// Date that is used in the following cases:
        /// - no date was parsed by TryParseDateOrTime();
        /// - no year was found by TryParseDate();
        /// - no century was found by TryParseDate();
        /// It is ignored if DefaultDateIsNow = true was set after DefaultDate 
        /// </summary>
        public static DateTime DefaultDate
        {
            set
            {
                _DefaultDate = value;
                DefaultDateIsNow = false;
            }
            get
            {
                if (DefaultDateIsNow)
                    return DateTime.Now;
                else
                    return _DefaultDate;
            }
        }
        static DateTime _DefaultDate = DateTime.Now;

        /// <summary>
        /// If true then DefaultDate property is ignored and DefaultDate is always DateTime.Now
        /// </summary>
        public static bool DefaultDateIsNow = true;

        /// <summary>
        /// Defines default date-time format.
        /// </summary>
        public enum DateTimeFormat
        {
            ///// <summary>
            ///// (!)It must not be used as an input parameter.
            ///// </summary>
            //_UNDETECTED = 0,
            /// <summary>
            /// Month number goes before day number
            /// </summary>
            USA_DATE = 1,
            /// <summary>
            /// Day number goes before month number
            /// </summary>
            UK_DATE = 2,
            ///// <summary>
            ///// time is specifed through AM or PM
            ///// </summary>
            //USA_TIME,
        }

        /// <summary>
        /// Tries to find time within the passed string (relatively to the passed parsedDate if any) and return it as ParsedDateTime object.
        /// It recognizes only time while ignoring date, so date in the returned ParsedDateTime is always 1/1/1
        /// </summary>
        /// <param name="str">string that contains date</param>
        /// <param name="orderedFormats">accepted formats in the applicable order</param>
        /// <param name="parsedTime">parsed date-time output</param>
        /// <param name="parsedDate">ParsedDateTime object if the date was found within this string, else NULL</param>
        /// <returns>true if time was found, else false</returns>
        /// <exception cref="Exception"></exception>
        static public bool TryParseTime(string str, IEnumerable<DateTimeFormat> orderedFormats, out ParsedDateTime parsedTime, ParsedDateTime parsedDate)
        {
            foreach (var f in orderedFormats)
                if (tryParseTime(str, f, out parsedTime, parsedDate))
                    return true;
            parsedTime = null;
            return false;
        }

        static bool tryParseTime(string str, DateTimeFormat format, out ParsedDateTime parsedTime, ParsedDateTime parsedDate)
        {
            parsedTime = null;

            string timeZoneR;
            if (format == DateTimeFormat.UK_DATE)
                timeZoneR = (@"\s*(?'ukTimeZone'Z|UTC|GMT)");
            else if (format == DateTimeFormat.USA_DATE)
                timeZoneR = (@"\s*(?'usTimeZone'Z|UTC|GMT|CST|EST)");
            else
                throw new Exception("Unknown format: " + format);
            timeZoneR = @"(?:" + timeZoneR + ")?";

            Match m;
            int timeIndex = -1;
            if (parsedDate != null && parsedDate.IndexOfDate > -1)
            {//look around the found date
                string ts = str.Substring(parsedDate.IndexOfDate + parsedDate.LengthOfDate);
                //look for <date> hh:mm:ss <UTC offset> 
                m = Regex.Match(ts, @"(?<=^\s*(?:,?\s|[\D\S]+\s|[T\-])?\s*)(?'hour'\d{2})\s*:\s*(?'minute'\d{2})\s*:\s*(?'second'\d{2})\s+(?'offset_sign'[\+\-])(?'offset_hh'\d{2}):?(?'offset_mm'\d{2})(?=$|[^\w])", RegexOptions.Compiled);
                if (m.Success)
                    timeIndex = parsedDate.IndexOfDate + parsedDate.LengthOfDate + m.Groups["hour"].Index;
                else
                {
                    //look for <date> [h]h:mm[:ss] [PM/AM] [Z/UTC/GMT] 
                    m = Regex.Match(ts, @"(?<=^\s*(?:,?\s|[\D\S]+\s|[T\-])?\s*)(?'hour'\d{1,2})\s*:\s*(?'minute'\d{2})\s*(?::\s*(?'second'\d{2}))?(?:\s*(?'ampm'AM|am|PM|pm))?" + timeZoneR + @"(?=$|[^\w])", RegexOptions.Compiled);
                    if (m.Success)
                        timeIndex = parsedDate.IndexOfDate + parsedDate.LengthOfDate + m.Groups["hour"].Index;
                    else
                    {
                        //look for [h]h:mm:ss [PM/AM] [Z/UTC/GMT] <date>
                        m = Regex.Match(str.Substring(0, parsedDate.IndexOfDate), @"(?<=^|[^\d])(?'hour'\d{1,2})\s*:\s*(?'minute'\d{2})\s*(?::\s*(?'second'\d{2}))?(?:\s*(?'ampm'AM|am|PM|pm))?" + timeZoneR + @"(?=$|[\s,]+)", RegexOptions.Compiled);
                        if (m.Success)
                            timeIndex = m.Groups["hour"].Index;
                        else
                        {
                            //look for [h]h:mm:ss [PM/AM] [Z/UTC/GMT] within <date>
                            m = Regex.Match(str.Substring(parsedDate.IndexOfDate, parsedDate.LengthOfDate), @"(?<=^|[^\d])(?'hour'\d{1,2})\s*:\s*(?'minute'\d{2})\s*(?::\s*(?'second'\d{2}))?(?:\s*(?'ampm'AM|am|PM|pm))?" + timeZoneR + @"(?=$|[\s,]+)", RegexOptions.Compiled);
                            if (m.Success)
                                timeIndex = parsedDate.IndexOfDate + m.Groups["hour"].Index;
                            else
                                return false;
                        }
                    }
                }
            }
            else//look anywhere within string
            {
                //look for hh:mm:ss <UTC offset> 
                m = Regex.Match(str, @"(?<=^|\s+|\s*T\s*)(?'hour'\d{2})\s*:\s*(?'minute'\d{2})\s*:\s*(?'second'\d{2})\s+(?'offset_sign'[\+\-])(?'offset_hh'\d{2}):?(?'offset_mm'\d{2})?(?=$|[^\w])", RegexOptions.Compiled);
                if (m.Success)
                    timeIndex = m.Groups["hour"].Index;
                else
                {
                    //look for [h]h:mm[:ss] [PM/AM] [Z/UTC/GMT]
                    m = Regex.Match(str, @"(?<=^|\s+|\s*T\s*)(?'hour'\d{1,2})\s*:\s*(?'minute'\d{2})\s*(?::\s*(?'second'\d{2}))?(?:\s*(?'ampm'AM|am|PM|pm))?" + timeZoneR + @"(?=$|[^\w])", RegexOptions.Compiled);
                    if (m.Success)
                        timeIndex = m.Groups["hour"].Index;
                    else
                        return false;
                }
            }

            //try
            //{
            int hour = int.Parse(m.Groups["hour"].Value);
            if (hour < 0 || hour > 23)
                return false;

            int minute = int.Parse(m.Groups["minute"].Value);
            if (minute < 0 || minute > 59)
                return false;

            int second = 0;
            if (!string.IsNullOrEmpty(m.Groups["second"].Value))
            {
                second = int.Parse(m.Groups["second"].Value);
                if (second < 0 || second > 59)
                    return false;
            }

            if (string.Compare(m.Groups["ampm"].Value, "PM", true) == 0 && hour < 12)
                hour += 12;
            else if (string.Compare(m.Groups["ampm"].Value, "AM", true) == 0 && hour == 12)
                hour -= 12;

            DateTime dateTime = new DateTime(1, 1, 1, hour, minute, second);

            if (m.Groups["offset_hh"].Success)
            {
                int offset_hh = int.Parse(m.Groups["offset_hh"].Value);
                int offset_mm = 0;
                if (m.Groups["offset_mm"].Success)
                    offset_mm = int.Parse(m.Groups["offset_mm"].Value);
                TimeSpan utcOffset = new TimeSpan(offset_hh, offset_mm, 0);
                if (m.Groups["offset_sign"].Value == "-")
                    utcOffset = -utcOffset;
                parsedTime = new ParsedDateTime(-1, -1, timeIndex, m.Length, dateTime, utcOffset, null);
                return true;
            }

            if (m.Groups["ukTimeZone"].Success)
            {
                TimeSpan utcOffset;
                switch (m.Groups["ukTimeZone"].Value)
                {
                    case "Z":
                    case "UTC":
                    case "GMT":
                        utcOffset = new TimeSpan(0, 0, 0);
                        break;
                    default:
                        throw new Exception("Time zone: " + m.Groups["ukTimeZone"].Value + " is not defined.");
                }
                parsedTime = new ParsedDateTime(-1, -1, timeIndex, m.Length, dateTime, utcOffset, DateTimeFormat.UK_DATE);
                return true;
            }
            if (m.Groups["usTimeZone"].Success)
            {
                TimeSpan utcOffset;
                switch (m.Groups["usTimeZone"].Value)
                {
                    case "Z":
                    case "UTC":
                    case "GMT":
                        utcOffset = new TimeSpan(0, 0, 0);
                        break;
                    case "CST":
                        utcOffset = new TimeSpan(-6, 0, 0);
                        break;
                    case "EST":
                        utcOffset = new TimeSpan(-5, 0, 0);
                        break;
                    default:
                        throw new Exception("Time zone: " + m.Groups["usTimeZone"].Value + " is not defined.");
                }
                parsedTime = new ParsedDateTime(-1, -1, timeIndex, m.Length, dateTime, utcOffset, DateTimeFormat.USA_DATE);
                return true;
            }

            parsedTime = new ParsedDateTime(-1, -1, timeIndex, m.Length, dateTime, null, null);
            return true;
        }

        static List<DateTimeFormat> getOrderedFormats(DateTimeFormat formats)
        {
            List<DateTimeFormat> orderedFormats = new List<DateTimeFormat>();
            if (formats.HasFlag(DateTimeFormat.UK_DATE))
                orderedFormats.Add(DateTimeFormat.UK_DATE);
            if (formats.HasFlag(DateTimeFormat.USA_DATE))
                orderedFormats.Add(DateTimeFormat.USA_DATE);
            return orderedFormats;
        }

        /// <summary>
        /// Tries to find date within the passed string and return it as ParsedDateTime object. 
        /// It recognizes only date while ignoring time, so time in the returned ParsedDateTime is always 0:0:0.
        /// If year of the date was not found then it accepts the current year. 
        /// </summary>
        /// <param name="str">string that contains date</param>
        /// <param name="orderedFormats">accepted formats in the applicable order</param>
        /// <param name="parsedDate">parsed date output</param>
        /// <returns>true if date was found, else false</returns>
        static public bool TryParseDate(string str, IEnumerable<DateTimeFormat> orderedFormats, out ParsedDateTime parsedDate)
        {
            foreach (var f in orderedFormats)
                if (tryParseDate(str, f, out parsedDate))
                    return true;
            parsedDate = null;
            return false;
        }

        static bool tryParseDate(string str, DateTimeFormat format, out ParsedDateTime parsedDate)
        {
            if (string.IsNullOrEmpty(str))
            {
                parsedDate = null;
                return false;
            }

            Match m;
            if (format == DateTimeFormat.UK_DATE)
            {
                //look for dd/mm/yy[yy]
                m = Regex.Match(str, @"(?<=^|[^\d])(?'day'\d{1,2})\s*(?'separator'[\\/\.])+\s*(?'month'\d{1,2})\s*\'separator'+\s*(?'year'\d{2}|\d{4})(?=$|[^\d])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    if (!convert2Date(int.Parse(m.Groups["year"].Value), int.Parse(m.Groups["month"].Value), int.Parse(m.Groups["day"].Value), out DateTime date))
                    {
                        parsedDate = null;
                        return false;
                    }
                    parsedDate = new ParsedDateTime(m.Index, m.Length, -1, -1, date, null, DateTimeFormat.UK_DATE);
                    return true;
                }

                //look for yy-mm-dd
                m = Regex.Match(str, @"(?<=^|[^\d])(?'year'\d{2})\s*(?'separator'[\-])\s*(?'month'\d{1,2})\s*\'separator'+\s*(?'day'\d{1,2})(?=$|[^\d])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    if (!convert2Date(int.Parse(m.Groups["year"].Value), int.Parse(m.Groups["month"].Value), int.Parse(m.Groups["day"].Value), out DateTime date))
                    {
                        parsedDate = null;
                        return false;
                    }
                    parsedDate = new ParsedDateTime(m.Index, m.Length, -1, -1, date, null, DateTimeFormat.UK_DATE);
                    return true;
                }
            }
            else if (format == DateTimeFormat.USA_DATE)
            {
                //look for mm/dd/yy[yy]
                m = Regex.Match(str, @"(?<=^|[^\d])(?'month'\d{1,2})\s*(?'separator'[\\/\.])+\s*(?'day'\d{1,2})\s*\'separator'+\s*(?'year'\d{2}|\d{4})(?=$|[^\d])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    if (!convert2Date(int.Parse(m.Groups["year"].Value), int.Parse(m.Groups["month"].Value), int.Parse(m.Groups["day"].Value), out DateTime date))
                    {
                        parsedDate = null;
                        return false;
                    }
                    parsedDate = new ParsedDateTime(m.Index, m.Length, -1, -1, date, null, DateTimeFormat.USA_DATE);
                    return true;
                }

                //look for mm-dd-yy[yy]
                m = Regex.Match(str, @"(?<=^|[^\d])(?'month'\d{1,2})\s*(?'separator'[\-])\s*(?'day'\d{1,2})\s*\'separator'+\s*(?'year'\d{2}|\d{4})(?=$|[^\d])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    if (!convert2Date(int.Parse(m.Groups["year"].Value), int.Parse(m.Groups["month"].Value), int.Parse(m.Groups["day"].Value), out DateTime date))
                    {
                        parsedDate = null;
                        return false;
                    }
                    parsedDate = new ParsedDateTime(m.Index, m.Length, -1, -1, date, null, DateTimeFormat.USA_DATE);
                    return true;
                }
            }
            else
                throw new Exception("Unknown format: " + format);

            //look for yyyy-mm-dd
            m = Regex.Match(str, @"(?<=^|[^\d])(?'year'\d{4})\s*(?'separator'[\-])\s*(?'month'\d{1,2})\s*\'separator'+\s*(?'day'\d{1,2})(?=$|[^\d])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (m.Success)
            {
                DateTime date;
                if (!convert2Date(int.Parse(m.Groups["year"].Value), int.Parse(m.Groups["month"].Value), int.Parse(m.Groups["day"].Value), out date))
                {
                    parsedDate = null;
                    return false;
                }
                parsedDate = new ParsedDateTime(m.Index, m.Length, -1, -1, date, null, null);
                return true;
            }

            //look for month dd yyyy
            m = Regex.Match(str, @"(?:^|[^\w])(?'month'Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[uarychilestmbro]*\s+(?'day'\d{1,2})(?:-?st|-?th|-?rd|-?nd)?\s*,?\s*(?'year'\d{4})(?=$|[^\w])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                //look for dd month [yy]yy
                m = Regex.Match(str, @"(?:^|[^\w:])(?'day'\d{1,2})(?:-?st\s+|-?th\s+|-?rd\s+|-?nd\s+|-|\s+)(?'month'Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[uarychilestmbro]*(?:\s*,?\s*|-)'?(?'year'\d{2}|\d{4})(?=$|[^\w])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                if (!m.Success)
                {
                    //look for yyyy month dd
                    m = Regex.Match(str, @"(?:^|[^\w])(?'year'\d{4})\s+(?'month'Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[uarychilestmbro]*\s+(?'day'\d{1,2})(?:-?st|-?th|-?rd|-?nd)?(?=$|[^\w])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    if (!m.Success)
                    {
                        //look for month dd hh:mm:ss MDT|UTC yyyy
                        m = Regex.Match(str, @"(?:^|[^\w])(?'month'Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[uarychilestmbro]*\s+(?'day'\d{1,2})\s+\d{2}\:\d{2}\:\d{2}\s+(?:MDT|UTC)\s+(?'year'\d{4})(?=$|[^\w])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        if (!m.Success)
                            //look for  month dd [yyyy]
                            m = Regex.Match(str, @"(?:^|[^\w])(?'month'Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[uarychilestmbro]*\s+(?'day'\d{1,2})(?:-?st|-?th|-?rd|-?nd)?(?:\s*,?\s*(?'year'\d{4}))?(?=$|[^\w])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    }
                }
            }
            if (m.Success)
            {
                int month = -1;
                int indexOfDate = m.Index;
                int lengthOfDate = m.Length;

                switch (m.Groups["month"].Value)
                {
                    case "Jan":
                    case "JAN":
                        month = 1;
                        break;
                    case "Feb":
                    case "FEB":
                        month = 2;
                        break;
                    case "Mar":
                    case "MAR":
                        month = 3;
                        break;
                    case "Apr":
                    case "APR":
                        month = 4;
                        break;
                    case "May":
                    case "MAY":
                        month = 5;
                        break;
                    case "Jun":
                    case "JUN":
                        month = 6;
                        break;
                    case "Jul":
                    case "JUL":
                        month = 7;
                        break;
                    case "Aug":
                    case "AUG":
                        month = 8;
                        break;
                    case "Sep":
                    case "SEP":
                        month = 9;
                        break;
                    case "Oct":
                    case "OCT":
                        month = 10;
                        break;
                    case "Nov":
                    case "NOV":
                        month = 11;
                        break;
                    case "Dec":
                    case "DEC":
                        month = 12;
                        break;
                }

                int year;
                if (!string.IsNullOrEmpty(m.Groups["year"].Value))
                    year = int.Parse(m.Groups["year"].Value);
                else
                    year = DefaultDate.Year;

                DateTime date;
                if (!convert2Date(year, month, int.Parse(m.Groups["day"].Value), out date))
                {
                    parsedDate = null;
                    return false;
                }
                parsedDate = new ParsedDateTime(indexOfDate, lengthOfDate, -1, -1, date, null, null);
                return true;
            }

            parsedDate = null;
            return false;
        }

        static bool convert2Date(int year, int month, int day, out DateTime date)
        {
            if (year < 100)
                year += (int)Math.Floor((decimal)DefaultDate.Year / 100) * 100;
            else if (year < 1000)
            {
                date = new DateTime(1, 1, 1);
                return false;
            }

            try
            {
                date = new DateTime(year, month, day);
            }
            catch
            {
                date = new DateTime(1, 1, 1);
                return false;
            }
            return true;
        }
    }
}
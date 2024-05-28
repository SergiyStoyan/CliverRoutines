using System;
using System.Threading;
using Cliver;

namespace CliverRoutinesTests
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string s1 = " Mon 11/23/2020  6:00AM     to Mon 12/21/2020  6:00AM";
                DateTimeRoutines.TryParseDateOrTime(s1, DateTimeRoutines.DateTimeFormat.USA_DATE, out DateTimeRoutines.ParsedDateTime date1);
                DateTimeRoutines.TryParseDateOrTime(s1.Substring(date1.IndexOfRemainder), DateTimeRoutines.DateTimeFormat.USA_DATE, out DateTimeRoutines.ParsedDateTime date2);
                if (date1.DateTime != new DateTime(2020, 11, 23, 6, 0, 0))
                    throw new Exception(s1);
                if (date2.DateTime != new DateTime(2020, 12, 21, 6, 0, 0))
                    throw new Exception(s1);

                DateTimeRoutinesTests t = new DateTimeRoutinesTests();
                t.TestDate();
                t.TestDateTime();
                t.TestTime();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}

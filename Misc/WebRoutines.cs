//********************************************************************************************
//Author: Sergiy Stoyan
//        s.y.stoyan@gmail.com, sergiy.stoyan@outlook.com, stoyan@cliversoft.com
//        http://www.cliversoft.com
//********************************************************************************************

using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


namespace Cliver
{
    public static class WebRoutines
    {
        static public string GetUrlQuery(Dictionary<string, string> names2value)
        {
            return string.Join("&", names2value.Select(n2v => WebUtility.UrlEncode(n2v.Key) + "=" + WebUtility.UrlEncode(n2v.Value)));
        }

        static public string GetUrlEncoded(string value)
        {
            return WebUtility.UrlEncode(value);
        }

        static public string GetUrlDecoded(string value)
        {
            return WebUtility.UrlDecode(value);
        }

        public static bool IsHttp(string path)
        {
            return path != null && Regex.IsMatch(path, @"^https?\:\/\/", RegexOptions.IgnoreCase);
        }
    }
}


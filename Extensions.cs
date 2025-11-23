//********************************************************************************************
//Author: Sergiy Stoyan
//        s.y.stoyan@gmail.com, sergiy.stoyan@outlook.com, stoyan@cliversoft.com
//        http://www.cliversoft.com
//********************************************************************************************

using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cliver
{
    public static class Extensions
    {
        /// <summary>
        /// Works for multiple values too, unlike the built-in ToString()
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string ToString2<T>(this T e, string separator) where T : Enum
        {
            return string.Join(separator, e.GetValues().Select(a => a.ToString()));
        }

        public static IEnumerable<T> GetValues<T>(this T e) where T : Enum
        {
            foreach (T v in Enum.GetValues(typeof(T)))
                if (e.HasFlag(v))
                    yield return v;
        }

        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
                action(item);
        }

        /// <summary>
        /// Copy the list of ranges into an output array. A range can be reversed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ps"></param>
        /// <param name="ranges"></param>
        /// <returns></returns>
        public static T[] CopyRanges<T>(this T[] ps, params (int I1, int I2)[] ranges)
        {
            T[] ps2 = new T[ranges.Sum(a => Math.Abs(a.I1 - a.I2) + 1)];
            int i2 = -1;
            foreach ((int I1, int I2) r in ranges)
            {
                int i = r.I1;
                for (i2++; i2 < ps2.Length; i2++)
                {
                    ps2[i2] = ps[i];
                    if (r.I1 <= r.I2)
                    {
                        if (++i > r.I2)
                            break;
                    }
                    else
                    {
                        if (--i < r.I2)
                            break;
                    }
                }
            }
            return ps2;
        }

        //public static object Convert(this object value, Type type)
        //{
        //    if (value == null)
        //        return type.GetDefault();
        //    type = Nullable.GetUnderlyingType(type) ?? type;
        //    return System.Convert.ChangeType(value, type);
        //}

        //public static T Convert<T>(this object value)
        //{
        //    if (value == null)
        //        return default(T);
        //    Type type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        //    return (T)System.Convert.ChangeType(value, type);
        //}

        public static byte GetNumberOfDigits(this long n)
        {
            byte c = 0;
            n = Math.Abs(n);
            do
            {
                n = n / 10;
                c++;
            }
            while (Math.Abs(n) >= 1);
            return c;
        }

        public static byte GetNumberOfDigits(this int n)
        {
            return GetNumberOfDigits((long)n);
        }

        /// <summary>
        /// Replacement for BeginInvoke() which is not supported in .NET5
        /// </summary>
        /// <param name="delegate"></param>
        /// <param name="ps"></param>
        public static void BeginInvoke(this Delegate @delegate, params object[] ps)
        {
            ThreadRoutines.Start(() => { @delegate.DynamicInvoke(ps); });
        }

        public static O CreateCloneByJson<O>(this O o)
        {
            return Serialization.Json.Clone<O>(o);
        }

        public static object CreateCloneByJson(this object o, Type type)
        {
            return Serialization.Json.Clone(type, o);
        }

        public static bool IsEqualByJson(this object a, object b)
        {
            return Serialization.Json.IsEqual(a, b);
        }

        public static string ToStringByJson(this object o, bool indented = true, bool polymorphic = false, bool ignoreNullValues = true, bool ignoreDefaultValues = false/*, bool ignoreReferenceLoop = true !!!can go to the endless cycle*/)
        {
            return JsonConvert.SerializeObject(o,
                    indented ? Formatting.Indented : Formatting.None,
                   new JsonSerializerSettings
                   {
                       TypeNameHandling = polymorphic ? TypeNameHandling.Auto : TypeNameHandling.None,
                       NullValueHandling = ignoreNullValues ? NullValueHandling.Ignore : NullValueHandling.Include,
                       DefaultValueHandling = ignoreDefaultValues ? DefaultValueHandling.Ignore : DefaultValueHandling.Include,
                       //ReferenceLoopHandling = ignoreReferenceLoop ? Newtonsoft.Json.ReferenceLoopHandling.Serialize : Newtonsoft.Json.ReferenceLoopHandling.Error
                   }
                   );
        }

        public static string ToStringByJson(this object o, JsonSerializerSettings jsonSerializerSettings)
        {
            return Serialization.Json.Serialize(o, jsonSerializerSettings);
        }
    }
}
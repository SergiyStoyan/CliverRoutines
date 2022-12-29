//********************************************************************************************
//Author: Sergiy Stoyan
//        systoyan@gmail.com
//        http://www.cliversoft.com
//********************************************************************************************
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Drawing;

namespace Cliver
{
    public abstract partial class StringTable
    {
        public void Read(IList<IList<object>> valuess, ReadingMode mode = ReadingMode.IgnoreEmptyRows)
        {
            int y = 0;
            read(mode, () => { return y < valuess.Count ? valuess[y++].Select(a => a.ToString()).ToList() : null; });
        }

        public void Write(string file)
        {
            using (var sw = new StreamWriter(file))
            {
                Write(sw);
            }
        }

        public void Read(string file, ReadingMode modes = ReadingMode.IgnoreEmptyRows)
        {
            using (var sr = new StreamReader(file))
            {
                Read(sr, modes);
            }
        }
    }
}
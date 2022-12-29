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
        public class Tsv : StringTable
        {
            protected override List<string> getRowValues(string line)
            {
                if (line == null)
                    return null;
                return Regex.Split(line, @"\t").ToList();
            }

            protected override string getLine(Row row)
            {
                List<string> svs = new List<string>();
                foreach (string v in row.Values)
                {
                    string s = Regex.Replace(v, @"[\s\t]", @" ");
                    svs.Add(s);
                }
                return string.Join("\t", svs);
            }
        }
    }
}
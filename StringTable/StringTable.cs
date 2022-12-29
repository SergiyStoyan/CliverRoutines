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
        public enum Modes
        {
            AppendDefaultHeaderIfNeeded = 1,
            AppendEmptyCellIfNeeded = 2,
            IgnoreEmptyRows = 4,
        }

        protected abstract List<string> getRowValues(string line);

        public void Read(StreamReader streamReader, Modes mode)
        {
            read(mode, () => { return getRowValues(streamReader.ReadLine()); });
        }

        void read(Modes mode, Func<List<string>> getRowValues)
        {
            Headers = getRowValues();
            if (Headers == null)
                throw new Exception("There is no header.");

            Rows = new List<Row>();
            int lineNumber = 1;
            for (List<string> vs = getRowValues(); vs != null; vs = getRowValues())
            {
                lineNumber++;
                if (vs.Count == 1 && string.IsNullOrEmpty(vs[0]) && Headers.Count > 1)
                {
                    if (mode.HasFlag(Modes.IgnoreEmptyRows))
                        continue;
                    //throw new Exception("The line " + lineNumber + " is empty.");
                }
                if (vs.Count > Headers.Count)
                {
                    if (!mode.HasFlag(Modes.AppendDefaultHeaderIfNeeded))
                        throw new Exception("The line " + lineNumber + " has more columns than headers: " + vs.Count + " > " + Headers.Count);
                }
                else if (vs.Count < Headers.Count)
                {
                    if (!mode.HasFlag(Modes.AppendEmptyCellIfNeeded))
                        throw new Exception("The line " + lineNumber + " has less columns than headers: " + vs.Count + " < " + Headers.Count);
                }
                Rows.Add(new Row(vs, Rows.Count + 1, this));
            }
            ColumnNumber = Rows.Select(a => a.Values.Count).Max();
        }

        public List<string> Headers { get; private set; }

        public List<Row> Rows { get; private set; }

        public int ColumnNumber { get; private set; } = 0;

        public int RowNumber { get { return Rows.Count; } }

        public string this[int y, string header]
        {
            get
            {
                return Rows[y][header];
            }
        }

        public string this[int y, int x]
        {
            get
            {
                return Rows[y - 1][x];
            }
        }

        public Row this[int y]
        {
            get
            {
                return Rows[y - 1];
            }
        }

        public class Row
        {
            public string this[int x]
            {
                get
                {
                    if (x > Values.Count)
                    {
                        if (x > table.ColumnNumber)
                            throw new Exception("X is out of the column number: " + x + " > " + table.ColumnNumber);
                        return null;
                    }
                    return Values[x - 1];
                }
            }

            public string this[string header]
            {
                get
                {
                    int x0 = table.Headers.IndexOf(header);
                    if (x0 < 0)
                        throw new Exception("No such header: '" + header + "'");
                    return this[x0 + 1];
                }
            }

            public List<string> Values { get; private set; }

            public int Y { get; }

            internal Row(List<string> values, int y, StringTable table)
            {
                Values = values;
                Y = y;
                this.table = table;
            }
            StringTable table;
        }

        protected abstract string getLine(Row row);

        public void Write(StreamWriter streamWriter)
        {
            streamWriter.WriteLine(getLine(new Row(Headers, 0, this)));
            foreach (Row row in Rows)
                streamWriter.WriteLine(getLine(row));
            streamWriter.Flush();
        }
    }
}
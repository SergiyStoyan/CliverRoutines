//********************************************************************************************
//Author: Sergey Stoyan
//        sergey.stoyan@gmail.com
//        http://www.cliversoft.com
//********************************************************************************************
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Reflection;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace Cliver
{
    /// <summary>
    /// Read/write a document-per-line file. Thread-safe.
    /// </summary>
    public class DataFile
    {
        public DataFile(string file)
        {
            File = file;
            FileSystemRoutines.CreateDirectory(PathRoutines.GetFileDir(File));
        }
        public readonly string File;

        public IEnumerable<string> ReadLines()
        {
            lock (this)
            {
                if (System.IO.File.Exists(File))
                    using (TextReader tr = new StreamReader(File))
                        for (string l = tr.ReadLine(); l != null; l = tr.ReadLine())
                            yield return l;
            }
        }

        public void WriteLines(IEnumerable<string> lines, bool append = true)
        {
            lock (this)
            {
                using (TextWriter tw = new StreamWriter(File, append))
                    foreach (string line in lines)
                        tw.WriteLine(line);
            }
        }

        public class Tsv<DocumentT> : DataFile where DocumentT : new()
        {
            public Tsv(string file) : base(file)
            {
            }
            FieldInfo[] pis = typeof(DocumentT).GetFields();

            public void Write(IEnumerable<DocumentT> documents, bool append = true)
            {
                lock (this)
                {
                    FileInfo fi = new FileInfo(File);
                    if (!fi.Exists || fi.Length == 0)
                    {
                        List<string> hs = new List<string>();
                        foreach (FieldInfo pi in pis)
                        {
                            if (pi.GetCustomAttribute<FieldPreparation.IgnoredField>() != null)
                                continue;
                            hs.Add(pi.Name);
                        }
                        WriteLines(new List<string> { FieldPreparation.Tsv.GetLine(hs) }, append);
                    }

                    WriteLines(documents.Select(a => FieldPreparation.Tsv.GetLine(a)), append);
                }
            }

            public IEnumerable<DocumentT> Read()
            {
                lock (this)
                {
                    var ls = ReadLines().GetEnumerator();
                    ls.MoveNext();
                    while (ls.MoveNext())
                    {
                        string[] vs = ls.Current.Split('\t');
                        DocumentT d = new DocumentT();
                        for (int i = 0; i < vs.Length; i++)
                            pis[i].SetValue(d, vs[i]);
                        yield return d;
                    }
                }
            }
        }

        public class Json<DocumentT> : DataFile where DocumentT : new()
        {
            public Json(string file) : base(file)
            { }

            public void Write(IEnumerable<DocumentT> documents, bool append = true)
            {
                lock (this)
                {
                    WriteLines(documents.Select(a => Serialization.Json.Serialize(a, false)), append);
                }
            }

            public IEnumerable<DocumentT> Read()
            {
                lock (this)
                {
                    foreach (string l in ReadLines())
                        yield return Serialization.Json.Deserialize<DocumentT>(l);
                }
            }
        }
    }
}

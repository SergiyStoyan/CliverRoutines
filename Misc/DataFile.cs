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
    abstract public class DataFile<DocumentT> where DocumentT : new()
    {
        protected DataFile(string file)
        {
            File = file;
            FileSystemRoutines.CreateDirectory(PathRoutines.GetFileDir(File));
        }
        public readonly string File;

        abstract public void Write(IEnumerable<DocumentT> documents, bool append = true);

        abstract public IEnumerable<DocumentT> Read();

        public class Tsv : DataFile<DocumentT>
        {
            public Tsv(string file) : base(file)
            {
            }
            readonly FieldInfo[] pis = typeof(DocumentT).GetFields();

            override public void Write(IEnumerable<DocumentT> documents, bool append = true)
            {
                lock (this)
                {
                    using (TextWriter tw = new StreamWriter(File, append))
                    {
                        FileInfo fi = new FileInfo(File);
                        if (!append || !fi.Exists || fi.Length == 0)
                        {
                            List<string> hs = new List<string>();
                            foreach (FieldInfo pi in pis)
                            {
                                if (pi.GetCustomAttribute<FieldPreparation.IgnoredField>() != null)
                                    continue;
                                hs.Add(pi.Name);
                            }
                            if (hs.Count == 0)
                                throw new Exception("Document type " + typeof(DocumentT).Name + " gives no header.");
                            tw.WriteLine(FieldPreparation.Tsv.GetLine(hs));
                        }

                        foreach (DocumentT d in documents)
                            tw.WriteLine(FieldPreparation.Tsv.GetLine(d));
                    }
                }
            }

            override public IEnumerable<DocumentT> Read()
            {
                lock (this)
                {
                    if (System.IO.File.Exists(File))
                        using (TextReader tr = new StreamReader(File))
                        {
                            string l = tr.ReadLine();//pass off the header
                            for (l = tr.ReadLine(); l != null; l = tr.ReadLine())
                            {
                                string[] vs = l.Split('\t');
                                DocumentT d = new DocumentT();
                                for (int i = 0; i < vs.Length; i++)
                                    pis[i].SetValue(d, vs[i]);
                                yield return d;
                            }
                        }
                }
            }
        }

        public class Json : DataFile<DocumentT> 
        {
            public Json(string file) : base(file)
            { }

            override public void Write(IEnumerable<DocumentT> documents, bool append = true)
            {
                lock (this)
                {
                    using (TextWriter tw = new StreamWriter(File, append))
                        foreach (DocumentT d in documents)
                            tw.WriteLine(Serialization.Json.Serialize(d, false));
                }
            }

            override public IEnumerable<DocumentT> Read()
            {
                lock (this)
                {
                    if (System.IO.File.Exists(File))
                        using (TextReader tr = new StreamReader(File))
                            for (string l = tr.ReadLine(); l != null; l = tr.ReadLine())
                                yield return Serialization.Json.Deserialize<DocumentT>(l);
                }
            }
        }
    }
}

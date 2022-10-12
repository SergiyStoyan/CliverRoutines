/********************************************************************************************
        Author: Sergey Stoyan
        sergey.stoyan@gmail.com
        sergey.stoyan@hotmail.com
        stoyan@cliversoft.com
        http://www.cliversoft.com
********************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json;

namespace Cliver
{
    public partial class ListDb
    {
        //public class Document
        //{
        //[Newtonsoft.Json.JsonIgnoreAttribute]
        //public string test;
        //}

        //public class IgnoredField : Attribute
        //{
        //}

        public static Table<DocumentType> GetTable<DocumentType>(string directory = null) where DocumentType : new()
        {
            return Table<DocumentType>.Get(directory);
        }

        public class Table<DocumentType> : IDisposable where DocumentType : new()
        {
            public static Table<DocumentType> Get(string directory = null)
            {
                directory = getNormalizedDirectory(directory);
                WeakReference wr;
                string key = directory + Path.DirectorySeparatorChar + typeof(DocumentType).Name;
                lock (tableKeys2table)
                {
                    if (!tableKeys2table.TryGetValue(key, out wr)
                    || !wr.IsAlive
                    )
                    {
                        Table<DocumentType> t = new Table<DocumentType>(directory, key);
                        wr = new WeakReference(t);
                        tableKeys2table[key] = wr;
                    }
                }
                return (Table<DocumentType>)wr.Target;
            }
            protected static Dictionary<string, WeakReference> tableKeys2table = new Dictionary<string, WeakReference>();

            protected static string getNormalizedDirectory(string directory = null)
            {
                if (directory == null)
                    directory = Cliver.Log.AppCompanyCommonDataDir;
                return PathRoutines.GetNormalizedPath(directory, true);
            }

            ~Table()
            {
                Dispose();
            }

            public void Dispose()
            {
                lock (this)
                {
                    try
                    {
                        if (fileWriter == null)
                            return;

                        lock (tableKeys2table)
                        {
                            if (tableKeys2table.ContainsKey(key))
                                tableKeys2table.Remove(key);
                        }

                        if (Mode.HasFlag(Modes.FLUSH_TABLE_ON_CLOSE))
                            Flush();
                        if (fileWriter != null)
                        {
                            fileWriter.Close();
                            fileWriter = null;
                        }
                        if (logWriter != null)
                        {
                            logWriter.Close();
                            logWriter = null;
                        }
                    }
                    catch
                    {
                        //when Dispose is called from finalizer, files may be already closed and so exception thrown
                    }
                }
            }

            public readonly string Log = null;
            TextWriter logWriter;
            public readonly string File = null;
            protected TextWriter fileWriter;
            readonly string newFile;
            public Modes Mode = Modes.FLUSH_TABLE_ON_CLOSE;
            public readonly string Name;
            protected readonly string key;
            protected readonly List<DocumentType> documents = new List<DocumentType>();

            public enum Modes
            {
                NULL = 0,
                //KEEP_TABLE_EVER_OPEN = 1,//requires explicitly calling Close()
                FLUSH_TABLE_ON_CLOSE = 2,
                FLUSH_TABLE_ON_START = 4,
            }

            public delegate void SavedHandler(DocumentType document, bool asNew);
            public event SavedHandler Saved = null;
            protected void invokeSaved(DocumentType document, bool asNew)
            {
                Saved?.Invoke(document, asNew);
            }

            public delegate void RemovedHandler(DocumentType document, bool sucess);
            public event RemovedHandler Removed = null;
            protected void invokeRemoved(DocumentType document, bool sucess)
            {
                Removed?.Invoke(document, sucess);
            }

            protected Table(string directory, string key)
            {
                directory = getNormalizedDirectory(directory);
                this.key = key;

                Name = typeof(DocumentType).Name + "s";

                File = directory + System.IO.Path.DirectorySeparatorChar + Name + ".listdb";
                newFile = File + ".new";
                Log = directory + System.IO.Path.DirectorySeparatorChar + Name + ".listdb.log";

                if (System.IO.File.Exists(newFile))
                {
                    if (System.IO.File.Exists(File))
                        System.IO.File.Delete(File);
                    System.IO.File.Move(newFile, File);
                    if (System.IO.File.Exists(Log))
                        System.IO.File.Delete(Log);
                }

                if (System.IO.File.Exists(File))
                {
                    using (TextReader fr = new StreamReader(File))
                    {
                        if (System.IO.File.Exists(Log))
                        {
                            foreach (string l in System.IO.File.ReadLines(Log))
                            {
                                (Action action, int number) r = readLogLine(l);
                                DocumentType documentType;
                                switch (r.action)
                                {
                                    case Action.flushed://it can be only the first line in the log
                                        for (int i = 0; i < r.number; i++)
                                        {
                                            string fl = fr.ReadLine();
                                            if (fl == null)
                                                throw new Exception("ListDb log file is broken: there are less documents in the table '" + Name + "' (" + i + ") than were fushed(" + r.number + ")");
                                            documentType = JsonConvert.DeserializeObject<DocumentType>(fl);
                                            documents.Add(documentType);
                                        }
                                        if (fr.ReadLine() != null)
                                            throw new Exception("ListDb log file is broken: there are more documents in the table '" + Name + "' than were fushed (" + r.number + ")");
                                        continue;
                                    case Action.deleted:
                                        try
                                        {
                                            documents.RemoveAt(r.number);
                                        }
                                        catch (Exception e)
                                        {
                                            throw new Exception("ListDb log file is broken.", e);
                                        }
                                        continue;
                                    case Action.replaced:
                                        {
                                            string fl = fr.ReadLine();
                                            if (fl == null)
                                                throw new Exception("ListDb log file is broken: there is no document in the table '" + Name + "' for a replaced index (" + r.number + ")");
                                            documentType = JsonConvert.DeserializeObject<DocumentType>(fl);
                                            if (r.number >= documents.Count)
                                                throw new Exception("ListDb log file is broken: there are less documents in the table '" + Name + "' than a replaced index (" + r.number + ")");
                                            documents.RemoveAt(r.number);
                                            documents.Insert(r.number, documentType);
                                        }
                                        continue;
                                    case Action.added:
                                        {
                                            string fl = fr.ReadLine();
                                            if (fl == null)
                                                throw new Exception("ListDb log file is broken: there is no document in the table '" + Name + "' for an added index (" + r.number + ")");
                                            documentType = JsonConvert.DeserializeObject<DocumentType>(fl);
                                            if (r.number != documents.Count)
                                                throw new Exception("ListDb log file is broken: number of documents in the table '" + Name + "' (" + documents.Count + ") is not equal an added index (" + r.number + ")");
                                            documents.Add(documentType);
                                        }
                                        continue;
                                    case Action.inserted:
                                        {
                                            string fl = fr.ReadLine();
                                            if (fl == null)
                                                throw new Exception("ListDb log file is broken: there is no document in the table '" + Name + "' for an inserted index (" + r.number + ")");
                                            documentType = JsonConvert.DeserializeObject<DocumentType>(fl);
                                            if (r.number >= documents.Count)
                                                throw new Exception("ListDb log file is broken: there less documents in the table '" + Name + "' (" + documents.Count + ") than an inserted index (" + r.number + ")");
                                            documents.Insert(r.number, documentType);
                                        }
                                        continue;
                                    case Action.flushing:
                                        continue;
                                    default:
                                        throw new Exception("Unknown action: " + r.action);
                                }
                            }
                            if (fr.ReadLine() != null)
                                throw new Exception("ListDb log file is broken: there more documents in the table '" + Name + "' than recorded in the log.");
                            if (Mode.HasFlag(Modes.FLUSH_TABLE_ON_START))
                            {
                                FileInfo fi = new FileInfo(Log);
                                if (fi.Exists && fi.Length > 0)
                                    Flush();
                            }
                        }
                        else
                        {
                            for (string s = fr.ReadLine(); s != null; s = fr.ReadLine())
                                documents.Add(JsonConvert.DeserializeObject<DocumentType>(s));
                        }
                    }
                }

                fileWriter = new StreamWriter(File, true);
                ((StreamWriter)fileWriter).AutoFlush = true;
                logWriter = new StreamWriter(Log, true);
                ((StreamWriter)logWriter).AutoFlush = true;
            }

            protected enum Action
            {
                added,
                replaced,
                inserted,
                deleted,
                flushed,
                flushing
            }

            protected void writeLogLine(Action action, int number)
            {
                lock (this)
                {
                    logWriter.WriteLine(action.ToString() + ": " + number);
                }
            }

            (Action action, int number) readLogLine(string line)
            {
                lock (this)
                {
                    Match m = logReadingRegex.Match(line);
                    if (!Enum.TryParse(m.Groups[1].Value, out Action action))
                        throw new Exception("Unknown action in the ListDb log: '" + m.Groups[1].Value + "'");
                    switch (action)
                    {
                        case Action.replaced:
                            return (Action.replaced, int.Parse(m.Groups[2].Value));
                        case Action.deleted:
                            return (Action.deleted, int.Parse(m.Groups[2].Value));
                        case Action.added:
                            return (Action.added, int.Parse(m.Groups[2].Value));
                        case Action.flushed:
                            return (Action.flushed, int.Parse(m.Groups[2].Value));
                        case Action.inserted:
                            return (Action.inserted, int.Parse(m.Groups[2].Value));
                        case Action.flushing:
                            return (Action.flushing, int.Parse(m.Groups[2].Value));
                        default:
                            throw new Exception("Unknown action: " + action);
                    }
                }
            }
            Regex logReadingRegex = new Regex("(" + string.Join("|", Enum.GetNames(typeof(Action))) + @")(?:\:\s*(\d+))?");

            /// <summary>
            /// Rewrite the data on the disk, cleaning the log.
            /// </summary>
            public void Flush()
            {
                lock (this)
                {
                    writeLogLine(Action.flushing, -1);

                    using (TextWriter newFileWriter = new StreamWriter(newFile, false))
                    {
                        foreach (DocumentType d in documents)
                            newFileWriter.WriteLine(JsonConvert.SerializeObject(d, Formatting.None));
                        newFileWriter.Flush();
                    }

                    if (fileWriter != null)
                        fileWriter.Close();
                    if (System.IO.File.Exists(File))
                        System.IO.File.Delete(File);
                    System.IO.File.Move(newFile, File);
                    fileWriter = new StreamWriter(File, true);
                    ((StreamWriter)fileWriter).AutoFlush = true;

                    if (logWriter != null)
                        logWriter.Close();
                    logWriter = new StreamWriter(Log, false);
                    ((StreamWriter)logWriter).AutoFlush = true;

                    writeLogLine(Action.flushed, documents.Count);
                }
            }

            /// <summary>
            /// Delete the table.
            /// </summary>
            public void Drop()
            {
                lock (this)
                {
                    documents.Clear();

                    if (fileWriter != null)
                        fileWriter.Close();
                    if (System.IO.File.Exists(File))
                        System.IO.File.Delete(File);

                    if (logWriter != null)
                        logWriter.Close();
                    if (System.IO.File.Exists(Log))
                        System.IO.File.Delete(Log);
                }
            }

            /// <summary>
            /// Remove all the data from the table.
            /// </summary>
            public void Clear()
            {
                lock (this)
                {
                    Drop();

                    fileWriter = new StreamWriter(File, false);
                    ((StreamWriter)fileWriter).AutoFlush = true;

                    logWriter = new StreamWriter(Log, false);
                    ((StreamWriter)logWriter).AutoFlush = true;
                }
            }

            /// <summary>
            /// Adds a document to the table if it does not exists. Otherwise, overwrites it.
            /// Table works as an ordered HashSet.
            /// </summary>
            /// <param name="document"></param>
            /// <returns></returns>
            virtual public Result Save(DocumentType document)
            {
                lock (this)
                {
                    int i = documents.IndexOf(document);
                    if (i >= 0)
                    {
                        fileWriter.WriteLine(JsonConvert.SerializeObject(document, Formatting.None));
                        writeLogLine(Action.replaced, i);
                        Saved?.Invoke(document, false);
                        return Result.UPDATED;
                    }
                    else
                    {
                        fileWriter.WriteLine(JsonConvert.SerializeObject(document, Formatting.None));
                        documents.Add(document);
                        writeLogLine(Action.added, documents.Count - 1);
                        Saved?.Invoke(document, true);
                        return Result.ADDED;
                    }
                }
            }

            public enum Result
            {
                ADDED,
                UPDATED,
                MOVED2TOP,
                MOVED,
                INSERTED,
            }

            /// <summary>
            /// Adds a document to the end. If it exists then it is moved to the end. 
            /// Table works as an ordered HashSet.
            /// </summary>
            /// <param name="document"></param>
            virtual public Result Add(DocumentType document)
            {
                lock (this)
                {
                    int i = documents.IndexOf(document);
                    if (i >= 0)
                    {
                        fileWriter.WriteLine(JsonConvert.SerializeObject(document, Formatting.None));
                        documents.RemoveAt(i);
                        documents.Add(document);
                        writeLogLine(Action.deleted, i);
                        writeLogLine(Action.added, documents.Count - 1);
                        Saved?.Invoke(document, false);
                        return Result.MOVED2TOP;
                    }
                    else
                    {
                        fileWriter.WriteLine(JsonConvert.SerializeObject(document, Formatting.None));
                        documents.Add(document);
                        writeLogLine(Action.added, documents.Count - 1);
                        Saved?.Invoke(document, true);
                        return Result.ADDED;
                    }
                }
            }

            public void AddRange(IEnumerable<DocumentType> documents)
            {
                lock (this)
                {
                    foreach (DocumentType document in documents)
                        Add(document);
                }
            }

            /// <summary>
            /// Table works as an ordered HashSet
            /// </summary>
            /// <param name="document"></param>
            virtual public Result Insert(int index, DocumentType document)
            {
                lock (this)
                {
                    int i = documents.IndexOf(document);
                    if (i >= 0)
                    {
                        fileWriter.WriteLine(JsonConvert.SerializeObject(document, Formatting.None));
                        documents.RemoveAt(i);
                        documents.Insert(index, document);
                        writeLogLine(Action.replaced, i);
                        Saved?.Invoke(document, false);
                        return Result.MOVED;
                    }
                    else
                    {
                        fileWriter.WriteLine(JsonConvert.SerializeObject(document, Formatting.None));
                        documents.Insert(index, document);
                        writeLogLine(Action.inserted, index);
                        Saved?.Invoke(document, true);
                        return Result.INSERTED;
                    }
                }
            }

            public IEnumerable<DocumentType> Find(Predicate<DocumentType> match)
            {
                lock (this)
                {
                    List<DocumentType> matchedDocuments = new List<DocumentType>();
                    foreach (DocumentType document in documents)
                        if (match(document))
                            yield return document;
                }
            }

            public void InsertRange(int index, IEnumerable<DocumentType> documents)
            {
                lock (this)
                {
                    throw new Exception("TBD");
                    //base.InsertRange(index, documents);
                }
            }

            public bool Remove(DocumentType document)
            {
                lock (this)
                {
                    int i = documents.IndexOf(document);
                    if (i >= 0)
                    {
                        documents.RemoveAt(i);
                        writeLogLine(Action.deleted, i);
                        Removed?.Invoke(document, true);
                        return true;
                    }
                    Removed?.Invoke(document, false);
                    return false;
                }
            }

            public int RemoveAll(Predicate<DocumentType> match)
            {
                lock (this)
                {
                    List<DocumentType> documentsToDelete = new List<DocumentType>();
                    foreach (DocumentType document in documents)
                        if (match(document))
                            documentsToDelete.Add(document);
                    foreach (DocumentType document in documentsToDelete)
                        Remove(document);
                    return documentsToDelete.Count;
                }
            }

            public void RemoveAt(int index)
            {
                lock (this)
                {
                    documents.RemoveAt(index);
                    writeLogLine(Action.deleted, index);
                }
            }

            public void RemoveRange(int index, int count)
            {
                lock (this)
                {
                    for (int i = index; i < count; i++)
                        RemoveAt(i);
                }
            }

            public int Count
            {
                get
                {
                    lock (this)
                    {
                        return documents.Count;
                    }
                }
            }

            public DocumentType First(Predicate<DocumentType> match = null)
            {
                lock (this)
                {
                    if (match == null)
                        return documents.First();
                    return documents.First(new Func<DocumentType, bool>(match));
                }
            }

            public DocumentType Last(Predicate<DocumentType> match = null)
            {
                lock (this)
                {
                    if (match == null)
                        return documents.Last();
                    return documents.Last(new Func<DocumentType, bool>(match));
                }
            }

            //public DocumentType? GetPrevious(DocumentType document)
            //{
            //    if (document == null)
            //        return null;
            //    int i = IndexOf(document);
            //    if (i < 1)
            //        return null;
            //    return (DocumentType?)this[i - 1];
            //}

            //public DocumentType? GetNext(DocumentType document)
            //{
            //    if (document == null)
            //        return null;
            //    int i = this.IndexOf(document);
            //    if (i + 1 >= this.Count)
            //        return null;
            //    return (DocumentType?)this[i + 1];
            //}
        }
    }
}
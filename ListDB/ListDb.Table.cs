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
        public class Table<DocumentT> : IDisposable where DocumentT : new()
        {
            public static Table<DocumentT> Get(string directory, bool ignoreRestoreError = true)
            {
                WeakReference wr;
                string file = getFile<DocumentT>(directory);
                lock (tableFiles2table)
                {
                    if (!tableFiles2table.TryGetValue(file, out wr)
                    || !wr.IsAlive
                    )
                    {
                        Table<DocumentT> t = new Table<DocumentT>(file, ignoreRestoreError);
                        wr = new WeakReference(t);
                        tableFiles2table[file] = wr;
                    }
                }
                return (Table<DocumentT>)wr.Target;
            }
            protected static Dictionary<string, WeakReference> tableFiles2table = new Dictionary<string, WeakReference>();

            protected static string getFile<DocumentT>(string directory)
            {
                return PathRoutines.GetNormalizedPath(directory, true) + Path.DirectorySeparatorChar + typeof(DocumentT).Name + "s" + ".listdb";
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

                        lock (tableFiles2table)
                        {
                            if (tableFiles2table.ContainsKey(File))
                                tableFiles2table.Remove(File);
                        }

                        if (Mode.HasFlag(Modes.FLUSH_TABLE_ON_CLOSE))
                            Flush();
                        if (fileWriter != null)
                        {
                            fileWriter.Close();
                            fileWriter = null;
                        }
                    }
                    catch
                    {
                        //when Dispose is called from finalizer, files may be already closed and so exception thrown
                    }
                }
            }

            public readonly string File = null;
            protected TextWriter fileWriter;
            public Modes Mode = Modes.FLUSH_TABLE_ON_CLOSE;
            public readonly string Name;
            protected readonly List<DocumentT> documents = new List<DocumentT>();

            public enum Modes
            {
                NULL = 0,
                //KEEP_TABLE_EVER_OPEN = 1,//requires explicitly calling Close()
                FLUSH_TABLE_ON_CLOSE = 2,
                FLUSH_TABLE_ON_START = 4,
            }

            public delegate void SavedHandler(DocumentT document, bool asNew);
            public event SavedHandler Saved = null;
            protected void invokeSaved(DocumentT document, bool asNew)
            {
                Saved?.Invoke(document, asNew);
            }

            public delegate void RemovedHandler(DocumentT document, bool sucess);
            public event RemovedHandler Removed = null;

            protected Table(string file, bool ignoreRestoreError)
            {
                File = file;
                Name = PathRoutines.GetFileNameWithoutExtention(file);

                (Action action, int number) operation = (action: Action.added, number: -1);
                if (System.IO.File.Exists(File))
                {
                    using (TextReader fr = new StreamReader(File))
                    {
                        try
                        {
                            for (string l = fr.ReadLine(); l != null; l = fr.ReadLine())
                            {
                                DocumentT document;
                                if (l[0] == '{' || l[0] == '[')
                                {
                                    document = JsonConvert.DeserializeObject<DocumentT>(l);
                                    documents.Add(document);
                                }
                                else
                                {
                                    operation = readActionLine(l);
                                    switch (operation.action)
                                    {
                                        case Action.deleted:
                                            if (documents.Count <= operation.number)
                                                throw new RestoreException("There are less documents in the table '" + Name + "' than a deleted element index (" + operation.number + ")");
                                            documents.RemoveAt(operation.number);
                                            continue;
                                        case Action.replaced:
                                            {
                                                l = fr.ReadLine();
                                                if (l == null)
                                                    throw new RestoreException("There is no document in the table '" + Name + "' for a replaced element index (" + operation.number + ")");
                                                document = JsonConvert.DeserializeObject<DocumentT>(l);
                                                if (operation.number >= documents.Count)
                                                    throw new RestoreException("There are less documents in the table '" + Name + "' than a replaced element index (" + operation.number + ")");
                                                documents.RemoveAt(operation.number);
                                                documents.Insert(operation.number, document);
                                            }
                                            continue;
                                        case Action.added:
                                            {
                                                l = fr.ReadLine();
                                                if (l == null)
                                                    throw new RestoreException("There is no document in the table '" + Name + "' for an added element index (" + operation.number + ")");
                                                document = JsonConvert.DeserializeObject<DocumentT>(l);
                                                if (operation.number != documents.Count)
                                                    throw new RestoreException("Number of documents in the table '" + Name + "' (" + documents.Count + ") is not equal an added element index (" + operation.number + ")");
                                                documents.Add(document);
                                            }
                                            continue;
                                        case Action.inserted:
                                            {
                                                l = fr.ReadLine();
                                                if (l == null)
                                                    throw new RestoreException("There is no document in the table '" + Name + "' for an inserted element index (" + operation.number + ")");
                                                document = JsonConvert.DeserializeObject<DocumentT>(l);
                                                if (operation.number >= documents.Count)
                                                    throw new RestoreException("There are less documents in the table '" + Name + "' (" + documents.Count + ") than an inserted element index (" + operation.number + ")");
                                                documents.Insert(operation.number, document);
                                            }
                                            continue;
                                        default:
                                            throw new Exception("Unknown action: " + operation.action);
                                    }
                                }
                            }
                        }
                        catch (RestoreException e)
                        {
                            FirstRestoreException = e;
                            if (!ignoreRestoreError)
                                throw;
                        }
                    }
                }
                if (Mode.HasFlag(Modes.FLUSH_TABLE_ON_START) && operation.number >= 0)
                    Flush();
                else
                {
                    fileWriter = new StreamWriter(File, true);
                    ((StreamWriter)fileWriter).AutoFlush = true;
                }
            }
            public readonly RestoreException FirstRestoreException;

            public class RestoreException : Exception
            {
                public RestoreException(string message) : base(message)
                { }
            }

            protected enum Action
            {
                added,
                replaced,
                inserted,
                deleted,
            }

            protected void writeAction(Action action, int index, DocumentT document)
            {
                lock (this)
                {
                    fileWriter.WriteLine(action.ToString() + ": " + index);
                    fileWriter.WriteLine(JsonConvert.SerializeObject(document, Formatting.None));
                }
            }
            protected void writeDeleted(int index)
            {
                lock (this)
                {
                    fileWriter.WriteLine(Action.deleted + ": " + index);
                }
            }

            (Action action, int number) readActionLine(string line)
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
                    case Action.inserted:
                        return (Action.inserted, int.Parse(m.Groups[2].Value));
                    default:
                        throw new Exception("Unknown action: " + action);
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
                    if (fileWriter != null)
                        fileWriter.Close();

                    string newFile = File + ".new";
                    using (TextWriter newFileWriter = new StreamWriter(newFile, false))
                    {
                        foreach (DocumentT d in documents)
                            newFileWriter.WriteLine(JsonConvert.SerializeObject(d, Formatting.None));
                        newFileWriter.Flush();
                    }

                    if (System.IO.File.Exists(File))
                        System.IO.File.Delete(File);
                    System.IO.File.Move(newFile, File);

                    fileWriter = new StreamWriter(File, true);
                    ((StreamWriter)fileWriter).AutoFlush = true;
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
                }
            }

            /// <summary>
            /// Adds a document to the table if it does not exists. Otherwise, overwrites it.
            /// Table works as an ordered HashSet.
            /// </summary>
            /// <param name="document"></param>
            /// <returns></returns>
            virtual public Result Save(DocumentT document)
            {
                lock (this)
                {
                    int i = documents.IndexOf(document);
                    if (i >= 0)
                    {
                        writeAction(Action.replaced, i, document);
                        Saved?.Invoke(document, false);
                        return Result.UPDATED;
                    }
                    else
                    {
                        documents.Add(document);
                        writeAction(Action.added, documents.Count - 1, document);
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
            virtual public Result Add(DocumentT document)
            {
                lock (this)
                {
                    int i = documents.IndexOf(document);
                    if (i >= 0)
                    {
                        documents.RemoveAt(i);
                        documents.Add(document);
                        writeDeleted(i);
                        writeAction(Action.added, documents.Count - 1, document);
                        Saved?.Invoke(document, false);
                        return Result.MOVED2TOP;
                    }
                    else
                    {
                        documents.Add(document);
                        writeAction(Action.added, documents.Count - 1, document);
                        Saved?.Invoke(document, true);
                        return Result.ADDED;
                    }
                }
            }

            public void AddRange(IEnumerable<DocumentT> documents)
            {
                lock (this)
                {
                    foreach (DocumentT document in documents)
                        Add(document);
                }
            }

            /// <summary>
            /// Table works as an ordered HashSet
            /// </summary>
            /// <param name="document"></param>
            virtual public Result Insert(int index, DocumentT document)
            {
                lock (this)
                {
                    int i = documents.IndexOf(document);
                    if (i >= 0)
                    {
                        documents.RemoveAt(i);
                        documents.Insert(index, document);
                        writeAction(Action.replaced, i, document);
                        Saved?.Invoke(document, false);
                        return Result.MOVED;
                    }
                    else
                    {
                        documents.Insert(index, document);
                        writeAction(Action.inserted, index, document);
                        Saved?.Invoke(document, true);
                        return Result.INSERTED;
                    }
                }
            }

            public IEnumerable<DocumentT> Find(Predicate<DocumentT> match)
            {
                lock (this)
                {
                    List<DocumentT> matchedDocuments = new List<DocumentT>();
                    foreach (DocumentT document in documents)
                        if (match(document))
                            yield return document;
                }
            }

            public void InsertRange(int index, IEnumerable<DocumentT> documents)
            {
                lock (this)
                {
                    throw new Exception("TBD");
                    //base.InsertRange(index, documents);
                }
            }

            public bool Remove(DocumentT document)
            {
                lock (this)
                {
                    int i = documents.IndexOf(document);
                    if (i >= 0)
                    {
                        documents.RemoveAt(i);
                        writeDeleted(i);
                        Removed?.Invoke(document, true);
                        return true;
                    }
                    Removed?.Invoke(document, false);
                    return false;
                }
            }

            public int RemoveAll(Predicate<DocumentT> match)
            {
                lock (this)
                {
                    List<DocumentT> documentsToDelete = new List<DocumentT>();
                    foreach (DocumentT document in documents)
                        if (match(document))
                            documentsToDelete.Add(document);
                    foreach (DocumentT document in documentsToDelete)
                        Remove(document);
                    return documentsToDelete.Count;
                }
            }

            public void RemoveAt(int index)
            {
                lock (this)
                {
                    documents.RemoveAt(index);
                    writeDeleted(index);
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

            public DocumentT First(Predicate<DocumentT> match = null)
            {
                lock (this)
                {
                    if (match == null)
                        return documents.First();
                    return documents.First(new Func<DocumentT, bool>(match));
                }
            }

            public DocumentT Last(Predicate<DocumentT> match = null)
            {
                lock (this)
                {
                    if (match == null)
                        return documents.Last();
                    return documents.Last(new Func<DocumentT, bool>(match));
                }
            }

            //public DocumentT? GetPrevious(DocumentT document)
            //{
            //    if (document == null)
            //        return null;
            //    int i = IndexOf(document);
            //    if (i < 1)
            //        return null;
            //    return (DocumentT?)this[i - 1];
            //}

            //public DocumentT? GetNext(DocumentT document)
            //{
            //    if (document == null)
            //        return null;
            //    int i = this.IndexOf(document);
            //    if (i + 1 >= this.Count)
            //        return null;
            //    return (DocumentT?)this[i + 1];
            //}
        }
    }
}
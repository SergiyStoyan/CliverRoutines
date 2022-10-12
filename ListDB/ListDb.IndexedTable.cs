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
using Newtonsoft.Json;

namespace Cliver
{
    public partial class ListDb
    {
        public class IndexedDocument
        {
            public long ID { get; internal set; }
        }

        //public static IndexedTable<DocumentType> GetIndexedTable<DocumentType>(string directory = null) where DocumentType : IndexedDocument, new()
        //{
        //    return IndexedTable<DocumentType>.Get(directory);
        //}

        //public static TableType Get<TableType, DocumentType>(string directory = null) where TableType : Table<DocumentType>, IDisposable where DocumentType : IndexedDocument, new()
        //{
        //    directory = Table < DocumentType > .getNormalizedDirectory(directory);
        //    WeakReference wr;
        //    string key = directory + System.IO.Path.DirectorySeparatorChar + typeof(DocumentType).Name;
        //    lock (tableKeys2table)
        //    {
        //        if (!tableKeys2table.TryGetValue(key, out wr)
        //            || !wr.IsAlive
        //            )
        //        {
        //            TableType t = new TableType(directory, key);
        //            wr = new WeakReference(t);
        //            tableKeys2table[key] = wr;
        //        }
        //    }
        //    return (TableType)wr.Target;
        //}

        public class IndexedTable<DocumentType> : Table<DocumentType>, IDisposable where DocumentType : IndexedDocument, new()
        {
            new public static IndexedTable<DocumentType> Get(string directory = null)
            {
                directory = getNormalizedDirectory(directory);
                WeakReference wr;
                string key = directory + System.IO.Path.DirectorySeparatorChar + typeof(DocumentType).Name;
                lock (tableKeys2table)
                {
                    if (!tableKeys2table.TryGetValue(key, out wr)
                        || !wr.IsAlive
                        )
                    {
                        IndexedTable<DocumentType> t = new IndexedTable<DocumentType>(directory, key);
                        wr = new WeakReference(t);
                        tableKeys2table[key] = wr;
                    }
                }
                return (IndexedTable<DocumentType>)wr.Target;
            }

            IndexedTable(string directory, string key) : base(directory, key)
            {
            }

            /// <summary>
            /// Table works as an ordered HashSet
            /// </summary>
            /// <param name="document"></param>
            /// <returns></returns>
            override public Result Save(DocumentType document)
            {
                lock (this)
                {
                    int i = documents.IndexOf(document);
                    if (i >= 0)
                    {
                        fileWriter.WriteLine(JsonConvert.SerializeObject(document, Formatting.None));
                        writeLogLine(Action.replaced, i);
                       invokeSaved( document, false);
                        return Result.UPDATED;
                    }
                    else
                    {
                        setNewId(document);
                        fileWriter.WriteLine(JsonConvert.SerializeObject(document, Formatting.None));
                        documents.Add(document);
                        writeLogLine(Action.added, documents.Count - 1);
                        invokeSaved(document, true);
                        return Result.ADDED;
                    }
                }
            }

            void setNewId(DocumentType document)
            {
                lock (this)
                {
                    System.Reflection.PropertyInfo pi = typeof(DocumentType).GetProperty("ID");
                    pi.SetValue(document, DateTime.Now.Ticks);
                }
            }

            public DocumentType GetById(int documentId)
            {
                lock (this)
                {
                    return documents.Where(x => x.ID == documentId).FirstOrDefault();
                }
            }

            /// <summary>
            /// Table works as an ordered HashSet
            /// </summary>
            /// <param name="document"></param>
            override public Result Add(DocumentType document)
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
                        invokeSaved(document, false);
                        return Result.MOVED2TOP;
                    }
                    else
                    {
                        setNewId(document);
                        fileWriter.WriteLine(JsonConvert.SerializeObject(document, Formatting.None));
                        documents.Add(document);
                        writeLogLine(Action.added, documents.Count - 1);
                        invokeSaved(document, true);
                        return Result.ADDED;
                    }
                }
            }

            /// <summary>
            /// Table works as an ordered HashSet
            /// </summary>
            /// <param name="index"></param>
            /// <param name="document"></param>
            override public Result Insert(int index, DocumentType document)
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
                        invokeSaved(document, false);
                        return Result.MOVED;
                    }
                    else
                    {
                        setNewId(document);
                        fileWriter.WriteLine(JsonConvert.SerializeObject(document, Formatting.None));
                        documents.Insert(index, document);
                        writeLogLine(Action.inserted, index);
                        invokeSaved(document, true);
                        return Result.INSERTED;
                    }
                }
            }
        }
    }
}
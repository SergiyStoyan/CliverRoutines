/********************************************************************************************
        Author: Sergiy Stoyan
        s.y.stoyan@gmail.com, sergiy.stoyan@outlook.com, stoyan@cliversoft.com
        http://www.cliversoft.com
********************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Cliver
{
    /// <summary>
    /// Features:
    /// - auto-disposing IDisposable Values which have left the list;
    /// </summary>
    /// <typeparam name="VT"></typeparam>
    public class HandyList<VT> : IDisposable, IEnumerable<VT> //where VT: class
    {
        public HandyList()
        {

        }

        public HandyList(IEnumerable<VT> list)
        {
            Values = list.ToList();
        }

        ~HandyList()
        {
            Dispose();
        }

        virtual public void Dispose()
        {
            lock (this)
            {
                if (Values != null)
                {
                    Clear();
                    Values = null;
                }
            }
        }

        virtual public void Clear()
        {
            lock (this)
            {
                foreach (VT v in Values)
                    if (v != null && v is IDisposable)
                        ((IDisposable)v).Dispose();
                Values.Clear();
            }
        }

        public void RemoveAt(int index)
        {
            lock (this)
            {                
                    dispose(Values[index]);
                Values.RemoveAt(index);
            }
        }

        void dispose(VT value)
        {
            lock (this)
            {
                if (value == null || !(value is IDisposable))
                    return;
                int vKeyCount = 0;
                Values.Where(a => a.Equals(value)).TakeWhile(a => ++vKeyCount < 2);
                if (vKeyCount < 2)//make sure it is the only inclusion of the object
                    ((IDisposable)value).Dispose();
            }
        }

        /// <summary>
        /// It is safe: returns default if does not exists.
        /// To check for existance, use TryGetValue().
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public VT this[int index]
        {
            get
            {
                lock (this)
                {
                    return Values[index];
                }
            }
            set
            {
                lock (this)
                {
                    VT v = Values[index];
                    if (v != null && !v.Equals(value))
                    {
                        int vKeyCount = 0;
                        Values.Where(a => a.Equals(v)).TakeWhile(a => ++vKeyCount < 2);
                        if (vKeyCount < 2)//make sure it is the only inclusion of the object
                            dispose(v);
                    }
                    Values[index] = value;
                }
            }
        }

        /// <summary>
        /// Underlaying List which can be used for the ordinary operations.
        /// </summary>
    public    List<VT> Values { get; private set; } = new List<VT>();

        public IEnumerator<VT> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(VT value)
        {
           Values.Add(value);
        }

        public int Count
        {
            get
            {
                return Values.Count;
            }
        }

        public List<VT> GetRange(int index, int count)
        {
            return Values.GetRange(index, count);
        }
    }
}
/********************************************************************************************
        Author: Sergiy Stoyan
        s.y.stoyan@gmail.com, sergiy.stoyan@outlook.com, stoyan@cliversoft.com
        http://www.cliversoft.com
********************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cliver
{
    public class Event<ArgumentT>
    {
        List<Handler> Handlers = new List<Handler>();

        public class Handler
        {
            public Action<ArgumentT> Action
            {
                get
                {
                    if (whileAliveObjectWeakRef == null || whileAliveObjectWeakRef.TryGetTarget(out object o))
                        return action;
                    return null;
                }
            }
            Action<ArgumentT> action;
            readonly WeakReference<object> whileAliveObjectWeakRef;

            public readonly bool Synchronous;

            internal Handler(Action<ArgumentT> action, bool synchronous, object whileAliveObject)
            {
                this.action = action;
                if (whileAliveObject != null)
                    whileAliveObjectWeakRef = new WeakReference<object>(whileAliveObject, false);
                Synchronous = synchronous;
            }
        }

        public int HandlersCount { get { return Handlers.Count; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="synchronous"></param>
        /// <param name="uniqueAction"></param>
        /// <param name="whileAliveObject">if set, callback depends on the weak reference to this object</param>
        /// <returns></returns>
        public Handler AddHandler(Action<ArgumentT> action, bool synchronous, bool uniqueAction = true, object whileAliveObject = null)
        {
            lock (this)
            {
                if (uniqueAction && Handlers.Find(a => a.Action == action) != null)
                    return null;
                Handler h = new Handler(action, synchronous, whileAliveObject);
                Handlers.Add(h);
                return h;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="beforeHandler"></param>
        /// <param name="action"></param>
        /// <param name="synchronous"></param>
        /// <param name="uniqueAction"></param>
        /// <param name="whileAliveObject">if set, callback depends on the weak reference to this object</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Handler InsertHandler(Handler beforeHandler, Action<ArgumentT> action, bool synchronous, bool uniqueAction = true, object whileAliveObject = null)
        {
            lock (this)
            {
                if (uniqueAction && Handlers.Find(a => a.Action == action) != null)
                    return null;

                int bhi = Handlers.IndexOf(beforeHandler);
                if (bhi < 0)
                    throw new Exception("No beforeHandler found: " + beforeHandler.ToString());
                Handler h = new Handler(action, synchronous, whileAliveObject);
                Handlers.Insert(bhi, new Handler(action, synchronous, whileAliveObject));
                return h;
            }
        }

        public IEnumerable<Handler> FindHandlers(Action<ArgumentT> action)
        {
            lock (this)
            {
                return Handlers.Select((handler, index) => (index, handler)).Where(a => a.handler.Action == action).Select(a => a.handler);
            }
        }

        public IEnumerable<Handler> GetHandlers()
        {
            lock (this)
            {
                return Handlers.Select(a => a);
            }
        }

        public void RemoveHandler(Handler handler)
        {
            lock (this)
            {
                while (Handlers.Remove(handler)) ;
            }
        }

        public void RemoveAllHandlers()
        {
            lock (this)
            {
                Handlers.Clear();
            }
        }

        public void RemoveHandlers(Action<ArgumentT> action)
        {
            lock (this)
            {
                Handlers.RemoveAll(a => a.Action == action);
            }
        }

        public bool __Subscribed
        {
            get
            {
                return Handlers.Count > 0;
            }
        }

        public Event<ArgumentT> CreateCopy()
        {
            lock (this)
            {
                var e = new Event<ArgumentT>();
                Handlers.ForEach(a => { e.Handlers.Add(a); });
                return e;
            }
        }

        public Action<ArgumentT> __Subscription
        {
            get
            {
                return Handlers.Count > 0 ? __Trigger : (Action<ArgumentT>)null;
            }
        }

        public void __Trigger(ArgumentT argument)
        {
            lock (this)
            {
                for (int i = Handlers.Count - 1; i >= 0; i--)
                {
                    var h = Handlers[i];
                    var a = h.Action;
                    if (a == null)
                    {
                        Handlers.RemoveAt(i);
                        continue;
                    }
                    if (h.Synchronous)
                        a.Invoke(argument);
                    else
                        a.BeginInvoke(argument);
                }
            }
        }
    }

    //public class Event2<DelegateT> where DelegateT : Delegate
    //{
    //    List<Handler> Handlers = new List<Handler>();

    //    public class Handler
    //    {
    //        public DelegateT Delegate { get; internal set; }
    //        public bool Synchronous { get; internal set; }

    //        internal Handler() { }
    //    }

    //    public int HandlersCount { get { return Handlers.Count; } }

    //    public Handler AddHandler(DelegateT  @delegate, bool synchronous, bool uniqueAction = true)
    //    {
    //        lock (this)
    //        {
    //            if (uniqueAction && Handlers.Find(a => a.Delegate == @delegate) != null)
    //                return null;
    //            Handler h = new Handler { Delegate = @delegate, Synchronous = synchronous };
    //            Handlers.Add(h);
    //            return h;
    //        }
    //    }

    //    public Handler InsertHandler(Handler beforeHandler, DelegateT @delegate, bool synchronous, bool uniqueAction = true)
    //    {
    //        lock (this)
    //        {
    //            if (uniqueAction && Handlers.Find(a => a.Delegate == @delegate) != null)
    //                return null;

    //            int bhi = Handlers.IndexOf(beforeHandler);
    //            if (bhi < 0)
    //                throw new Exception("No beforeHandler found: " + beforeHandler.ToString());
    //            Handler h = new Handler { Delegate = @delegate, Synchronous = synchronous };
    //            Handlers.Insert(bhi, new Handler { Delegate = @delegate, Synchronous = synchronous });
    //            return h;
    //        }
    //    }

    //    public IEnumerable<Handler> FindHandlers(DelegateT @delegate)
    //    {
    //        lock (this)
    //        {
    //            return Handlers.Select((handler, index) => (index, handler)).Where(a => a.handler.Delegate == @delegate).Select(a => a.handler);
    //        }
    //    }

    //    public IEnumerable<Handler> GetHandlers()
    //    {
    //        lock (this)
    //        {
    //            return Handlers.Select(a => a);
    //        }
    //    }

    //    public void RemoveHandler(Handler handler)
    //    {
    //        lock (this)
    //        {
    //            while (Handlers.Remove(handler)) ;
    //        }
    //    }

    //    public void RemoveAllHandlers()
    //    {
    //        lock (this)
    //        {
    //            Handlers.Clear();
    //        }
    //    }

    //    public void __Trigger(DelegateT argument)
    //    {
    //        lock (this)
    //        {
    //            foreach (var h in Handlers)
    //                if (h.Synchronous)
    //                    h.Delegate.Invoke(argument);
    //                else
    //                    h.Delegate.BeginInvoke(argument);
    //        }
    //    }
    //}
}
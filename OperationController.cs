//********************************************************************************************
//Author: Sergiy Stoyan
//        s.y.stoyan@gmail.com, sergiy.stoyan@outlook.com, stoyan@cliversoft.com
//        http://www.cliversoft.com
//********************************************************************************************
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Cliver
{
    /// <summary>
    /// A standardized API between an operation method/class and its invoker which is usually GUI.
    /// (!)The same instance must be available to both: the operation method/class and its invoker.
    /// Can be inherited and enhanced with methods like OnProgress() etc.
    /// Provides:
    /// - safely aborting of the operation;
    /// - event entries;
    /// - async methods;
    /// </summary>
    public class OperationController
    {
        public readonly string Title;
        //public virtual string Title
        //{
        //    get { return title; }
        //    set
        //    {
        //        if (title != null)
        //            throw new Exception(nameof(Title) + " must be set only once.");
        //        title = value;
        //    }
        //}
        //string title = null;

        public OperationController(string title = null)
        {
            Title = title;
        }

        public OperationController(string title, Action operation) : this(title)
        {
            Operation = operation;
        }

        /// <summary>
        /// Must be called by the invoker.
        /// </summary>
        /// <returns></returns>
        public OperationStatus Perform()
        {
            OperationStatus status = OperationStatus.Running;
            try
            {
                OnStart.ForEach(a => a());
                Operation();
                OnCompletion.ForEach(a => a());
                status = OperationStatus.Completed;
            }
            catch (Exception e)
            {
                status = Aborting ? OperationStatus.Aborted : OperationStatus.Failed;
                OnException.ForEach(a => a(e));
            }
            finally
            {
                OnFinally.ForEach(a => a());
                Status = status;
            }
            return Status;
        }

        readonly public List<Action> OnStart = new List<Action>();

        readonly public List<Action> OnCompletion = new List<Action>();

        readonly public List<Action> OnFinally = new List<Action>();

        readonly public List<Action<Exception>> OnException = new List<Action<Exception>>();

        /// <summary>
        /// The operation must throw exception on error. Otherwise it is considered completed successfully.
        /// </summary>
        public virtual Action Operation
        {
            get { return operation; }
            set
            {
                if (operation != null)
                    throw new Exception(nameof(Operation) + " must be set only once.");
                operation = value;
            }
        }
        Action operation = null;

        /// <summary>
        /// Provides aborting mechanisms, usually by causing exceptions within the operation code.
        /// Must be called from the operation method.
        /// </summary>
        /// <param name="actions"></param>
        /// <exception cref="Exception"></exception>
        public void AddAbortingActions(params Action[] actions)
        {
            if (Aborting)
                throw new Exception("Aborting.");
            abortingActions.AddRange(actions);
        }
        List<Action> abortingActions = new List<Action>();

        public bool Abort(int timeoutMss)
        {
            Aborting = true;
            abortingActions.ForEach(a => a());
            return SleepRoutines.WaitForCondition(() => { return Status >= OperationStatus.Running; }, timeoutMss, 100);
        }
        public bool Aborting { get; private set; } = false;

        public enum OperationStatus
        {
            Created,
            Running,
            Completed,
            Aborted,
            Failed
        }
        public OperationStatus Status { get; private set; } = OperationStatus.Created;

        async public Task<OperationStatus> PerformAsync()
        {
            return await Task.Run(Perform);
        }

        async public Task<bool> AbortAsync(int timeoutMss)
        {
            return await Task.Run(() => { return Abort(timeoutMss); });
        }
    }
}
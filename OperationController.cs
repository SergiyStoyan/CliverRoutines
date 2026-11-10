//********************************************************************************************
//Author: Sergiy Stoyan
//        s.y.stoyan@gmail.com, sergiy.stoyan@outlook.com, stoyan@cliversoft.com
//        http://www.cliversoft.com
//********************************************************************************************
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
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

        //public OperationController(string title, Func<Task> operationAsync) : this(title)
        //{
        //    OperationAsync = operationAsync;
        //}

        /// <summary>
        /// Must be called by the invoker.
        /// </summary>
        /// <returns></returns>
        public OperationStatus Perform()
        {
            try
            {
                OnStart.ForEach(a => a());

                //if (Operation != null)
                    Operation();
                //else if (OperationAsync != null)
                //    TaskRoutines.RunSync(OperationAsync);
                //else
                //    throw new Exception("Operation is not set.");

                OnCompletion.ForEach(a => a());
                Status = OperationStatus.Completed;
            }
            catch (Exception e)
            {
                Status = Aborting ? OperationStatus.Aborted : OperationStatus.Failed;
                OnException.ForEach(a => a(e));
            }
            finally
            {
                OnFinally.ForEach(a => a());
            }
            return Status;
        }

        //async public Task<OperationStatus> PerformAsync()
        //{
        //    try
        //    {
        //        OnStart.ForEach(a => a());

        //        if (Operation != null)
        //            await Task.Run(Operation);
        //        else if (OperationAsync != null)
        //            await OperationAsync();
        //        else
        //            throw new Exception("Operation is not set.");

        //        OnCompletion.ForEach(a => a());
        //        Status = OperationStatus.Completed;
        //    }
        //    catch (Exception e)
        //    {
        //        Status = Aborting ? OperationStatus.Aborted : OperationStatus.Failed;
        //        OnException.ForEach(a => a(e));
        //    }
        //    finally
        //    {
        //        OnFinally.ForEach(a => a());
        //    }
        //    return Status;
        //}
        async public Task<OperationStatus> PerformAsync()
        {
            return await Task.Run(Perform);
        }

        readonly public List<Action> OnStart = new List<Action>();

        readonly public List<Action> OnCompletion = new List<Action>();

        readonly public List<Action> OnFinally = new List<Action>();

        readonly public List<Action<Exception>> OnException = new List<Action<Exception>>();

        /// <summary>
        /// The operation must throw exception on error, otherwise it is considered completed successfully.
        /// (!)It must not be async, otherwise the invoker will not be able to catch exceptions.
        /// </summary>
        public virtual Action Operation
        {
            get { return operation; }
            set
            {
                //if (operationAsync != null)
                //    throw new Exception($"{nameof(OperationAsync)} and {nameof(Operation)} must not be set together.");
                if (operation != null)
                    throw new Exception(nameof(Operation) + " must be set only once.");
                operation = value;
            }
        }
        Action operation = null;

        //public virtual Func<Task> OperationAsync
        //{
        //    get { return operationAsync; }
        //    set
        //    {
        //        if (operation != null)
        //            throw new Exception($"{nameof(OperationAsync)} and {nameof(Operation)} must not be set together.");
        //        if (operationAsync != null)
        //            throw new Exception(nameof(OperationAsync) + " must be set only once.");
        //        operationAsync = value;
        //    }
        //}
        //Func<Task> operationAsync = null;

        //bool isOperationAsync()
        //{
        //    MethodInfo method = Operation.Method;
        //    return method.GetCustomAttribute<AsyncStateMachineAttribute>() != null;
        //}

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

        async public Task<bool> AbortAsync(int timeoutMss)
        {
            return await Task.Run(() => { return Abort(timeoutMss); });
        }
    }
}
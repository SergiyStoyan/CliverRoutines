//********************************************************************************************
//Author: Sergiy Stoyan
//        s.y.stoyan@gmail.com, sergiy.stoyan@outlook.com, stoyan@cliversoft.com
//        http://www.cliversoft.com
//********************************************************************************************
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cliver
{
    /// <summary>
    /// A base class for a custom operation that exposes a standardized API to the operation invoker which is usually GUI.
    /// See OperationController for details.
    /// </summary>
    public class Operation
    {
        public Operation(string title = null)
        {
            OperationController = new OperationController(title);
            //(!)must be set by the heir
            //OperationController.Operation = () => { ?(?,...) };
        }

        readonly public OperationController OperationController;
    }

    /// <summary>
    /// A base class for a custom operation that exposes a standardized API to the operation invoker which is usually GUI.
    /// See OperationController for details.
    /// </summary>
    abstract public class Operation2
    {
        public Operation2(string title = null)
        {
            OperationController = new OperationController(title == null ? GetType().Name : title, Do);
        }

        readonly public OperationController OperationController;

        protected abstract void Do();
    }

    /// <summary>
    /// A base class for a custom operation that exposes a standardized API to the operation invoker which is usually GUI.
    /// See OperationController for details.
    /// </summary>
    public class Operation3
    {
        public Operation3(string title, string methodName, params object[] parameters)
        {
            OperationController = new OperationController(title);
            OperationController.Operation = () => { GetType().InvokeMember(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, this, parameters); };
        }

        public Operation3(string methodName, params object[] parameters) : this(null, methodName, parameters)
        {
        }

        readonly public OperationController OperationController;
    }
}
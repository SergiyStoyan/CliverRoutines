//********************************************************************************************
//Author: Sergiy Stoyan
//        s.y.stoyan@gmail.com, sergiy.stoyan@outlook.com, stoyan@cliversoft.com
//        http://www.cliversoft.com
//********************************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;

namespace Cliver
{
    /// <summary>
    /// (!)OperationController might be a more practical choice.
    /// A base class for a custom operation that exposes a standard API to its invoker which is usually GUI.
    /// Can be inherited and enhanced with custom methods like OnProgress() etc.
    /// Provides:
    /// - safely aborting of the operation;
    /// - event entries;
    /// - async methods;
    /// </summary>
    abstract public class Operation : OperationController
    {
        public Operation()
        {
            this.Operation = Body;
        }

        //new public virtual string Title { get { return GetType().Name; } protected set { throw new NotImplementedException(); } }

        protected abstract void Body();
    }
}
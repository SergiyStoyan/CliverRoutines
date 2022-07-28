//********************************************************************************************
//Author: Sergey Stoyan
//        sergey.stoyan@gmail.com
//        sergey.stoyan@hotmail.com
//        http://www.cliversoft.com
//********************************************************************************************

//################################
// 
// (!!!) When using a certain database type include the respective implementation files into the project and add the required reference.
//
//################################

using System;
using System.Data;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Cliver.Db
{
    public abstract class Connection : IDisposable
    {
        ~Connection()
        {
            Dispose();
        }

        public virtual void Dispose()
        {
            lock (this)
            {
                foreach (Command dc in sqls2command.Values)
                    dc.Dispose();
                connection?.Dispose();
            }
        }

        //static Connection()
        //{
        //    This = Create();
        //}

        //public static Connection This { get; protected set; }

        //public static Connection Create(string connectionString = null)
        //{
        //    if (connectionString == null)
        //        throw new Exception("connectionString is null.");

        //    if (Regex.IsMatch(connectionString, @"\.mdf|\.sdf|Initial\s+Catalog\s*=", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline))
        //        return new MsSqlConnection(connectionString);
        //    if (Regex.IsMatch(connectionString, @"\.mdf|\.sdf|Initial\s+Catalog\s*=", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline))
        //        return new MySqlConnection(connectionString);
        //    throw new Exception("Could not detect an appropriate wrapper class for " + connectionString);
        //}

        //public static Connection CreateFromNativeConnection(object connection)
        //{
        //    if (connection == null)
        //        throw new Exception("Connection is null.");

        //    if (connection is System.Data.SqlClient.SqlConnection)
        //    {
        //        System.Data.SqlClient.SqlConnection c = (System.Data.SqlClient.SqlConnection)connection;
        //        if (c.State != ConnectionState.Open)
        //            c.Open();
        //        return new MsSqlConnection(c);
        //    }
        //    throw new Exception("Could not detect an appropriate wrapper class for " + ((System.Data.SqlClient.SqlConnection)connection).ConnectionString);
        //}

        protected Connection(string connectionString = null, Log.MessageType logDefaultMessageType = Log.MessageType.DEBUG)
        {
            ConnectionString = connectionString;
            LogDefaultMessageType = logDefaultMessageType;
        }

        protected Connection(System.Data.Common.DbConnection connection, Log.MessageType logDefaultMessageType = Log.MessageType.DEBUG)
        {
            this.connection = connection;
            ConnectionString = connection.ConnectionString;
            LogDefaultMessageType = logDefaultMessageType;
        }

        public readonly string ConnectionString;
        public Log.MessageType LogDefaultMessageType = Log.MessageType.DEBUG;

        /// <summary>
        /// Current database
        /// </summary>
        public string Database
        {
            get
            {
                return connection.Database;
            }
        }

        /// <summary>
        /// Native connection that must be casted.
        /// </summary>
        public System.Data.Common.DbConnection RefreshedNativeConnection
        {
            get
            {
                lock (sqls2command)
                {
                    return getRefreshedNativeConnection();
                }
            }
        }
        protected abstract System.Data.Common.DbConnection getRefreshedNativeConnection();
        protected System.Data.Common.DbConnection connection;

        /// <summary>
        /// Creates and caches/retrieves a command.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        public Command this[string sql, Cliver.Log.MessageType? logMessageType = null]
        {
            get
            {
                lock (sqls2command)
                {
                    Command c;
                    if (!sqls2command.TryGetValue(sql, out c))
                    {
                        c = createCommand(sql);
                        sqls2command[sql] = c;
                    }
                    if (logMessageType != null)
                        c.LogMessageType = (Cliver.Log.MessageType)logMessageType;
                    return c;
                }
            }
        }
        abstract protected Command createCommand(string sql, Cliver.Log.MessageType? logMessageType = null);
        protected Dictionary<string, Command> sqls2command = new Dictionary<string, Command>();

        /// <summary>
        /// Creates a not cached command.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public Command Get(string sql, Cliver.Log.MessageType? logMessageType = null)
        {
            return createCommand(sql, logMessageType);
        }

        public void Close()
        {
            connection.Close();
        }
    }
}
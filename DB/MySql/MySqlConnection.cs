//********************************************************************************************
//Author: Sergey Stoyan
//        sergey.stoyan@gmail.com
//        sergey.stoyan@hotmail.com
//        http://www.cliversoft.com
//********************************************************************************************

using System;
using System.Data;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace Cliver.Db
{
    public class MySqlConnection : Connection
    {
        public MySqlConnection(string connectionString = null, Log.MessageType logDefaultMessageType = Log.MessageType.DEBUG)
            : base(connectionString, logDefaultMessageType)
        {
            connection = new MySql.Data.MySqlClient.MySqlConnection(ConnectionString);
        }

        public MySqlConnection(MySql.Data.MySqlClient.MySqlConnection connection, Log.MessageType logDefaultMessageType = Log.MessageType.DEBUG)
            : base(connection, logDefaultMessageType)
        {
        }

        override protected System.Data.Common.DbConnection getRefreshedNativeConnection()
        {
            if (connection.State != ConnectionState.Open)
            {
                //if (connection.State != ConnectionState.Closed)
                //{
                //    connection.Dispose();
                //    connection = new SqlConnection(connection.ConnectionString);
                //}
                connection.Open();
                Dictionary<string, Command> s2cs = new Dictionary<string, Command>();
                foreach (string sql in sqls2command.Keys)
                    s2cs[sql] = new MsSqlCommand(sql, this);
                sqls2command = s2cs;
            }
            return connection;
        }

        override protected Command createCommand(string sql, Cliver.Log.MessageType? logMessageType = null)
        {
            var c = new MySqlCommand(sql, this, logMessageType);
            return c;
        }
    }
}
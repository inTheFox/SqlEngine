using System;
using System.Collections.Generic;
using System.Text;
using Gangs.SqlEngine;

namespace SqlEngine.SqlEngine
{
    public class SqlContextFactory
    {
        private readonly object _contextInstance;
        private readonly string _connectionQuery;

        /// <summary>
        /// Standard construct
        /// </summary>
        /// <param name="contextInstance">Database Context object</param>
        /// <param name="connectionQuery">Connection string</param>
        public SqlContextFactory(object contextInstance, string connectionQuery)
        {
            this._contextInstance = contextInstance;
            this._connectionQuery = connectionQuery;
        }

        /// <summary>
        /// Set table to engine
        /// </summary>
        /// <typeparam name="T">Table type</typeparam>
        /// <returns>Created SqlEngine<T> instance</returns>
        public SqlEngine<T> SetTable<T>()
        {
            return new SqlEngine<T>(this._contextInstance, this._connectionQuery);
        }
    }
}

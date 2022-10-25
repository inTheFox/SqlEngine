using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using MySqlX.XDevAPI.CRUD;
using MySqlX.XDevAPI.Relational;

namespace Gangs.SqlEngine
{
    public class SqlTable
    {
        public string TableName { get; set; }
        public IEnumerable<SqlColumn> Columns { get; set; }
        public readonly object _sqlEngine;

        public SqlTable(object sqlEngine)
        {
            this._sqlEngine = sqlEngine;
            this.Columns = new List<SqlColumn>();
        }

        public SqlTable(object sqlEngine, string tableName, IEnumerable<SqlColumn> columns) : this(sqlEngine)
        {
            TableName = tableName;
            Columns = columns;
        }
    }

    public static class TableExtension
    {
        public static void Add(this SqlTable table, object row)
        {
            MethodInfo method = table._sqlEngine.GetType().GetMethod("Add");
            method.Invoke(table._sqlEngine, new object[] { row });
        }
        public static T Add<T>(this SqlTable table, object row)
        {
            MethodInfo method = table._sqlEngine.GetType().GetMethod("Add");
            T result = (T)method.Invoke(table._sqlEngine, new object[] { row });
            return result;
        }
        public static List<T> ToList<T>(this SqlTable table)
        {
            MethodInfo method = table._sqlEngine.GetType().GetMethod("ToList");
            List<T> result = (List<T>)method.Invoke(table._sqlEngine, new object[] {});
            return result;
        }
        public static void Remove(this SqlTable table, object entity)
        {
            MethodInfo method = table._sqlEngine.GetType().GetMethod("Remove");
            method.Invoke(table._sqlEngine, new object[] {entity});
        }
        public static TEntity FirstOrDefault<TEntity>(this SqlTable table, Func<TEntity, bool> predicate)
        {
            MethodInfo method = table._sqlEngine.GetType().GetMethod("FirstOrDefault");
            TEntity result = (TEntity)method.Invoke(table._sqlEngine, new object[] { predicate });
            return result;
        }
        public static IEnumerable<TEntity> Where<TEntity>(this SqlTable table, Func<TEntity, bool> predicate)
        {
            MethodInfo method = table._sqlEngine.GetType().GetMethod("Where");
            IEnumerable<TEntity> result = (IEnumerable<TEntity>)method.Invoke(table._sqlEngine, new object[] { predicate });
            return result;
        }
    }
}

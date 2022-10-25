using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Gangs.Attributes;

namespace Gangs.SqlEngine
{
    public class SqlEngineModelParser : IDisposable
    {
        private IEnumerable<Type> supportedTypes = new List<Type>()
        {
            typeof(string),
            typeof(int),
            typeof(float)
        };

        private readonly object _sqlEngine;

        public SqlEngineModelParser(object sqlEngine)
        {
            this._sqlEngine = sqlEngine;
        }

        public SqlTable? ParseModel(Type type)
        {
            SqlTable table;
            List<SqlColumn> columns = new List<SqlColumn>();

            string tableName = type.Name;
            SqlTableNameAttribute? atr = (SqlTableNameAttribute)type.GetCustomAttributes(false).FirstOrDefault(p => p is SqlColumnNameAttribute);
            if (!(atr is null))
            {
                tableName = atr.TableName;
            }

            foreach (PropertyInfo property in type.GetProperties())
            {
                if (supportedTypes.Contains(property.PropertyType))
                {
                    bool isPrimaryKey =
                        !(property.GetCustomAttributes(false).FirstOrDefault(p => p is SqlKeyIntegerAttribute) is null);
                    string columnName = property.Name;

                    if (property.GetCustomAttributes(false).Contains(typeof(SqlColumnNameAttribute)))
                        columnName = property.GetCustomAttribute<SqlColumnNameAttribute>()?.Name;

                    columns.Add(new SqlColumn(columnName, property.PropertyType, isPrimaryKey));
                }
            }
            table = new SqlTable(this._sqlEngine, tableName, columns); return table;
        }
        

        public SqlTable? ParseModel<T>() => ParseModel(typeof(T));

        public void Dispose()
        {
            supportedTypes = null;
        }
    }
}

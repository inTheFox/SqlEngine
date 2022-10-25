using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Gangs.Attributes;

namespace Gangs.SqlEngine
{
    public class SqlEngineCommandParser<T> : IDisposable
    {
        private readonly SqlTable table;
        private Dictionary<Type, string>? mySqlTypes;

        public SqlEngineCommandParser(SqlTable sqlTable)
        {
            this.table = sqlTable;

            this.mySqlTypes = new Dictionary<Type, string>();
            this.mySqlTypes.Add(typeof(int), "INT");
            this.mySqlTypes.Add(typeof(string), "TEXT");
            this.mySqlTypes.Add(typeof(bool), "BOOL");
            this.mySqlTypes.Add(typeof(float), "FLOAT");
        }

        public string? ConvertToMySqlType(Type type)
        {
            if (!this.mySqlTypes.ContainsKey(type))
                return null;
            return this.mySqlTypes[type];
        }

        public int GetPrimaryKey(T model)
        {
            PropertyInfo? primaryKeyProperty = model.GetType().GetProperties()
                .FirstOrDefault(p => p.GetCustomAttributes(false).FirstOrDefault(e => e.GetType() == typeof(SqlKeyIntegerAttribute)) is SqlKeyIntegerAttribute);
            if (primaryKeyProperty is null)
                throw new Exception("Primary key not found");
            return (int)primaryKeyProperty.GetValue(model);
        }

        public string GetPrimaryKeyPropertyName(T model)
        {
            PropertyInfo? primaryKeyProperty = model.GetType().GetProperties()
                .FirstOrDefault(p => p.GetCustomAttributes(false).FirstOrDefault(e => e.GetType() == typeof(SqlKeyIntegerAttribute)) is SqlKeyIntegerAttribute);
            return primaryKeyProperty.Name;
        }

        public string GetCreateTableCommand()
        {
            string command = $"CREATE TABLE IF NOT EXISTS `{table.TableName}` (";
            int iterationNumber = 0;
            foreach (SqlColumn column in table.Columns)
            {
                iterationNumber++;
                string? columnType = ConvertToMySqlType(column.ColumnType);
                if (columnType is null)
                    throw new Exception("This type is not supported");

                command += $"`{column.ColumnName}` {columnType}";
                if (column.IsPrimaryKey && column.ColumnType == typeof(int))
                    command += " PRIMARY KEY AUTO_INCREMENT";

                if (iterationNumber < table.Columns.Count())
                    command += ",";
                
            }
            command += ")";
            return command;
        }

        public string GetRemoveCommand(T model)
        {
            int primaryKey = GetPrimaryKey(model);
            return $"DELETE FROM `{this.table.TableName}` WHERE `{GetPrimaryKeyPropertyName(model)}` = {primaryKey}";
        }

        public string GetUpdateCommand(T model)
        {
            string command = $"UPDATE `{this.table.TableName}` SET ";
            int iterationNumber = 0;
            foreach (SqlColumn column in this.table.Columns)
            {
                iterationNumber++;
                if (column.IsPrimaryKey) continue;
                if (model.GetType().GetProperty(column.ColumnName)?.GetValue(model) is null) continue;

                command += $"`{column.ColumnName}` = @{column.ColumnName}";

                if (iterationNumber < this.table.Columns.Count())
                {
                    command += ",";
                }
            }

            command += $" WHERE `{GetPrimaryKeyPropertyName(model)}` = {GetPrimaryKey(model)}";
            return command;
        }

        public string GetRowsCommand()
        {
            return $"SELECT * FROM `{table.TableName}`";
        }

        public void Dispose()
        {
            mySqlTypes = null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;

namespace Gangs.SqlEngine
{
    public class SqlEngine<T> : IDisposable
    {
        #region Service fields
        private readonly SqlEngineModelParser _sqlEngineModelParser;
        private readonly SqlEngineCommandParser<T> _sqlEngineCommandParser;
        private readonly SqlTable _sqlTable;
        private readonly MySqlConnection _mySqlConnection;
        private readonly object _contextInstance;
        private readonly string _connectionQuery;
        #endregion

        #region Table properties
        public int Length => this.GetLength();
        #endregion

        public SqlEngine(object contextInstance, string connectionQuery)
        {
            this._contextInstance = contextInstance;
            this._connectionQuery = connectionQuery;
            this._sqlEngineModelParser = new SqlEngineModelParser(this);
            this._sqlTable = this._sqlEngineModelParser.ParseModel<T>();
            this._sqlEngineCommandParser = new SqlEngineCommandParser<T>(this._sqlTable);
            this._mySqlConnection = new MySqlConnection(this._connectionQuery);
            this._mySqlConnection.Open();
            this.CreateTable();
        }

        /// <summary>
        /// Create table here
        /// </summary>
        private void CreateTable()
        {
            MySqlCommand query = new MySqlCommand(this._sqlEngineCommandParser.GetCreateTableCommand(), this._mySqlConnection);
            query.ExecuteNonQuery();
        }
        public List<T> ToList()
        {
            List<T> result = new List<T>();
            MySqlCommand query = new MySqlCommand(this._sqlEngineCommandParser.GetRowsCommand(), this._mySqlConnection);
            
            using MySqlDataReader reader = query.ExecuteReader();
            while (reader.Read())
            {
                T instance = Activator.CreateInstance<T>();

                foreach (SqlColumn column in this._sqlTable.Columns)
                {
                    if (column.ColumnType == typeof(string))
                    {
                        instance?.GetType()?.GetProperty(column.ColumnName)?.SetValue(instance, reader.GetString(column.ColumnName));
                    }
                    else if (column.ColumnType == typeof(int))
                    {
                        instance?.GetType()?.GetProperty(column.ColumnName)?.SetValue(instance, reader.GetInt32(column.ColumnName));
                    }
                    else if (column.ColumnType == typeof(bool))
                    {
                        instance?.GetType()?.GetProperty(column.ColumnName)?.SetValue(instance, reader.GetBoolean(column.ColumnName));
                    }
                }
                result.Add(instance);
            }
            return result;
        }
        public T FirstOrDefault(Func<T, bool> predicate)
        {
            return ToList().FirstOrDefault(predicate);
        }
        public T Add(T model)
        {
            string sqlCommand = $"INSERT INTO `{this._sqlTable.TableName}` VALUES(";
            int iterationNumber = 0;
            foreach (SqlColumn column in this._sqlTable.Columns)
            {
                iterationNumber++;
                sqlCommand += $"@{column.ColumnName}";

                if (iterationNumber < this._sqlTable.Columns.Count())
                    sqlCommand += ",";
            }
            sqlCommand += ")";

            MySqlCommand query = new MySqlCommand(sqlCommand, this._mySqlConnection);
            foreach (SqlColumn column in this._sqlTable.Columns)
            {
                query.Parameters.AddWithValue($"@{column.ColumnName}",
                    typeof(T)?.GetProperty(column.ColumnName)?.GetValue(model));
            }

            query.ExecuteNonQuery();
            return model;
        }
        public void Remove(T model)
        {
            MySqlCommand query = new MySqlCommand(this._sqlEngineCommandParser.GetRemoveCommand(model), this._mySqlConnection);
            query.ExecuteNonQuery();
        }
        public void Clear()
        {
            MySqlCommand query = new MySqlCommand($"DELETE FROM `{this._sqlTable.TableName}`", this._mySqlConnection);
            query.ExecuteNonQuery();
        }
        public void Update(T model)
        {
            MySqlCommand query = new MySqlCommand(this._sqlEngineCommandParser.GetUpdateCommand(model), this._mySqlConnection);
            foreach (SqlColumn column in this._sqlTable.Columns)
            {
                if (column.IsPrimaryKey) continue;
                if (model.GetType().GetProperty(column.ColumnName)?.GetValue(model) is null) continue;
                object propertyValue = model.GetType().GetProperty(column.ColumnName)?.GetValue(model);
                query.Parameters.AddWithValue($"@{column.ColumnName}", propertyValue);
            }
            query.ExecuteNonQuery();
        }
        public IEnumerable<T> Where(Func<T, bool> predicate)
        {
            return ToList().Where(predicate);
        }
        public SqlTable GetTable()
        {
            return this._sqlTable;
        }
        private int GetLength()
        {
            return ToList().Count;
        }
        public void Dispose()
        {
            this._mySqlConnection.Close();
            this._sqlEngineModelParser.Dispose();
            this._sqlEngineCommandParser.Dispose();
        }
    }
}

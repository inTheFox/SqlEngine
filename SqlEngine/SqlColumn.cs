using System;
using System.Collections.Generic;
using System.Text;

namespace Gangs.SqlEngine
{
    public class SqlColumn
    {
        public string ColumnName { get; set; }
        public Type ColumnType { get; set; }
        public bool IsPrimaryKey { get; set; }

        public SqlColumn()
        {
            
        }

        public SqlColumn(string columnName, Type columnType, bool isPrimaryKey = false) : this()
        {
            this.ColumnName = columnName;
            this.ColumnType = columnType;
            this.IsPrimaryKey = isPrimaryKey;
        }
    }
}

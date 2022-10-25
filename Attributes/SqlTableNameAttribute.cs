using System;
using System.Collections.Generic;
using System.Text;

namespace Gangs.Attributes
{
    public class SqlTableNameAttribute : Attribute
    {
        public string TableName { get; set; }

        public SqlTableNameAttribute(string tableName)
        {
            TableName = tableName;
        }
    }
}

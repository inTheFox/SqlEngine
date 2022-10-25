using System;
using System.Collections.Generic;
using System.Text;

namespace Gangs.Attributes
{
    public class SqlColumnNameAttribute : Attribute
    {
        public string Name { get; set; }

        public SqlColumnNameAttribute(string columnName)
        {
            Name = columnName;
        }
    }
}

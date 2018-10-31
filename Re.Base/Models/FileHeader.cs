using System;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Models
{
    public class FileHeader
    {

        public long BlocksInFile { get; set; }
    }

    public class DataStructure
    {
        public List<FieldDefinition> Fields { get; set; }
    }

    public class FieldDefinition
    {
        public string FieldName { get; set; }
        public DataType DataType { get; set; }
        public bool Nullable { get; set; }

    }

    public enum DataType
    {
        DateTime,
        BigString,
        LittleString,
        Int16,
        Int32,
        Int64,
        Decimal,
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Re.Base.Data.Models
{
    public class FileHeader
    {

        public long BlocksInFile { get; set; }
    }

    public class DataStructure
    {
        public List<FieldDefinition> Fields { get; set; }
		public List<IndexDefinition> Indexes { get; set; }

        public int GetRecordSize()
        {
            Creation.FieldTypeFactory factory = new Creation.FieldTypeFactory();
            return Fields.Sum(f => factory.GetFieldType(f.DataType).Size);
        }
    }

    public class FieldDefinition
    {
        public string FieldName { get; set; }
        public DataType DataType { get; set; }
        public bool Nullable { get; set; }

    }

	public class IndexDefinition
	{
		public string IndexName { get; set; }
		public int[] FieldIds { get; set; }
	}

    public enum DataType
    {
        Boolean,
        DateTime,
        BigString,
        LittleString,
        Int16,
        Int32,
        Int64,
        Decimal,
    }
}

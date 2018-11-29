using Re.Base.Extensions;
using Re.Base.Interfaces;
using Re.Base.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace Re.Base.DataFieldTypes
{
    public class Int64FieldType : IDataFieldType
    {
        public DataType DataType => DataType.Int64;

        public byte DataTypeToken => Constants.Tokens.Int64TypeToken;

        public int Size => 8;

        public bool IsValid(object value)
        {
            Int64? casted = value as Int64?;
            return casted.HasValue;
        }

        public object ReadFromStream(FileStream stream)
        {
            return stream.ReadInt64();
        }

        public void WriteToStream(FileStream stream, object value)
        {
            stream.WriteInt64((Int64)value);
        }
    }
}

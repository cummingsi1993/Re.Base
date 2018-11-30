using Re.Base.Data.Extensions;
using Re.Base.Data.Interfaces;
using Re.Base.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Re.Base.Data.DataFieldTypes
{
    public class Int16FieldType : IDataFieldType
    {
        public DataType DataType => DataType.Int16;

        public int Size => 2;

        public byte DataTypeToken => Constants.Tokens.Int16TypeToken;

        public bool IsValid(object value)
        {
            Int16? casted = value as Int16?;
            return casted.HasValue;
        }

        public object ReadFromStream(FileStream stream)
        {
            return stream.ReadInt16();
        }

        public void WriteToStream(FileStream stream, object value)
        {
            stream.WriteInt16((Int16)value);
        }
    }
}

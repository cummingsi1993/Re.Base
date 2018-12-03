using Re.Base.Data.Extensions;
using Re.Base.Data.Interfaces;
using Re.Base.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Re.Base.Data.DataFieldTypes
{
    public class Int32FieldType : IDataFieldType
    {
        public DataType DataType => DataType.Int32;

        public byte DataTypeToken => Constants.Tokens.Int32TypeToken;

        public int Size => 4;

        public bool IsValid(object value)
        {
            Int32? casted = value as Int32?;
            return casted.HasValue;
        }

        public object ReadFromStream(Stream stream)
        {
            return stream.ReadInt32();
        }

        public void WriteToStream(Stream stream, object value)
        {
            stream.WriteInt32((Int32)value);
        }
    }
}

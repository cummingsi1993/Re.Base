using Re.Base.Extensions;
using Re.Base.Interfaces;
using Re.Base.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Re.Base.DataFieldTypes
{
    public class BoolFieldType : IDataFieldType
    {
        public DataType DataType => DataType.Boolean;

        public byte DataTypeToken => Constants.Tokens.BooleanTypeToken;

        public int Size => 1;

        public bool IsValid(object value)
        {
            Boolean? casted = value as Boolean?;
            return casted.HasValue;
        }

        public object ReadFromStream(FileStream stream)
        {
            return stream.ReadBoolean();
        }

        public void WriteToStream(FileStream stream, object value)
        {
            stream.WriteBoolean((bool)value);
        }
    }
}

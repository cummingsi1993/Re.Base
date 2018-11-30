using Re.Base.Data.Extensions;
using Re.Base.Data.Interfaces;
using Re.Base.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Re.Base.Data.DataFieldTypes
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

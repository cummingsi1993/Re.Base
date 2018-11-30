using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Re.Base.Data.Models;
using Re.Base.Data.Extensions;

namespace Re.Base.Data.DataFieldTypes
{
    class DateTimeFieldType : Interfaces.IDataFieldType
    {
        public DataType DataType => DataType.DateTime;

        public int Size => Constants.Lengths.DateTimeLength;

        public byte DataTypeToken => Constants.Tokens.DateTimeTypeToken;

        public bool IsValid(object value)
        {
            return (value as DateTime?).HasValue;
        }

        public object ReadFromStream(FileStream stream)
        {
            return stream.ReadDateTime();
        }

        public void WriteToStream(FileStream stream, object value)
        {
            stream.WriteDateTime((DateTime)value);
        }
    }
}

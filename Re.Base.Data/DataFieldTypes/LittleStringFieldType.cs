using Re.Base.Data.Interfaces;
using Re.Base.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Re.Base.Data.Extensions;

namespace Re.Base.Data.DataFieldTypes
{
    class LittleStringFieldType : IDataFieldType
    {
        public DataType DataType => DataType.LittleString;

        public int Size => Constants.Lengths.LittleStringLength;

        public byte DataTypeToken => Constants.Tokens.LittleStringTypeToken;

        public bool IsValid(object value)
        {
            return (value as string) != null;
        }

        public object ReadFromStream(FileStream stream)
        {
            return stream.ReadUTF8String(Constants.Lengths.LittleStringLength);
        }

        public void WriteToStream(FileStream stream, object value)
        {
            stream.WriteUTF8String((string)value, Constants.Lengths.LittleStringLength);
        }
    }
}

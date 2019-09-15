using Re.Base.Data.Interfaces;
using Re.Base.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Re.Base.Data.DataFieldTypes
{
    public class DecimalFieldType : IDataFieldType
    {
        public DataType DataType => DataType.Decimal;

        public int Size => 10;

        public byte DataTypeToken => throw new NotImplementedException();

        public bool IsValid(object value)
        {
            throw new NotImplementedException();
        }

        public object ReadFromStream(Stream stream)
        {
            throw new NotImplementedException();
        }

        public void WriteToStream(Stream stream, object value)
        {
            throw new NotImplementedException();
        }
    }
}

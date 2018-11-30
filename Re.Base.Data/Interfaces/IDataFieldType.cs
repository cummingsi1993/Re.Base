using Re.Base.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Re.Base.Data.Interfaces
{
    public interface IDataFieldType
    {

        DataType DataType { get; }

        int Size { get; }

        byte DataTypeToken { get; }

        bool IsValid(object value);

        void WriteToStream(FileStream stream, object value);

        object ReadFromStream(FileStream stream);
    }
}

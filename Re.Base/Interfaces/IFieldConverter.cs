using Re.Base.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Interfaces
{
    internal interface IFieldConverter<T>
        where T : struct
    {

        bool IsCompatibleWithType(DataType type);

        byte[] ToBytes(T value);
        T ToValue(byte[] bytes);

    }
}

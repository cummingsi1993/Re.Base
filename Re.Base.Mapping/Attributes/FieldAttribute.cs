using Re.Base.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Mapping.Attributes
{

    public class FieldAttribute : Attribute
    {

        
        DataType _dataType;
        Boolean _nullable;
        string _fieldName;

        public FieldAttribute(DataType dataType, bool nullable = false, string fieldName = null)
        {
            _fieldName = fieldName;
            _dataType = dataType;
            _nullable = nullable;
        }

        public string GetFieldName()
        {
            return _fieldName;
        }

        public DataType GetDataType()
        {
            return _dataType;
        }

        public Boolean GetNullable()
        {
            return _nullable;
        }

    }
}

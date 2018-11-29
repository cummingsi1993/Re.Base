using Re.Base.DataFieldTypes;
using Re.Base.Interfaces;
using Re.Base.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Creation
{
    public class FieldTypeFactory
    {
        private List<IDataFieldType> _fieldTypes;
        public FieldTypeFactory()
        {
            _fieldTypes = new List<IDataFieldType>();

            _fieldTypes.Add(new Int16FieldType());
            _fieldTypes.Add(new Int32FieldType());
            _fieldTypes.Add(new Int64FieldType());
            _fieldTypes.Add(new BoolFieldType());

        }

        public IDataFieldType GetFieldType(DataType type)
        {
            return _fieldTypes.Find(f => f.DataType == type);
        }

    }
}

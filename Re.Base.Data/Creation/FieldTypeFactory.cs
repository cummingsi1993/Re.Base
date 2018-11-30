using Re.Base.Data.DataFieldTypes;
using Re.Base.Data.Interfaces;
using Re.Base.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Data.Creation
{
    public class FieldTypeFactory
    {
        private Dictionary<DataType, IDataFieldType> _fieldTypes;
        public FieldTypeFactory()
        {
            _fieldTypes = new Dictionary<DataType, IDataFieldType>();

            var int16Type = new Int16FieldType();
            var int32Type = new Int32FieldType();
            var int64Type = new Int64FieldType();
            var boolType = new BoolFieldType();
            var dateType = new DateTimeFieldType();
            var littleStringType = new LittleStringFieldType();

            _fieldTypes.Add(int16Type.DataType, int16Type);
            _fieldTypes.Add(int32Type.DataType, int32Type);
            _fieldTypes.Add(int64Type.DataType, int64Type);
            _fieldTypes.Add(boolType.DataType, boolType);
            _fieldTypes.Add(dateType.DataType, dateType);
            _fieldTypes.Add(littleStringType.DataType, littleStringType);

        }

        public IDataFieldType GetFieldType(DataType type)
        {
            return _fieldTypes[type];
        }

    }
}

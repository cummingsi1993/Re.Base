using Re.Base.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Re.Base.Data.Logic
{
    public class IndexManager
    {

        private Dictionary<int, Indexing.IIndex<bool>> _booleanIndexes;
        private Dictionary<int, Indexing.IIndex<Int16>> _int16Indexes;
        private Dictionary<int, Indexing.IIndex<Int32>> _int32Indexes;
        private Dictionary<int, Indexing.IIndex<Int64>> _int64Indexes;

        private Dictionary<int, Indexing.IIndex<string>> _stringIndexes;
        private Dictionary<int, Indexing.IIndex<decimal>> _decimalIndexes;

        private Dictionary<int, Indexing.IIndex<DateTime>> _dateTimeIndexes;

        public IndexManager()
        {
            _booleanIndexes = new Dictionary<int, Indexing.IIndex<bool>>();
            _int16Indexes = new Dictionary<int, Indexing.IIndex<short>>();
            _int32Indexes = new Dictionary<int, Indexing.IIndex<int>>();
            _int64Indexes = new Dictionary<int, Indexing.IIndex<long>>();
            _stringIndexes = new Dictionary<int, Indexing.IIndex<string>>();
            _decimalIndexes = new Dictionary<int, Indexing.IIndex<decimal>>();
            _dateTimeIndexes = new Dictionary<int, Indexing.IIndex<DateTime>>();
        }

        private void AddField(object fieldValue, int fieldId, long recordLocation, DataType type)
        {
            if (type == DataType.Boolean)
            {
                _booleanIndexes[fieldId].InsertRecord((Boolean)fieldValue, recordLocation);
            }
            else if (type == DataType.Int16)
            {
                _int16Indexes[fieldId].InsertRecord((Int16)fieldValue, recordLocation);
            }
            else if (type == DataType.Int32)
            {
                _int32Indexes[fieldId].InsertRecord((Int32)fieldValue, recordLocation);
            }
            else if (type == DataType.Int64)
            {
                _int64Indexes[fieldId].InsertRecord((Int64)fieldValue, recordLocation);
            }
            else if (type == DataType.Decimal)
            {
                _decimalIndexes[fieldId].InsertRecord((Decimal)fieldValue, recordLocation);
            }
            else if (type == DataType.BigString || type == DataType.LittleString)
            {
                _stringIndexes[fieldId].InsertRecord((String)fieldValue, recordLocation);
            }
            else if (type == DataType.DateTime)
            {
                _dateTimeIndexes[fieldId].InsertRecord((DateTime)fieldValue, recordLocation);
            }
        }

        public void RegisterIndex(Models.DataType type, IndexDefinition indexDefinition)
        {

            if (type == DataType.Boolean)
            {
                _booleanIndexes.Add(indexDefinition.FieldId, Indexing.Creation.IndexFactory.CreateIndex<bool>(indexDefinition.IndexType));
            }
            else if (type == DataType.Int16)
            {
                _int16Indexes.Add(indexDefinition.FieldId, Indexing.Creation.IndexFactory.CreateIndex<Int16>(indexDefinition.IndexType));
            }
            else if (type == DataType.Int32)
            {
                _int32Indexes.Add(indexDefinition.FieldId, Indexing.Creation.IndexFactory.CreateIndex<Int32>(indexDefinition.IndexType));
            }
            else if (type == DataType.Int64)
            {
                _int64Indexes.Add(indexDefinition.FieldId, Indexing.Creation.IndexFactory.CreateIndex<Int64>(indexDefinition.IndexType));
            }
            else if (type == DataType.Decimal)
            {
                _decimalIndexes.Add(indexDefinition.FieldId, Indexing.Creation.IndexFactory.CreateIndex<decimal>(indexDefinition.IndexType));
            }
            else if (type == DataType.BigString || type == DataType.LittleString)
            {
                _stringIndexes.Add(indexDefinition.FieldId, Indexing.Creation.IndexFactory.CreateIndex<string>(indexDefinition.IndexType));
            }
            else if (type == DataType.DateTime)
            {
                _dateTimeIndexes.Add(indexDefinition.FieldId, Indexing.Creation.IndexFactory.CreateIndex<DateTime>(indexDefinition.IndexType));
            }



        }

        public bool FieldIsIndexed(int fieldId)
        {
            return _booleanIndexes.ContainsKey(fieldId)
                || _int16Indexes.ContainsKey(fieldId)
                || _int32Indexes.ContainsKey(fieldId)
                || _int64Indexes.ContainsKey(fieldId)
                || _stringIndexes.ContainsKey(fieldId)
                || _decimalIndexes.ContainsKey(fieldId)
                || _dateTimeIndexes.ContainsKey(fieldId);
        }

        public bool AnyFieldIsIndexed()
        {
            return _booleanIndexes.Count > 0
                || _int16Indexes.Count > 0
                || _int32Indexes.Count > 0
                || _int64Indexes.Count > 0
                || _stringIndexes.Count > 0
                || _decimalIndexes.Count > 0
                || _dateTimeIndexes.Count > 0;
        }

        public long Count()
        {
            if (_booleanIndexes.Count > 0)
            {
                Indexing.IIndex<bool> index = _booleanIndexes[_booleanIndexes.Keys.First()];
                return index.CountRecords();

            }
            else if (_int16Indexes.Count > 0)
            {
                Indexing.IIndex<Int16> index = _int16Indexes[_int16Indexes.Keys.First()];
                return index.CountRecords();
            }
            else if (_int32Indexes.Count > 0)
            {
                Indexing.IIndex<Int32> index = _int32Indexes[_int32Indexes.Keys.First()];
                return index.CountRecords();
            }
            else if (_int64Indexes.Count > 0)
            {
                Indexing.IIndex<Int64> index = _int64Indexes[_int64Indexes.Keys.First()];
                return index.CountRecords();
            }
            else if (_stringIndexes.Count > 0)
            {
                Indexing.IIndex<string> index = _stringIndexes[_stringIndexes.Keys.First()];
                return index.CountRecords();
            }
            else if (_decimalIndexes.Count > 0)
            {
                Indexing.IIndex<decimal> index = _decimalIndexes[_decimalIndexes.Keys.First()];
                return index.CountRecords();
            }
            else if (_dateTimeIndexes.Count > 0)
            {
                Indexing.IIndex<DateTime> index = _dateTimeIndexes[_dateTimeIndexes.Keys.First()];
                return index.CountRecords();
            }
            else
            {
                throw new Exception();
            }
        }

        public long GetLocationFromIndex(int fieldId, object fieldValue, DataStructure currentSchema)
        {
            var type = currentSchema.Fields[fieldId].DataType;

            if (type == DataType.Boolean)
            {
                return _booleanIndexes[fieldId].GetRecordLocation((Boolean)fieldValue);
            }
            else if (type == DataType.Int16)
            {
                return _int16Indexes[fieldId].GetRecordLocation((Int16)fieldValue);
            }
            else if (type == DataType.Int32)
            {
                return _int32Indexes[fieldId].GetRecordLocation((Int32)fieldValue);
            }
            else if (type == DataType.Int64)
            {
                return _int64Indexes[fieldId].GetRecordLocation((Int64)fieldValue);
            }
            else if (type == DataType.Decimal)
            {
                return _decimalIndexes[fieldId].GetRecordLocation((Decimal)fieldValue);
            }
            else if (type == DataType.BigString || type == DataType.LittleString)
            {
                return _stringIndexes[fieldId].GetRecordLocation((String)fieldValue);
            }
            else if (type == DataType.DateTime)
            {
                return _dateTimeIndexes[fieldId].GetRecordLocation((DateTime)fieldValue);
            }
            else
            {
                throw new Exception();
            }


        }

        public void AddRecord(object[] fields, long recordLocation, DataStructure currentSchema)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                if (FieldIsIndexed(i))
                {
                    object fieldValue = fields[i];
                    this.AddField(fieldValue, i, recordLocation, currentSchema.Fields[i].DataType);
                }
            }
        }

    }
}

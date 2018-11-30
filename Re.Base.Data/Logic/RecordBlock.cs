using Re.Base.Data.Extensions;
using Re.Base.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Re.Base.Data.Constants;
using Re.Base.Data.Interfaces;

namespace Re.Base.Data.Logic
{
    public class RecordBlock
    {
        private FileStream _stream;
        private DataStructure _schema;
        private Creation.FieldTypeFactory _fieldTypeFactory;

        private RecordBlock(FileStream stream, DataStructure schema, long index)
        {
            _stream = stream;
            _schema = schema;

            this.Index = index;

            _stream.SeekToBlockHeader(index);
            this.BlockHeader = _stream.ReadBlockHeader();
            _fieldTypeFactory = new Creation.FieldTypeFactory();
        }

        public static RecordBlock LoadFromStream(FileStream stream, DataStructure schema, long index)
        {
            var block = new RecordBlock(stream, schema, index);
            
            return block;
        }

        public static RecordBlock CreateNew(FileStream stream, DataStructure schema, long index)
        {
            stream.SeekToBlockHeader(index);
            stream.WriteBlockHeader(new BlockHeader() { BlockFragmented = false, BlockSequence = index, FreeBytes = Constants.Lengths.BlockLength, RecordCount = 0 });
            stream.Write(new byte[Lengths.BlockLength], 0, Lengths.BlockLength);

            var block = new RecordBlock(stream, schema, index);

            return block;
        }

        public BlockHeader BlockHeader { get; private set; }

        public Record ReadRecord(long index)
        {
            _stream.SeekToRecord(_schema, this.Index, index);
            
            return this.ReadNextRecord();
        }

        public Record[] ReadAllRecords()
        {

            _stream.SeekToBlockContents(this.BlockHeader.BlockSequence);
            Record[] records = new Record[this.BlockHeader.RecordCount];

            
            for (int i = 0; i < this.BlockHeader.RecordCount; i++)
            {

                Record record = new Record() { Fields = new RecordField[_schema.Fields.Count] };

                for (int f = 0; f < _schema.Fields.Count; f++)
                {
                    var fieldDefinition = _schema.Fields[f];

                    var fieldType = _fieldTypeFactory.GetFieldType(fieldDefinition.DataType);
                    object fieldValue = fieldType.ReadFromStream(_stream);

                    record.Fields[f] = new RecordField() { Value = fieldValue, DataType = fieldDefinition.DataType };
                }

                records[i] = record;
            }

            return records;
        }

        public Record[] Query(Func<Record, bool> func)
        {
            List<Record> records = new List<Record>();
            _stream.SeekToBlockContents(this.Index);
            for (int i = 0; i < this.BlockHeader.RecordCount; i++)
            {
                Record record = ReadNextRecord();
                if (func(record))
                {
                    records.Add(record);
                }
            }
            return records.ToArray();
        }

        public long Index { get; private set; }

        public void InsertRecord(params object[] fields)
        {
            _stream.SeekToRecord(_schema, BlockHeader.BlockSequence, BlockHeader.RecordCount);

            IDataFieldType[] fieldTypes = new IDataFieldType[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                var fieldDefinition = _schema.Fields[i];

                fieldTypes[i] = _fieldTypeFactory.GetFieldType(fieldDefinition.DataType);
            }

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (!fieldTypes[i].IsValid(field))
                {
                    //Maybe i should collect all the failures and return them at once.
                    throw new Exception();
                }
            }


            //Only if there are no failures, we start writing to the file
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                fieldTypes[i].WriteToStream(_stream, field);
            }


            BlockHeader.FreeBytes -= _schema.GetRecordSize();
            BlockHeader.RecordCount++;
            _stream.Flush();
            this.WriteBlockHeader();
        }

        public void DeleteRecord()
        {

        }

        public void UpdateRecord()
        {

        }

        private Record ReadNextRecord()
        {
            Record record = new Record() { Fields = new RecordField[_schema.Fields.Count] };

            for (int f = 0; f < _schema.Fields.Count; f++)
            {
                var fieldDefinition = _schema.Fields[f];

                var fieldType = _fieldTypeFactory.GetFieldType(fieldDefinition.DataType);
                object fieldValue = fieldType.ReadFromStream(_stream);

                record.Fields[f] = new RecordField() { Value = fieldValue, DataType = fieldDefinition.DataType };
            }

            return record;

        }

        private void WriteBlockHeader()
        {
            _stream.SeekToBlockHeader(this.Index);
            _stream.WriteBlockHeader(this.BlockHeader);
            _stream.Flush();
        }



    }
}

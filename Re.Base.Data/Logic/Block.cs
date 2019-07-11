using Re.Base.Data.Extensions;
using Re.Base.Data.Interfaces;
using Re.Base.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Re.Base.Data.Logic
{
    public abstract class Block<TModel>
    {

        private FileStream _stream;
        private DataStructure _schema;
        private Creation.FieldTypeFactory _fieldTypeFactory;

        private int _valueByteSize;

        public Block(FileStream stream, DataStructure schema, long index)
        {
            _stream = stream;
            _schema = schema;

            this.Index = index;

            _stream.SeekToBlockHeader(index);
            this.BlockHeader = _stream.ReadBlockHeader();
            _fieldTypeFactory = new Creation.FieldTypeFactory();
            _valueByteSize = _schema.GetRecordSize();
        }

        private void WriteBlockHeader()
        {
            _stream.SeekToBlockHeader(this.Index);
            _stream.WriteBlockHeader(this.BlockHeader);
            _stream.Flush();
        }

        public long Index { get; private set; }
        public BlockHeader BlockHeader { get; private set; }

        #region Read Data
        public byte[] ReadNextBytes()
        {
            byte[] valueBytes = new byte[_valueByteSize];
            _stream.Read(valueBytes, 0, _valueByteSize);
            return valueBytes;
        }

        public TModel ReadNext()
        {
            long currentPosition = _stream.Position;
            byte[] valueBytes = ReadNextBytes();
            return this.GetValue(_schema, _fieldTypeFactory, currentPosition, valueBytes);
        }

        public TModel Read(long index)
        {
            _stream.SeekToRecord(_schema, this.Index, index);
            return ReadNext();
        }

        public TModel[] ReadAll()
        {
            TModel[] models = new TModel[BlockHeader.RecordCount];
            _stream.SeekToBlockContents(this.Index);
            for (int i = 0; i < this.BlockHeader.RecordCount; i++)
            {
                TModel model = this.ReadNext();
                models[i] = model;
            }

            return models;
        }

        public TModel[] Query(Func<TModel, bool> func)
        {
            List<TModel> models = new List<TModel>();
            _stream.SeekToBlockContents(Index);
            for (int i = 0; i < BlockHeader.RecordCount; i++)
            {
                TModel model = ReadNext();
                if (func(model))
                {
                    models.Add(model);
                }
            }

            return models.ToArray();
        }

        public TModel[] Query(Func<byte[], bool> func)
        {
            List<TModel> models = new List<TModel>();
            _stream.SeekToBlockContents(Index);
            for (int i = 0; i < BlockHeader.RecordCount; i++)
            {
                long currentPosition = _stream.Position;
                byte[] modelBytes = ReadNextBytes();
                if (func(modelBytes))
                {
                    models.Add(GetValue(_schema, _fieldTypeFactory, currentPosition, modelBytes));
                }
            }

            return models.ToArray();
        }

        #endregion

        #region Write Data

        /// <summary>
        /// Inserts the record returns the location at which it was inserted. 
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public long Insert(params object[] fields)
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

            long recordLocation = _stream.Position;

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

            return recordLocation;
        }

        #endregion

        protected abstract TModel GetValue(DataStructure schema, Creation.FieldTypeFactory fieldTypeFactory, long recordLocation, byte[] bytes);
        


    }
}

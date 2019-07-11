using Re.Base.Data.Extensions;
using Re.Base.Data.Interfaces;
using Re.Base.Data.Logic;
using Re.Base.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Re.Base.Data
{
    public class DataStore : IDataStore
    {
        private FileStream _stream;
        private FileStream _schemaStream;
        private IndexManager _indexManager;

        private FileHeader _fileHeader;
        private DataStructure _schema;
        private Creation.FieldTypeFactory _fieldTypeFactory;

        public DataStore(string fileLocation, string recordName)
        {
            _fieldTypeFactory = new Creation.FieldTypeFactory();
            _indexManager = new IndexManager();

            string fileName = $"{fileLocation}/data_{recordName}.rbs";
            string schemaFileName = $"{fileLocation}/schema_{recordName}.rbs";
            if (File.Exists(fileName))
            {
                _stream = File.Open(fileName, FileMode.Open);
                _fileHeader = this.ReadFileHeader();
            }
            else
            {
                _stream = File.Create(fileName);
                _fileHeader = new FileHeader()
                {
                    BlocksInFile = 0
                };


            }

            if (File.Exists(schemaFileName))
            {
                _schemaStream = File.Open(schemaFileName, FileMode.Open);
                this.ReadSchema();
            }
            else
            {
                _schemaStream = File.Create(schemaFileName);
                _schema = new DataStructure()
                {
                    Fields = new List<FieldDefinition>(),
                    Indexes = new List<IndexDefinition>()
                };
            }

            foreach(IndexDefinition indexDefinition in _schema.Indexes)
            {
                _indexManager.RegisterIndex(_schema.Fields[indexDefinition.FieldId].DataType, indexDefinition);
            }

            //If there are any in-memory indexes, we will have to build the indexes from scratch by reading in the entire table.
            if (_schema.Indexes.Exists(i => i.IndexType == Indexing.Enums.IndexType.InMemoryIndex))
            {
                foreach (Record record in this.ReadAllRecords())
                {
                    _indexManager.AddRecord(record.Fields.Select(f => f.Value).ToArray(), record.Location, _schema);
                }
            }

        }

        private void WriteFileHeader()
        {
            //Set position to the beginning of the stream.
            _stream.Position = 0;

            byte[] bytes = new byte[Constants.Lengths.FileHeaderLength];
            //This is a token byte to assure the processor this is a correct file.
            bytes[0] = Constants.Tokens.FileBeginToken;
            BitConverter.GetBytes((long)_fileHeader.BlocksInFile).CopyTo(bytes, 1);

            int bytePointer = 9;
            
            //I havent decided what else needs to be in the header yet...so we will fill it up with blank space.
            FillArray(bytes, Constants.Lengths.FileHeaderLength, bytePointer);
            _stream.Write(bytes, 0, Constants.Lengths.FileHeaderLength);
        }

        private FileHeader ReadFileHeader()
        {
            if (_fileHeader != null)
            {
                return _fileHeader;
            }

            _stream.Position = 0;
            
            _fileHeader = _stream.ReadFileHeader();
            return _fileHeader;
        }

        #region Schema 

        private void ReadSchema()
        {
            _schemaStream.Position = 0;
            if (_schemaStream.ReadByte() != Constants.Tokens.FileBeginToken)
            {
                throw new InvalidOperationException();
            }

            _schema = new DataStructure()
            {
                Fields = new List<FieldDefinition>(),
                Indexes = new List<IndexDefinition>()
            };

            bool EOF = false;
            bool schemaSegment = false;

            while (!EOF && !schemaSegment)
            {
                _schema.Fields.Add(_schemaStream.ReadFieldDefinition());
                byte potentialSegmentChange = _schemaStream.PeekByte();

                if (potentialSegmentChange == Constants.Tokens.SchemaSegmentSeperationToken) schemaSegment = true;
                if (_schemaStream.Length <= _schemaStream.Position) EOF = true;
            }

            if (schemaSegment) _schemaStream.ReadByte();

            while (!EOF)
            {
                _schema.Indexes.Add(_schemaStream.ReadIndexDefinition());
                if (_schemaStream.Length <= _schemaStream.Position) EOF = true;
            }
        }

        public void WriteSchema()
        {
            _schemaStream.Position = 0;
            _schemaStream.WriteByte(Constants.Tokens.FileBeginToken);

            foreach(FieldDefinition field in _schema.Fields)
            {
                _schemaStream.WriteFieldDefinition(field);
            }

            _schemaStream.WriteByte(Constants.Tokens.SchemaSegmentSeperationToken);

            foreach(IndexDefinition index in _schema.Indexes)
            {
                _schemaStream.WriteIndexDefinition(index);
            }

            //Truncate the stream
            if (_schemaStream.Position < _schemaStream.Length)
            {
                _schemaStream.SetLength(_schemaStream.Position + 1);
            }

            _schemaStream.Flush();
        }

        public void AddField(FieldDefinition field)
        {
            _schema.Fields.Add(field);
            WriteSchema();
        }

        public void RemoveField(string fieldName)
        {
            _schema.Fields.RemoveAll(field => field.FieldName == fieldName);
            WriteSchema();
        }

        public void ModifyField(string fieldName, FieldDefinition newFieldDefinition)
        {
            foreach(FieldDefinition field in _schema.Fields)
            {
                if (field.FieldName == fieldName)
                {
                    field.DataType = newFieldDefinition.DataType;
                    field.FieldName = newFieldDefinition.FieldName;
                    field.Nullable = newFieldDefinition.Nullable;
                }
            }
        }

        public void AddIndex(int fieldId, string name, Indexing.Enums.IndexType type)
        {
            var definition = new IndexDefinition() { FieldId = fieldId, IndexName = name, IndexType = type };
            _indexManager.RegisterIndex(_schema.Fields[fieldId].DataType, definition);
            _schema.Indexes.Add(definition);
            this.WriteSchema();
        }

        public DataStructure GetSchema()
        {
            return _schema;
        }

        #endregion

        public void InsertRecord(params object[] fields)
        {
            long insertLocation;

            //Find available spot to place record....
            if (_fileHeader.BlocksInFile == 0)
            {

                var block = RecordBlock.CreateNew(_stream, _schema, 0);

                _fileHeader.BlocksInFile++;
                WriteFileHeader();

                insertLocation = block.Insert(fields);


            }
            else
            {
                var lastBlock = RecordBlock.LoadFromStream(_stream, _schema, _fileHeader.BlocksInFile - 1);
                if (lastBlock.BlockHeader.FreeBytes > _schema.GetRecordSize())
                {
                    insertLocation = lastBlock.Insert(fields);
                }
                else
                {
                    var newBlock = RecordBlock.CreateNew(_stream, _schema, _fileHeader.BlocksInFile);
                    insertLocation = newBlock.Insert(fields);

                    _fileHeader.BlocksInFile++;
                    WriteFileHeader();
                }
            }

            _indexManager.AddRecord(fields, insertLocation, _schema);
        }

        public Record[] ReadAllRecords()
        {
            List<Record> records = new List<Record>();
            for (int i = 0; i < this._fileHeader.BlocksInFile; i++)
            {
                var block = RecordBlock.LoadFromStream(_stream, _schema, i);
                records.AddRange(block.ReadAll());
            }

            return records.ToArray();
        }

        public Record ReadRecordFromPointer(long pointer)
        {
            int totalBlockLength = Constants.Lengths.BlockLength + Constants.Lengths.BlockHeaderLength;
            long pointerInBody = pointer - Constants.Lengths.FileHeaderLength;

            int blockNumber = (int)Math.Floor((decimal)pointerInBody / totalBlockLength);

            long remainingBytes = pointerInBody - (blockNumber * totalBlockLength);
            long trueIndex = (int)(remainingBytes / _schema.GetRecordSize());
            
            var block = RecordBlock.LoadFromStream(_stream, _schema, blockNumber - 1);
            return block.Read(trueIndex);
        }

        public Record ReadRecord(long index)
        {
            long totalIndex = 0;

            for (int i = 0; i < this._fileHeader.BlocksInFile; i++)
            {
                var block = RecordBlock.LoadFromStream(_stream, _schema, i);
                //the index is less than the upper bound of this block
                if (index < totalIndex + block.BlockHeader.RecordCount)
                {
                    return block.Read(index - totalIndex);
                }

                totalIndex += block.BlockHeader.RecordCount;
            }

            throw new Exception();
        }

        public Record[] Query(Func<Record, bool> query)
        {
            List<Record> records = new List<Record>();
            for (int i = 0; i < this._fileHeader.BlocksInFile; i++)
            {
                var block = RecordBlock.LoadFromStream(_stream, _schema, i);
                records.AddRange(block.Query(query));
            }

            return records.ToArray();
        }

        public Record[] QueryBy(int fieldId, object fieldValue)
        {
            if (_indexManager.FieldIsIndexed(fieldId))
            {
                long pointer = _indexManager.GetLocationFromIndex(fieldId, fieldValue, _schema);
                return new Record[] { this.ReadRecordFromPointer(pointer) };
            }
            else
            {
                return this.Query(r => r.Fields[fieldId].Value == fieldValue);
            }
        }

        #region Helper Functions





        private void FillArray(byte[] bytes, long count, long startingIndex = 0)
        {
            for (long i = startingIndex; i < count; i++)
            {
                bytes[i] = 0;
            }
        }

        #endregion

    }
   
}

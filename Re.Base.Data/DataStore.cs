using Re.Base.Data.Extensions;
using Re.Base.Data.Interfaces;
using Re.Base.Data.Logic;
using Re.Base.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Re.Base.Data
{
    public class DataStore : IDataStore
    {
        private FileStream _stream;
        private FileStream _schemaStream;
        private FileHeader _fileHeader;
        private DataStructure _schema;
        private Creation.FieldTypeFactory _fieldTypeFactory;

        public DataStore(string fileLocation, string recordName)
        {
            _fieldTypeFactory = new Creation.FieldTypeFactory();

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
                    Fields = new List<FieldDefinition>()
                };
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
                Fields = new List<FieldDefinition>()
            };

            bool EOF = false;

            while (!EOF)
            {
                _schema.Fields.Add(_schemaStream.ReadFieldDefinition());
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

        public DataStructure GetSchema()
        {
            return _schema;
        }

        #endregion

        public void InsertRecord(params object[] fields)
        {
            //Find available spot to place record....
            if (_fileHeader.BlocksInFile == 0)
            {

                var block = RecordBlock.CreateNew(_stream, _schema, 0);

                _fileHeader.BlocksInFile++;
                WriteFileHeader();

                block.Insert(fields);

            }
            else
            {
                var lastBlock = RecordBlock.LoadFromStream(_stream, _schema, _fileHeader.BlocksInFile - 1);
                if (lastBlock.BlockHeader.FreeBytes > _schema.GetRecordSize())
                {
                    lastBlock.Insert(fields);
                }
                else
                {
                    var newBlock = RecordBlock.CreateNew(_stream, _schema, _fileHeader.BlocksInFile);
                    newBlock.Insert(fields);

                    _fileHeader.BlocksInFile++;
                    WriteFileHeader();
                }
            }
            

            
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

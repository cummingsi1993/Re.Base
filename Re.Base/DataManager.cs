using Re.Base.Extensions;
using Re.Base.Interfaces;
using Re.Base.Logic;
using Re.Base.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Re.Base
{
    public class DataManager
    {
        private const byte FileBeginToken = 0x000A;
        private const byte BlockBeginToken = 0x000B;
        private const byte RecordBeginToken = 0x000C;



        private const int FileHeaderLength = 1024;
        private const int BlockHeaderLength = 128;
        private const int RecordHeaderLength = 128;
        private const int BlockLength = 8000;

        private const int LittleStringLength = 100;
        private const int BigStringLength = 1000;

        private FileStream _stream;
        private FileStream _schemaStream;
        private FileHeader _fileHeader;
        private DataStructure _schema;
        private Creation.FieldTypeFactory _fieldTypeFactory;

        public DataManager(string fileLocation, string recordName)
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

        /// <summary>
        /// The file header is 1024 bytes.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="header"></param>
        public void WriteFileHeader()
        {
            //Set position to the beginning of the stream.
            _stream.Position = 0;

            byte[] bytes = new byte[FileHeaderLength];
            //This is a token byte to assure the processor this is a correct file.
            bytes[0] = FileBeginToken;
            BitConverter.GetBytes((long)_fileHeader.BlocksInFile).CopyTo(bytes, 1);

            int bytePointer = 9;
            
            //I havent decided what else needs to be in the header yet...so we will fill it up with blank space.
            FillArray(bytes, FileHeaderLength, bytePointer);
            _stream.Write(bytes, 0, FileHeaderLength);
        }

        public FileHeader ReadFileHeader()
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
        public void ReadSchema()
        {
            _schemaStream.Position = 0;
            if (_schemaStream.ReadByte() != FileBeginToken)
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
            _schemaStream.WriteByte(FileBeginToken);

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

        #endregion


        public void InsertRecord(params object[] fields)
        {
            //Find available spot to place record....
            if (_fileHeader.BlocksInFile == 0)
            {

                var block = RecordBlock.CreateNew(_stream, _schema, 0);

                _fileHeader.BlocksInFile++;
                WriteFileHeader();

                block.InsertRecord(fields);

            }
            else
            {
                var lastBlock = RecordBlock.LoadFromStream(_stream, _schema, _fileHeader.BlocksInFile - 1);
                if (lastBlock.BlockHeader.FreeBytes > _schema.GetRecordSize())
                {
                    lastBlock.InsertRecord(fields);
                }
                else
                {
                    var newBlock = RecordBlock.CreateNew(_stream, _schema, _fileHeader.BlocksInFile);
                    newBlock.InsertRecord(fields);

                    _fileHeader.BlocksInFile++;
                    WriteFileHeader();
                }
            }
            

            
        }

        public BlockHeader ReadBlockHeader(long blockSequence)
        {
            JumpToBlockHeader(blockSequence);

            return _stream.ReadBlockHeader();
        }

        public Record[] ReadAllRecords()
        {
            List<Record> records = new List<Record>();
            for (int i = 0; i < this._fileHeader.BlocksInFile; i++)
            {
                var block = RecordBlock.LoadFromStream(_stream, _schema, i);
                records.AddRange(block.ReadAllRecords());
            }

            return records.ToArray();
        }

        public byte[] ReadFullRecord(long blockSequence, RecordHeader[] allHeadersInblock, int headToRead)
        {
            var recordToRead = allHeadersInblock[headToRead];
            JumpToRecord(blockSequence, allHeadersInblock, headToRead);

            byte[] record = new byte[recordToRead.BytesInRecord];

            _stream.Read(record, 0, recordToRead.BytesInRecord);
            return record;
        }

        #region Helper Functions

        private void JumpToBlock(long blockSequence)
        {
            _stream.Position = ((BlockLength + BlockHeaderLength) * blockSequence) + FileHeaderLength + BlockHeaderLength;
        }

        private void JumpToBlockHeader(long blockSequence)
        {
            _stream.Position = ((BlockLength + BlockHeaderLength) * blockSequence) + FileHeaderLength;
        }

        private void JumpToRecord(long blockSequence, RecordHeader[] headersInBlock, int headerSequence)
        {
            long position = ((BlockLength + BlockHeaderLength) * blockSequence) + FileHeaderLength + BlockHeaderLength;
            for (int i = 0; i < headerSequence; i++)
            {
                var header = headersInBlock[i];
                position += RecordHeaderLength;
                position += header.BytesInRecord;
            }

            position += RecordHeaderLength;

            _stream.Position = position;
        }

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

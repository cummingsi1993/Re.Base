using Re.Base.Extensions;
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

        private const byte DateTimeTypeToken = 0x001A;
        private const byte Int64TypeToken = 0x001B;
        private const byte Int32TypeToken = 0x001C;
        private const byte Int16TypeToken = 0x001D;
        private const byte DecimalTypeToken = 0x001E;
        private const byte BigStringTypeToken = 0x001F;
        private const byte LittleStringTypeToken = 0x002A;

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

        public DataManager(string fileLocation, string recordName)
        {
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

            }

            List<RecordField> recordFields = new List<RecordField>();

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var fieldDefinition = _schema.Fields[i];

                if (fieldDefinition.DataType == DataType.Int32)
                {
                    Int32? value = field as Int32?;
                    if (!value.HasValue)
                    {
                        throw new Exception();
                    }
                    recordFields.Add(new RecordField() { DataType = fieldDefinition.DataType, Value = value.Value });
                }
                else if (fieldDefinition.DataType == DataType.Int16)
                {
                    Int16? value = field as Int16?;
                    if (!value.HasValue)
                    {
                        throw new Exception();
                    }
                    recordFields.Add(new RecordField() { DataType = fieldDefinition.DataType, Value = value.Value });
                }


                
            }
            
        }

        public byte[] ReadBlock(long blockSequence)
        {
            JumpToBlock(blockSequence);

            byte[] bytes = new byte[BlockLength];

            _stream.Read(bytes, 0, BlockLength);

            return bytes;
        }

        public void WriteBlockHeader(BlockHeader header)
        {
            JumpToBlockHeader(header.BlockSequence);

            byte[] bytes = new byte[BlockHeaderLength];
            bytes[0] = BlockBeginToken;
            BitConverter.GetBytes(header.BlockSequence).CopyTo(bytes, 1);
            BitConverter.GetBytes(header.FreeBytes).CopyTo(bytes, 9);
            _stream.Write(bytes, 0, BlockHeaderLength);
        }

        public void WriteNewBlock()
        {
            _fileHeader.BlocksInFile = _fileHeader.BlocksInFile + 1;
            WriteFileHeader();

            //Jump to the end of the stream;
            BlockHeader header = new BlockHeader() { BlockFragmented = false, BlockSequence = _fileHeader.BlocksInFile, FreeBytes = BlockLength };
            WriteBlockHeader(header);

            _stream.Write(new byte[BlockLength], 0, BlockLength);
        }

        public BlockHeader ReadBlockHeader(long blockSequence)
        {
            JumpToBlockHeader(blockSequence);

            return _stream.ReadBlockHeader();
        }

        public RecordHeader[] ReadRecordsInBlock(long blockSequence, int recordCount)
        {
            JumpToBlock(blockSequence);
            RecordHeader[] headers = new RecordHeader[recordCount];

            for (int h = 0; h < recordCount; h++)
            {
                byte[] recordHeaderBytes = new byte[RecordHeaderLength];
                _stream.Read(recordHeaderBytes, 0, RecordHeaderLength);

                if (recordHeaderBytes[0] != RecordBeginToken)
                {
                    throw new InvalidOperationException();
                }

                RecordHeader header = new RecordHeader();
                header.BytesInRecord = BitConverter.ToInt32(recordHeaderBytes, 1);
                byte[] idBytes = new byte[16];

                for (int i = 0; i < 15; i++)
                {
                    idBytes[i] = recordHeaderBytes[i + 5];
                }

                header.Id = new Guid(idBytes);

                headers[h] = header;
            }

            return headers;
        }

        public byte[] ReadFullRecord(long blockSequence, RecordHeader[] allHeadersInblock, int headToRead)
        {
            var recordToRead = allHeadersInblock[headToRead];
            JumpToRecord(blockSequence, allHeadersInblock, headToRead);

            byte[] record = new byte[recordToRead.BytesInRecord];

            _stream.Read(record, 0, recordToRead.BytesInRecord);
            return record;
        }

        public void WriteRecordToBlock(long blockSequence, byte[] bytes)
        {

            BlockHeader header = ReadBlockHeader(blockSequence);
            header.FreeBytes -= (bytes.Length + RecordHeaderLength);

            WriteBlockHeader(header);

            long firstAvailableByte = BlockLength - header.FreeBytes;
            _stream.Position += firstAvailableByte;

            byte[] headerBytes = new byte[RecordHeaderLength];
            headerBytes[0] = RecordBeginToken;
            BitConverter.GetBytes((long)bytes.Length).CopyTo(headerBytes, 1);

            Guid recordId = Guid.NewGuid();
            recordId.ToByteArray().CopyTo(headerBytes, 9);
            _stream.Write(headerBytes, 0, RecordHeaderLength);

            _stream.Write(bytes, 0, bytes.Length);
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

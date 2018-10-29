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
        private FileHeader _fileHeader;
        

        public DataManager(string fileLocation)
        {
            if (File.Exists(fileLocation))
            {
                _stream = File.Open(fileLocation, FileMode.Open);
                _fileHeader = this.ReadFileHeader();
            }
            else
            {
                _stream = File.Create(fileLocation);
                _fileHeader = new FileHeader()
                {
                    BlocksInFile = 0,
                    DataStructure = new DataStructure()
                    {
                        Fields = new List<FieldDefinition>()
                    }
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
            foreach(FieldDefinition field in _fileHeader.DataStructure.Fields)
            {
                byte[] fieldNameBytes = Encoding.ASCII.GetBytes(field.FieldName);
                fieldNameBytes.CopyTo(bytes, bytePointer);
                bytePointer += fieldNameBytes.Length;

                byte dataTypeByte;
                switch (field.DataType)
                {
                    case DataType.Int32:
                        dataTypeByte = Int32TypeToken;
                        break;
                    case DataType.Int64:
                        dataTypeByte = Int64TypeToken;
                        break;
                    case DataType.Int16:
                        dataTypeByte = Int16TypeToken;
                        break;
                    case DataType.DateTime:
                        dataTypeByte = DateTimeTypeToken;
                        break;
                    case DataType.Decimal:
                        dataTypeByte = DecimalTypeToken;
                        break;
                    case DataType.LittleString:
                        dataTypeByte = LittleStringTypeToken;
                        break;
                    case DataType.BigString:
                        dataTypeByte = BigStringTypeToken;
                        break;
                    default:
                        throw new NotSupportedException("");
                }

                bytes[bytePointer] = dataTypeByte;
                bytePointer++;

                //TODO: nullable

            }

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

            FileHeader header = new FileHeader();
            _stream.Position = 0;
            byte[] headerBytes = new byte[FileHeaderLength];

            _stream.Read(headerBytes, 0, FileHeaderLength);

            //The file header is corrupt
            if (headerBytes[0] != FileBeginToken)
            {
                throw new InvalidOperationException();
            }

            long blockCount = BitConverter.ToInt64(headerBytes, 1);
            header.BlocksInFile = blockCount;



            _fileHeader = header;
            return header;
        }
        public void AddField(DataType type, string name, bool nullable)
        {
            if (_fileHeader.BlocksInFile > 0)
            {
                throw new NotSupportedException("Altering the data structure after records have been inserted is not supported");
            }

            _fileHeader.DataStructure.Fields.Add(new FieldDefinition() { DataType = type, FieldName = name, Nullable = nullable });

            WriteFileHeader();
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

            byte[] bytes = new byte[BlockHeaderLength];

            _stream.Read(bytes, 0, BlockHeaderLength);

            if (bytes[0] != BlockBeginToken)
            {
                throw new InvalidOperationException();
            }

            BlockHeader header = new BlockHeader();

            header.BlockSequence = BitConverter.ToInt64(bytes, 1);
            header.FreeBytes = BitConverter.ToInt64(bytes, 9);

            return header;
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

    public class SchemaManager
    {
        private FileStream _stream;
        
        public SchemaManager(string fileLocation)
        {
            if (File.Exists(fileLocation))
            {
                _stream = File.Open(fileLocation, FileMode.Open);
            }
            else
            {
                _stream = File.Create(fileLocation);
            }


        }
    }
}

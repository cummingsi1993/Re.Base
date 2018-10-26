using Re.Base.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Re.Base
{
    public class DataManager
    {
        private const byte FileBeginToken = 0x00A;
        private const byte BlockBeginToken = 0x00B;
        private const byte RecordBeginToken = 0x00C;

        private const int FileHeaderLength = 1024;
        private const int BlockHeaderLength = 128;
        private const int RecordHeaderLength = 128;
        private const int BlockLength = 8000;

        private FileHeader fileHeader;

        /// <summary>
        /// The file header is 1024 bytes.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="header"></param>
        public void WriteFileHeader(FileStream stream, FileHeader header)
        {
            //Set position to the beginning of the stream.
            stream.Position = 0;
            
            byte[] bytes = new byte[FileHeaderLength];
            //This is a token byte to assure the processor this is a correct file.
            bytes[0] = FileBeginToken;
            BitConverter.GetBytes((long)header.BlocksInFile).CopyTo(bytes, 1);
            //I havent decided what else needs to be in the header yet...so we will fill it up with blank space.
            FillArray(bytes, FileHeaderLength, 9);
            stream.Write(bytes, 0, FileHeaderLength);

            fileHeader = header;
        }

        public FileHeader ReadFileHeader(FileStream stream)
        {
            if (this.fileHeader != null)
            {
                return this.fileHeader;
            }

            FileHeader header = new FileHeader();
            stream.Position = 0;
            byte[] headerBytes = new byte[FileHeaderLength];

            stream.Read(headerBytes, 0, FileHeaderLength);

            //The file header is corrupt
            if (headerBytes[0] != 0x000A)
            {
                throw new InvalidOperationException();
            }

            long blockCount = BitConverter.ToInt64(headerBytes, 1);
            header.BlocksInFile = blockCount;

            this.fileHeader = header;
            return header;
        }

        public byte[] ReadBlock(FileStream stream, long blockSequence)
        {
            JumpToBlock(stream, blockSequence);

            byte[] bytes = new byte[BlockLength];

            stream.Read(bytes, 0, BlockLength);

            return bytes;
        }

        public void WriteBlockHeader(FileStream stream, BlockHeader header)
        {
            JumpToBlockHeader(stream, header.BlockSequence);

            byte[] bytes = new byte[BlockHeaderLength];
            bytes[0] = BlockBeginToken;
            BitConverter.GetBytes(header.BlockSequence).CopyTo(bytes, 1);
            BitConverter.GetBytes(header.FreeBytes).CopyTo(bytes, 9);
            stream.Write(bytes, 0, BlockHeaderLength);
        }

        public void WriteNewBlock(FileStream stream)
        {
            this.fileHeader.BlocksInFile = this.fileHeader.BlocksInFile + 1;
            WriteFileHeader(stream, this.fileHeader);

            //Jump to the end of the stream;
            BlockHeader header = new BlockHeader() { BlockFragmented = false, BlockSequence = fileHeader.BlocksInFile, FreeBytes = BlockLength };
            WriteBlockHeader(stream, header);
            
            stream.Write(new byte[BlockLength], 0, BlockLength);
        }

        public BlockHeader ReadBlockHeader(FileStream stream, long blockSequence)
        {
            JumpToBlockHeader(stream, blockSequence);

            byte[] bytes = new byte[BlockHeaderLength];

            stream.Read(bytes, 0, BlockHeaderLength);

            if (bytes[0] != 0x000B)
            {
                throw new InvalidOperationException();
            }

            BlockHeader header = new BlockHeader();

            header.BlockSequence = BitConverter.ToInt64(bytes, 1);
            header.FreeBytes = BitConverter.ToInt64(bytes, 9);

            return header;
        }

        public RecordHeader[] ReadRecordsInBlock(FileStream stream, long blockSequence, int recordCount)
        {
            JumpToBlock(stream, blockSequence);
            RecordHeader[] headers = new RecordHeader[recordCount];

            for (int h = 0; h < recordCount; h++)
            {
                byte[] recordHeaderBytes = new byte[RecordHeaderLength];
                stream.Read(recordHeaderBytes, 0, RecordHeaderLength);

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

        public byte[] ReadFullRecord(FileStream stream, long blockSequence, RecordHeader[] allHeadersInblock, int headToRead)
        {
            var recordToRead = allHeadersInblock[headToRead];
            JumpToRecord(stream, blockSequence, allHeadersInblock, headToRead);

            byte[] record = new byte[recordToRead.BytesInRecord];

            stream.Read(record, 0, recordToRead.BytesInRecord);
            return record;
        }

        public void WriteRecordToBlock(FileStream stream, long blockSequence, byte[] bytes)
        {

            BlockHeader header = ReadBlockHeader(stream, blockSequence);
            header.FreeBytes -= (bytes.Length + RecordHeaderLength);

            WriteBlockHeader(stream, header);
                        
            long firstAvailableByte = BlockLength - header.FreeBytes;
            stream.Position += firstAvailableByte;

            byte[] headerBytes = new byte[RecordHeaderLength];
            headerBytes[0] = RecordBeginToken;
            BitConverter.GetBytes((long)bytes.Length).CopyTo(headerBytes, 1);

            Guid recordId = Guid.NewGuid();
            recordId.ToByteArray().CopyTo(headerBytes, 9);
            stream.Write(headerBytes, 0, RecordHeaderLength);

            stream.Write(bytes, 0, bytes.Length);
        }

        #region Helper Functions

        private void JumpToBlock(FileStream stream, long blockSequence)
        {
            stream.Position = ((BlockLength + BlockHeaderLength) * blockSequence) + FileHeaderLength + BlockHeaderLength;
        }

        private void JumpToBlockHeader(FileStream stream, long blockSequence)
        {
            stream.Position = ((BlockLength + BlockHeaderLength) * blockSequence) + FileHeaderLength;
        }

        private void JumpToRecord(FileStream stream, long blockSequence, RecordHeader[] headersInBlock, int headerSequence)
        {
            long position = ((BlockLength + BlockHeaderLength) * blockSequence) + FileHeaderLength + BlockHeaderLength;
            for (int i = 0; i < headerSequence; i++)
            {
                var header = headersInBlock[i];
                position += RecordHeaderLength;
                position += header.BytesInRecord;
            }

            position += RecordHeaderLength;

            stream.Position = position;
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

using Re.Base.Data.Constants;
using Re.Base.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Re.Base.Data.Extensions
{
    public static class StreamExtensions
    {


        #region Constant Configuration
        private const int BlockHeaderLength = 128;
        private const int FileHeaderLength = 1024;
        #endregion

        #region Read Functions
        public static Byte PeekByte(this Stream stream)
        {
            byte next = (byte)stream.ReadByte();
            stream.Position--;
            return next;
        }

        public static Boolean ReadBoolean(this Stream stream)
        {
            return stream.ReadByte() == 1;
        }

        public static Int16 ReadInt16(this Stream stream)
        {
            byte[] bytes = new byte[2];
            stream.Read(bytes, 0, 2);

            return BitConverter.ToInt16(bytes, 0);
        }

        public static Int32 ReadInt32(this Stream stream)
        {
            byte[] bytes = new byte[4];
            stream.Read(bytes, 0, 4);

            return BitConverter.ToInt32(bytes, 0);
        }

        public static Int64 ReadInt64(this Stream stream)
        {
            byte[] bytes = new byte[8];
            stream.Read(bytes, 0, 8);

            return BitConverter.ToInt64(bytes, 0);
        }

        public static DateTime ReadDateTime(this Stream stream)
        {
            byte[] bytes = new byte[8];
            stream.Read(bytes, 0, 8);
            var ticks = BitConverter.ToInt64(bytes, 0);
            return DateTime.FromBinary(ticks);
        }

        public static String ReadAsciiString(this Stream stream, int bytesToRead)
        {
            byte[] bytes = new byte[bytesToRead];
            stream.Read(bytes, 0, bytesToRead);

            return Encoding.ASCII.GetString(bytes);
        }

        public static String ReadUTF8String(this Stream stream, int bytesToRead)
        {
            byte[] bytes = new byte[bytesToRead];
            stream.Read(bytes, 0, bytesToRead);

            return Encoding.UTF8.GetString(bytes);
        }

        public static DataType ReadDataType(this Stream stream)
        {
            byte dataTypeToken = (byte)stream.ReadByte();

            if (dataTypeToken == Tokens.DateTimeTypeToken) return DataType.DateTime;
            else if (dataTypeToken == Tokens.Int64TypeToken) return DataType.Int64;
            else if (dataTypeToken == Tokens.Int32TypeToken) return DataType.Int32;
            else if (dataTypeToken == Tokens.Int16TypeToken) return DataType.Int16;
            else if (dataTypeToken == Tokens.BooleanTypeToken) return DataType.Boolean;
            else if (dataTypeToken == Tokens.DecimalTypeToken) return DataType.Decimal;
            else if (dataTypeToken == Tokens.BigStringTypeToken) return DataType.BigString;
            else if (dataTypeToken == Tokens.LittleStringTypeToken) return DataType.LittleString;
            else
            {
                throw new InvalidOperationException();
            }
        }

        public static IndexDefinition ReadIndexDefinition(this Stream stream)
        {
            string name = stream.ReadAsciiString(Lengths.IndexNameLength);
            short typeId = stream.ReadInt16();
            Indexing.Enums.IndexType type =
                typeId == 1 ? Indexing.Enums.IndexType.InMemoryIndex :
                throw new Exception();

            //int fieldCount = stream.ReadInt32();

            //int[] fields = new int[fieldCount];

            //for (int i = 0; i < fieldCount; i++)
            //{
            //    fields[i] = stream.ReadInt32();
            //}

            int fieldId = stream.ReadInt32();

            return new IndexDefinition()
            {
                IndexName = name,
                IndexType = type,
                FieldId = fieldId
            };
        }

        public static void WriteIndexDefinition(this Stream stream, IndexDefinition definition)
        {
            stream.WriteAsciiString(definition.IndexName, Lengths.IndexNameLength);

            short typeId = definition.IndexType == Indexing.Enums.IndexType.InMemoryIndex ? (short)1 :
                throw new Exception();

            stream.WriteInt16(typeId);

            stream.WriteInt32(definition.FieldId);

            //stream.WriteInt32(definition.FieldIds.Length);
            
            //foreach(int fieldId in definition.FieldIds)
            //{
            //    stream.WriteInt32(fieldId);
            //}

        }

        public static FieldDefinition ReadFieldDefinition(this Stream stream)
        {
            FieldDefinition field = new FieldDefinition();

            int fieldNameLength = stream.ReadInt32();
            string fieldName = stream.ReadUTF8String(fieldNameLength);
            field.FieldName = fieldName;
            field.DataType = stream.ReadDataType();
            field.Nullable = stream.ReadBoolean();

            return field;

        }

        public static RecordHeader ReadRecordHeader(this Stream stream)
        {
            RecordHeader header = new RecordHeader();

            byte[] headerBytes = new byte[Lengths.RecordHeaderLength];

            stream.Read(headerBytes, 0, Lengths.RecordHeaderLength);

            if (headerBytes[0] != Tokens.RecordBeginToken)
            {
                throw new InvalidOperationException();
            }

            header.IsDeleted = BitConverter.ToBoolean(headerBytes, 1);

            return header;
        }

        public static BlockHeader ReadBlockHeader(this Stream stream)
        {
            byte beginToken = (byte)stream.ReadByte();

            if (beginToken != Tokens.BlockBeginToken)
            {
                throw new InvalidOperationException();
            }

            BlockHeader header = new BlockHeader();

            header.BlockSequence = stream.ReadInt64();
            header.FreeBytes = stream.ReadInt64();
            header.RecordCount = stream.ReadInt64();

            return header;
        }

        public static FileHeader ReadFileHeader(this Stream stream)
        {
            FileHeader header = new FileHeader();
            
            byte[] headerBytes = new byte[FileHeaderLength];

            stream.Read(headerBytes, 0, FileHeaderLength);

            //The file header is corrupt
            if (headerBytes[0] != Tokens.FileBeginToken)
            {
                    throw new InvalidOperationException();
            }

            long blockCount = BitConverter.ToInt64(headerBytes, 1);
            header.BlocksInFile = blockCount;

            return header;
        }

		public static IndexDefinition ReadIndex(this Stream stream)
		{
			IndexDefinition index = new IndexDefinition();
			index.IndexName = stream.ReadUTF8String(100);

            return index;
		}

		#endregion

		#region Write Functions

		public static void WriteBoolean(this Stream stream, Boolean value)
        {
            byte byteValue =(byte)(value == true ? 1 : 0);
            stream.WriteByte(byteValue);
        }

        public static void WriteInt16(this Stream stream, Int16 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void WriteInt32(this Stream stream, Int32 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void WriteInt64(this Stream stream, Int64 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void WriteDateTime(this Stream stream, DateTime value)
        {
            byte[] bytes = BitConverter.GetBytes(value.Ticks);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void WriteAsciiString(this Stream stream, string value, int? size = null)
        {
            byte[] bytes;
            byte[] encodedBytes = Encoding.ASCII.GetBytes(value);
            if (size.HasValue)
            {
                bytes = new byte[size.Value];
                encodedBytes.CopyTo(bytes, 0);
                FillArrayWithEmptySpace(encodedBytes, encodedBytes.Length - 1);
            }
            else
            {
                bytes = encodedBytes;
            }

            stream.Write(bytes, 0, bytes.Length);
        }

        public static void WriteUTF8String(this Stream stream, string value, int? size = null)
        {
            byte[] bytes;
            byte[] encodedBytes = Encoding.UTF8.GetBytes(value);
            if (size.HasValue)
            {
                bytes = new byte[size.Value];
                encodedBytes.CopyTo(bytes, 0);
                FillArrayWithEmptySpace(encodedBytes, encodedBytes.Length - 1);
            }
            else
            {
                bytes = encodedBytes;
            }

            stream.Write(bytes, 0, bytes.Length);
        }

        public static void WriteDataType(this Stream stream, DataType value)
        {
            if (value == DataType.DateTime) stream.WriteByte(Tokens.DateTimeTypeToken);
            else if (value == DataType.Int64) stream.WriteByte(Tokens.Int64TypeToken);
            else if (value == DataType.Int32) stream.WriteByte(Tokens.Int32TypeToken);
            else if (value == DataType.Int16) stream.WriteByte(Tokens.Int16TypeToken);
            else if (value == DataType.Boolean) stream.WriteByte(Tokens.BooleanTypeToken);
            else if (value == DataType.Decimal) stream.WriteByte(Tokens.DecimalTypeToken);
            else if (value == DataType.BigString) stream.WriteByte(Tokens.BigStringTypeToken);
            else if (value == DataType.LittleString) stream.WriteByte(Tokens.LittleStringTypeToken);
            else
                throw new InvalidOperationException();

        }

        public static void WriteFieldDefinition(this Stream stream, FieldDefinition fieldDefinition)
        {
            byte[] fieldNameBytes = Encoding.UTF8.GetBytes(fieldDefinition.FieldName);
            int fieldNameLength = fieldNameBytes.Length;
            stream.WriteInt32(fieldNameLength);
            stream.Write(fieldNameBytes, 0, fieldNameLength);
            stream.WriteDataType(fieldDefinition.DataType);
            stream.WriteBoolean(fieldDefinition.Nullable);
        }

        public static void WriteBlockHeader(this Stream stream, BlockHeader blockHeader)
        {
            stream.WriteByte(Tokens.BlockBeginToken);
            stream.WriteInt64(blockHeader.BlockSequence);
            stream.WriteInt64(blockHeader.FreeBytes);
            stream.WriteInt64(blockHeader.RecordCount);
        }

        public static void WriteRecordHeader(this Stream stream, RecordHeader recordHeader)
        {
            byte[] headerBytes = new byte[Lengths.RecordHeaderLength];
            MemoryStream headerStream = new MemoryStream(headerBytes);

            headerStream.WriteByte(Tokens.RecordBeginToken);
            headerStream.WriteBoolean(recordHeader.IsDeleted);

            stream.Write(headerBytes, 0, Lengths.RecordHeaderLength);

        }

        #endregion

        #region Helper Functions

        private static void FillArrayWithEmptySpace(byte[] bytes, long startingIndex = 0, long? endingIndex = null)
        {
            long count = endingIndex ?? bytes.Length;
            for (long i = startingIndex; i < count; i++)
            {
                bytes[i] = 0;
            }
        }

        #endregion

        #region Seek Functions 

        public static void SeekToBlockHeader(this Stream fileStream, long blockIndex)
        {
            fileStream.Position = ((Lengths.BlockLength + Lengths.BlockHeaderLength) * (blockIndex + 1)) + Lengths.FileHeaderLength;
        }

        public static void SeekToBlockContents(this Stream fileStream, long blockIndex)
        {
            fileStream.Position = ((Lengths.BlockLength + Lengths.BlockHeaderLength) * (blockIndex + 1)) + Lengths.FileHeaderLength + Lengths.BlockHeaderLength;
        }

        public static void SeekToRecordInBlock(this Stream fileStream, long blockIndex, int recordSize, int recordIndex)
        {
            fileStream.Position = ((Lengths.BlockLength + Lengths.BlockHeaderLength) * (blockIndex + 1)) + Lengths.FileHeaderLength + Lengths.BlockHeaderLength + (recordSize * recordIndex);
        }

        public static void SeekToRecord(this Stream fileStream, DataStructure schema, long blockIndex, long recordIndex)
        {
            var blockContents = ((Lengths.BlockLength + Lengths.BlockHeaderLength) * (blockIndex + 1)) + Lengths.FileHeaderLength + Lengths.BlockHeaderLength;

            fileStream.Position = blockContents + (recordIndex * (schema.GetRecordSize() + Lengths.RecordHeaderLength));
        }

        public static void SeekToRecordContents(this Stream fileStream, DataStructure schema, long blockIndex, long recordIndex)
        {
            var blockContents = ((Lengths.BlockLength + Lengths.BlockHeaderLength) * (blockIndex + 1)) + Lengths.FileHeaderLength + Lengths.BlockHeaderLength;
            fileStream.Position = blockContents + (recordIndex * (schema.GetRecordSize() + Lengths.RecordHeaderLength)) + Lengths.RecordHeaderLength;
        }

        #endregion
    }
}

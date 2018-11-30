using Re.Base.Data.Constants;
using Re.Base.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Re.Base.Data.Extensions
{
    public static class FileStreamExtensions
    {


        #region Constant Configuration
        private const int BlockHeaderLength = 128;
        private const int FileHeaderLength = 1024;
        #endregion

        #region Read Functions
        public static Boolean ReadBoolean(this FileStream fileStream)
        {
            return fileStream.ReadByte() == 1;
        }

        public static Int16 ReadInt16(this FileStream fileStream)
        {
            byte[] bytes = new byte[4];
            fileStream.Read(bytes, 0, 4);

            return BitConverter.ToInt16(bytes, 0);
        }

        public static Int32 ReadInt32(this FileStream fileStream)
        {
            byte[] bytes = new byte[4];
            fileStream.Read(bytes, 0, 4);

            return BitConverter.ToInt32(bytes, 0);
        }

        public static Int64 ReadInt64(this FileStream fileStream)
        {
            byte[] bytes = new byte[8];
            fileStream.Read(bytes, 0, 8);

            return BitConverter.ToInt64(bytes, 0);
        }

        public static DateTime ReadDateTime(this FileStream fileStream)
        {
            byte[] bytes = new byte[8];
            fileStream.Read(bytes, 0, 8);
            var ticks = BitConverter.ToInt64(bytes, 0);
            return DateTime.FromBinary(ticks);
        }

        public static String ReadAsciiString(this FileStream fileStream, int bytesToRead)
        {
            byte[] bytes = new byte[bytesToRead];
            fileStream.Read(bytes, 0, bytesToRead);

            return Encoding.ASCII.GetString(bytes);
        }

        public static String ReadUTF8String(this FileStream fileStream, int bytesToRead)
        {
            byte[] bytes = new byte[bytesToRead];
            fileStream.Read(bytes, 0, bytesToRead);

            return Encoding.UTF8.GetString(bytes);
        }

        public static DataType ReadDataType(this FileStream fileStream)
        {
            byte dataTypeToken = (byte)fileStream.ReadByte();

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

        public static FieldDefinition ReadFieldDefinition(this FileStream fileStream)
        {
            FieldDefinition field = new FieldDefinition();

            int fieldNameLength = fileStream.ReadInt32();
            string fieldName = fileStream.ReadUTF8String(fieldNameLength);
            field.FieldName = fieldName;
            field.DataType = fileStream.ReadDataType();
            field.Nullable = fileStream.ReadBoolean();

            return field;

        }

        public static BlockHeader ReadBlockHeader(this FileStream fileStream)
        {
            byte beginToken = (byte)fileStream.ReadByte();

            if (beginToken != Tokens.BlockBeginToken)
            {
                throw new InvalidOperationException();
            }

            BlockHeader header = new BlockHeader();

            header.BlockSequence = fileStream.ReadInt64();
            header.FreeBytes = fileStream.ReadInt64();
            header.RecordCount = fileStream.ReadInt64();

            return header;
        }

        public static FileHeader ReadFileHeader(this FileStream fileStream)
        {
            FileHeader header = new FileHeader();
            
            byte[] headerBytes = new byte[FileHeaderLength];

            fileStream.Read(headerBytes, 0, FileHeaderLength);

            //The file header is corrupt
            if (headerBytes[0] != Tokens.FileBeginToken)
            {
                throw new InvalidOperationException();
            }

            long blockCount = BitConverter.ToInt64(headerBytes, 1);
            header.BlocksInFile = blockCount;

            return header;
        }
        #endregion

        #region Write Functions

        public static void WriteBoolean(this FileStream fileStream, Boolean value)
        {
            byte byteValue =(byte)(value == true ? 1 : 0);
            fileStream.WriteByte(byteValue);
        }

        public static void WriteInt16(this FileStream fileStream, Int16 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            fileStream.Write(bytes, 0, bytes.Length);
        }

        public static void WriteInt32(this FileStream fileStream, Int32 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            fileStream.Write(bytes, 0, bytes.Length);
        }

        public static void WriteInt64(this FileStream fileStream, Int64 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            fileStream.Write(bytes, 0, bytes.Length);
        }

        public static void WriteDateTime(this FileStream fileStream, DateTime value)
        {
            byte[] bytes = BitConverter.GetBytes(value.Ticks);
            fileStream.Write(bytes, 0, bytes.Length);
        }

        public static void WriteAsciiString(this FileStream fileStream, string value, int? size = null)
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
            
            fileStream.Write(bytes, 0, bytes.Length);
        }

        public static void WriteUTF8String(this FileStream fileStream, string value, int? size = null)
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

            fileStream.Write(bytes, 0, bytes.Length);
        }

        public static void WriteDataType(this FileStream fileStream, DataType value)
        {
            if (value == DataType.DateTime) fileStream.WriteByte(Tokens.DateTimeTypeToken);
            else if (value == DataType.Int64) fileStream.WriteByte(Tokens.Int64TypeToken);
            else if (value == DataType.Int32) fileStream.WriteByte(Tokens.Int32TypeToken);
            else if (value == DataType.Int16) fileStream.WriteByte(Tokens.Int16TypeToken);
            else if (value == DataType.Boolean) fileStream.WriteByte(Tokens.BooleanTypeToken);
            else if (value == DataType.Decimal) fileStream.WriteByte(Tokens.DecimalTypeToken);
            else if (value == DataType.BigString) fileStream.WriteByte(Tokens.BigStringTypeToken);
            else if (value == DataType.LittleString) fileStream.WriteByte(Tokens.LittleStringTypeToken);
            else
                throw new InvalidOperationException();

        }

        public static void WriteFieldDefinition(this FileStream fileStream, FieldDefinition fieldDefinition)
        {
            byte[] fieldNameBytes = Encoding.UTF8.GetBytes(fieldDefinition.FieldName);
            int fieldNameLength = fieldNameBytes.Length;
            fileStream.WriteInt32(fieldNameLength);
            fileStream.Write(fieldNameBytes, 0, fieldNameLength);
            fileStream.WriteDataType(fieldDefinition.DataType);
            fileStream.WriteBoolean(fieldDefinition.Nullable);
        }

        public static void WriteBlockHeader(this FileStream fileStream, BlockHeader blockHeader)
        {
            fileStream.WriteByte(Tokens.BlockBeginToken);
            fileStream.WriteInt64(blockHeader.BlockSequence);
            fileStream.WriteInt64(blockHeader.FreeBytes);
            fileStream.WriteInt64(blockHeader.RecordCount);
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

        public static void SeekToBlockHeader(this FileStream fileStream, long blockIndex)
        {
            fileStream.Position = ((Lengths.BlockLength + Lengths.BlockHeaderLength) * (blockIndex + 1)) + Lengths.FileHeaderLength;
        }

        public static void SeekToBlockContents(this FileStream fileStream, long blockIndex)
        {
            fileStream.Position = ((Lengths.BlockLength + Lengths.BlockHeaderLength) * (blockIndex + 1)) + Lengths.FileHeaderLength + Lengths.BlockHeaderLength;
        }

        public static void SeekToRecordInBlock(this FileStream fileStream, long blockIndex, int recordSize, int recordIndex)
        {
            fileStream.Position = ((Lengths.BlockLength + Lengths.BlockHeaderLength) * (blockIndex + 1)) + Lengths.FileHeaderLength + Lengths.BlockHeaderLength + (recordSize * recordIndex);
        }

        public static void SeekToRecord(this FileStream fileStream, DataStructure schema, long blockIndex, long recordIndex)
        {
            var blockContents = ((Lengths.BlockLength + Lengths.BlockHeaderLength) * (blockIndex + 1)) + Lengths.FileHeaderLength + Lengths.BlockHeaderLength;

            fileStream.Position = blockContents + (recordIndex * schema.GetRecordSize());
        }

        #endregion
    }
}

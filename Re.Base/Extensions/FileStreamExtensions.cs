using Re.Base.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Re.Base.Extensions
{
    public static class FileStreamExtensions
    {
        #region Tokens
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

        #endregion

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

            if (dataTypeToken == DateTimeTypeToken) return DataType.DateTime;
            else if (dataTypeToken == Int64TypeToken) return DataType.Int64;
            else if (dataTypeToken == Int32TypeToken) return DataType.Int32;
            else if (dataTypeToken == Int16TypeToken) return DataType.Int16;
            else if (dataTypeToken == DecimalTypeToken) return DataType.Decimal;
            else if (dataTypeToken == BigStringTypeToken) return DataType.BigString;
            else if (dataTypeToken == LittleStringTypeToken) return DataType.LittleString;
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
            byte[] bytes = new byte[BlockHeaderLength];

            fileStream.Read(bytes, 0, BlockHeaderLength);

            if (bytes[0] != BlockBeginToken)
            {
                throw new InvalidOperationException();
            }

            BlockHeader header = new BlockHeader();

            header.BlockSequence = BitConverter.ToInt64(bytes, 1);
            header.FreeBytes = BitConverter.ToInt64(bytes, 9);

            return header;
        }

        public static FileHeader ReadFileHeader(this FileStream fileStream)
        {
            FileHeader header = new FileHeader();
            
            byte[] headerBytes = new byte[FileHeaderLength];

            fileStream.Read(headerBytes, 0, FileHeaderLength);

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
            if (value == DataType.DateTime) fileStream.WriteByte(DateTimeTypeToken);
            else if (value == DataType.Int64) fileStream.WriteByte(Int64TypeToken);
            else if (value == DataType.Int32) fileStream.WriteByte(Int32TypeToken);
            else if (value == DataType.Int16) fileStream.WriteByte(Int16TypeToken);
            else if (value == DataType.Decimal) fileStream.WriteByte(DecimalTypeToken);
            else if (value == DataType.BigString) fileStream.WriteByte(BigStringTypeToken);
            else if (value == DataType.LittleString) fileStream.WriteByte(LittleStringTypeToken);

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
    }
}

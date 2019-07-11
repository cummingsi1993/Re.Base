using Re.Base.Data.Extensions;
using Re.Base.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Re.Base.Data.Constants;
using Re.Base.Data.Interfaces;
using Re.Base.Data.Creation;

namespace Re.Base.Data.Logic
{
    public class RecordBlock : Block<Record>
    {
        private RecordBlock(FileStream stream, DataStructure schema, long index)
            : base(stream, schema, index)
        {
        }

        public static RecordBlock LoadFromStream(FileStream stream, DataStructure schema, long index)
        {
            var block = new RecordBlock(stream, schema, index);
            
            return block;
        }

        public static RecordBlock CreateNew(FileStream stream, DataStructure schema, long index)
        {
            stream.SeekToBlockHeader(index);
            stream.WriteBlockHeader(new BlockHeader() { BlockFragmented = false, BlockSequence = index, FreeBytes = Constants.Lengths.BlockLength, RecordCount = 0 });
            stream.Write(new byte[Lengths.BlockLength], 0, Lengths.BlockLength);

            var block = new RecordBlock(stream, schema, index);

            return block;
        }

        protected override Record GetValue(DataStructure schema, FieldTypeFactory fieldTypeFactory, long currentPosition, byte[] bytes)
        {
            Record record = new Record() { Fields = new RecordField[schema.Fields.Count], Location = currentPosition };
            MemoryStream stream = new MemoryStream(bytes);
            for (int i = 0; i < schema.Fields.Count; i++)
            {
                var field = schema.Fields[i];
            
                IDataFieldType fieldType = fieldTypeFactory.GetFieldType(field.DataType);
                object fieldValue = fieldType.ReadFromStream(stream);
                record.Fields[i] = new RecordField() { DataType = field.DataType, Value = fieldValue };
                
            }
            return record;
        }
    }
}

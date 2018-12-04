using Re.Base.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Re.Base.Data.Creation;
using Re.Base.Data.Extensions;
using Re.Base.Data.Interfaces;
using Re.Base.Data.Constants;

namespace Re.Base.Data.Logic
{
    public class IndexBlock : Block<Index>
    {

		public IndexBlock(FileStream stream, DataStructure schema, long index)
			:base(stream, schema, index)
		{

		}

		public static IndexBlock LoadFromStream(FileStream stream, DataStructure schema, long index)
		{
			var block = new IndexBlock(stream, schema, index);

			return block;
		}

		public static IndexBlock CreateNew(FileStream stream, DataStructure schema, long index)
		{
			stream.SeekToBlockHeader(index);
			stream.WriteBlockHeader(new BlockHeader() { BlockFragmented = false, BlockSequence = index, FreeBytes = Constants.Lengths.BlockLength, RecordCount = 0 });
			stream.Write(new byte[Lengths.BlockLength], 0, Lengths.BlockLength);

			var block = new IndexBlock(stream, schema, index);

			return block;
		}

		protected override Index GetValue(DataStructure schema, FieldTypeFactory fieldTypeFactory, byte[] bytes)
		{
			Index index = new Index() { Keys = new object[schema.Fields.Count - 1] };
			MemoryStream stream = new MemoryStream(bytes);

			index.RecordSequence = stream.ReadInt64();

			for (int i = 1; i < schema.Fields.Count; i++)
			{
				var field = schema.Fields[i];

				IDataFieldType fieldType = fieldTypeFactory.GetFieldType(field.DataType);
				object fieldValue = fieldType.ReadFromStream(stream);
				index.Keys[i] = fieldValue;

			}
			return index;
		}
	}
}

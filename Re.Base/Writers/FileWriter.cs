using Re.Base.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Re.Base.Writers
{
    public class FileWriter<TModel>
        where TModel : class
    {
        string fileName;
		long blockSize = 8000;
		FileHeader header;

        public FileWriter(string databaseLocation)
        {
			string sourceName = typeof(TModel).Name;
            fileName = $"{databaseLocation}/data_{sourceName}.rbs";
        }

        public void WriteNewModel(TModel model)
        {
            using (FileStream stream = File.Exists(fileName) ?
                OpenFile() :
                this.StartNewFile())
            {
                //DbRecord<TModel> record = new DbRecord<TModel>(model);
                string serializedModel = Newtonsoft.Json.JsonConvert.SerializeObject(model);

                byte[] modelBytes = Encoding.UTF8.GetBytes(serializedModel);

				FindAvailableBlock(stream, modelBytes.Length);
				

                stream.Write(modelBytes, 0, modelBytes.Length);
                stream.Close();
            }
        }

		private FileStream OpenFile()
		{
			FileStream fileStream = File.Open(fileName, FileMode.Open);
			ReadFileHeader(fileStream);

			return fileStream;
		}

        private FileStream StartNewFile()
        {
            FileStream fileStream = System.IO.File.Create(fileName);
			header = new FileHeader() { BlocksInFile = 0 };

			WriteHeader(fileStream);
            WriteBlockHeader(fileStream, new BlockHeader() { BlockSequence = 0, FreeBytes = 8000 });
            AddBlockToHeader(fileStream);
			long beginningOfFirstBlock = 9;
			WriteEmptyBlock(fileStream);
			fileStream.Position = beginningOfFirstBlock;
            return fileStream;
        }

        private void WriteHeader(FileStream stream)
        {
			stream.WriteByte(0x000A);
            byte[] bytes = BitConverter.GetBytes((long)header.BlocksInFile);
            stream.Write(bytes, 0, 8);
        }

		private void FindAvailableBlock(FileStream stream, long bytesNeeded)
		{
			BlockHeader current = null;
			while(current == null || current.FreeBytes < bytesNeeded)
			{
				current = ReadBlockHeader(stream);
				
				//if (!blockFound && this.header.BlocksInFile == header.BlockSequence - 1)
				//{
				//	WriteBlockHeader(stream, new BlockHeader() { BlockSequence = header.BlockSequence + 1, FreeBytes = 8000 });
				//}
			}

			stream.Position = stream.Position + (8000 - current.FreeBytes);
		}

		private void ReadFileHeader(FileStream stream)
		{
			header = new FileHeader();

			stream.Position = 0;
			byte[] headerBytes = new byte[1];
			byte[] blockCountBytes = new byte[8];

			stream.Read(headerBytes, 0, 1);
			stream.Read(blockCountBytes, 0, 8);

			//The file header is corrupt
			if (headerBytes[0] != 0x000A)
			{
				throw new InvalidOperationException();
			}

			long blockCount = BitConverter.ToInt64(blockCountBytes, 0);
			header.BlocksInFile = blockCount;
		}

		private BlockHeader ReadBlockHeader(FileStream stream)
		{
			byte[] headerByte = new byte[1];
			stream.Read(headerByte, 0, 1);

			if (headerByte[0] != 0x000B)
			{
				throw new InvalidOperationException();
			}

			byte[] blockSequenceBytes = new byte[8];
			stream.Read(blockSequenceBytes, 0, 8);

			long blockSequence = BitConverter.ToInt64(blockSequenceBytes, 0);

			byte[] blockAvailableBytes = new byte[8];
			stream.Read(blockAvailableBytes, 0, 8);

			long blockAvailable = BitConverter.ToInt64(blockAvailableBytes, 0);
			return new BlockHeader() { BlockSequence = blockSequence, FreeBytes = blockAvailable };
		}

        private void WriteBlockHeader(FileStream stream, BlockHeader blockHeader)
        {
            stream.WriteByte(0x000B);
            byte[] blockSequence = BitConverter.GetBytes(blockHeader.BlockSequence);
            byte[] blockHeaderBytes = BitConverter.GetBytes(blockHeader.FreeBytes);
            stream.Write(blockSequence, 0, 8);
            stream.Write(blockHeaderBytes, 0, 8);
        }

		private void WriteEmptyBlock(FileStream stream)
		{
			byte[] block = new byte[blockSize];
			for (int i = 0; i < blockSize; i++)
			{
				block[i] = 0;
			}

			stream.Write(block, 0, block.Length);
		}

        private void AddBlockToHeader(FileStream stream)
        {
            //Write into the header that there is now a new block;
            long pos = stream.Position;
            stream.Position = 1;

            byte[] blockCountBytes = new byte[8];
            stream.Read(blockCountBytes, 0, 8);
			stream.Position = 1;
            //I could probably optimize this by doing bit-math
            long blockCount = BitConverter.ToInt64(blockCountBytes, 0);
            blockCount++;

            byte[] bytes = BitConverter.GetBytes(blockCount);
            stream.Write(bytes, 0, 8);

            stream.Position = pos;
        }



    }
}

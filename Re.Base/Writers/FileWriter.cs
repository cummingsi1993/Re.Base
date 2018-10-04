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
        public FileWriter(string databaseLocation)
        {

            string sourceName = typeof(TModel).Name;
            fileName = $"{databaseLocation}/data_{sourceName}.rbs";

        }

        public void WriteNewModel(TModel model)
        {
            using (FileStream stream = File.Exists(fileName) ?
                File.Open(fileName, FileMode.Open) :
                this.StartNewFile())
            {
                DbRecord<TModel> record = new DbRecord<TModel>(model);
                string serializedModel = Newtonsoft.Json.JsonConvert.SerializeObject(record);

                byte[] modelBytes = Encoding.UTF8.GetBytes(serializedModel);
                stream.Write(modelBytes, 0, modelBytes.Length);
                stream.Close();
            }
        }


        private FileStream StartNewFile()
        {
            FileStream fileStream = System.IO.File.Create(fileName);

            WriteHeader(fileStream);
            WriteBlockHeader(fileStream, new BlockHeader() { BlockSequence = 0, BytesInBlock = 0 });
            AddBlockToHeader(fileStream);
            return fileStream;
        }

        private void WriteHeader(FileStream stream)
        {
            stream.WriteByte(0x000A);

            byte[] bytes = BitConverter.GetBytes((long)0);
            stream.Write(bytes, 0, 8);

        }

        private void WriteBlockHeader(FileStream stream, BlockHeader blockHeader)
        {
            stream.WriteByte(0x000B);
            byte[] blockSequence = BitConverter.GetBytes(blockHeader.BlockSequence);
            byte[] blockHeaderBytes = BitConverter.GetBytes(blockHeader.BytesInBlock);
            stream.Write(blockSequence, 0, 8);
            stream.Write(blockHeaderBytes, 0, 8);
        }

        private void AddBlockToHeader(FileStream stream)
        {
            //Write into the header that there is now a new block;
            long pos = stream.Position;
            stream.Position = 1;

            byte[] blockCountBytes = new byte[8];
            stream.Read(blockCountBytes, 0, 8);

            //I could probably optimize this by doing bit-math
            long blockCount = BitConverter.ToInt64(blockCountBytes, 0);
            blockCount++;

            byte[] bytes = BitConverter.GetBytes(blockCount);
            stream.Write(bytes, 0, 8);

            stream.Position = pos;
        }



    }
}

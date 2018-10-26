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
        DataManager manager;
        public FileWriter(string databaseLocation)
        {
			string sourceName = typeof(TModel).Name;
            fileName = $"{databaseLocation}/data_{sourceName}.rbs";

            manager = new DataManager();
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

				if (header.BlocksInFile > 0)
                {
                    bool blockFound = false;
                    long blockSequence = 1;
                    while(!blockFound)
                    {
                        BlockHeader block = manager.ReadBlockHeader(stream, blockSequence);
                        if (block.FreeBytes > modelBytes.Length)
                        {
                            manager.WriteRecordToBlock(stream, blockSequence, modelBytes);
                            blockFound = true;
                        }
                        blockSequence++;
                    }

                    
                }
                else
                {
                    manager.WriteNewBlock(stream);
                    manager.WriteRecordToBlock(stream, 1, modelBytes);
                }


                stream.Close();
            }
        }

		private FileStream OpenFile()
		{
			FileStream fileStream = File.Open(fileName, FileMode.Open);
            header = manager.ReadFileHeader(fileStream);

			return fileStream;
		}

        private FileStream StartNewFile()
        {
            FileStream fileStream = System.IO.File.Create(fileName);
			header = new FileHeader() { BlocksInFile = 0 };

            manager.WriteFileHeader(fileStream, header);
            
			
            return fileStream;
        }

        private void WriteHeader(FileStream stream)
        {
			stream.WriteByte(0x000A);
            byte[] bytes = BitConverter.GetBytes((long)header.BlocksInFile);
            stream.Write(bytes, 0, 8);
        }

		

        
    }
}

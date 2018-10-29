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

            manager = new DataManager(fileName);
        }

        public void WriteNewModel(TModel model)
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
                    BlockHeader block = manager.ReadBlockHeader(blockSequence);
                    if (block.FreeBytes > modelBytes.Length)
                    {
                        manager.WriteRecordToBlock(blockSequence, modelBytes);
                        blockFound = true;
                    }
                    blockSequence++;
                }

                    
            }
            else
            {
                manager.WriteNewBlock();
                manager.WriteRecordToBlock(1, modelBytes);
            }
            
        }

		

		

        
    }
}

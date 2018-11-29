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
		long blockSize = 8000;
		FileHeader header;
        DataManager manager;
        public FileWriter(string databaseLocation)
        {
			string sourceName = typeof(TModel).Name;
            

            manager = new DataManager(databaseLocation, sourceName);
        }

        public void WriteNewModel(TModel model)
        {
            manager.AddField(new FieldDefinition() { DataType = DataType.Int32, FieldName = "Id", Nullable = false });
            manager.AddField(new FieldDefinition() { DataType = DataType.Boolean, FieldName = "Test", Nullable = false });
            manager.AddField(new FieldDefinition() { DataType = DataType.DateTime, FieldName = "DateOfBirth", Nullable = false });
            manager.AddField(new FieldDefinition() { DataType = DataType.LittleString, FieldName = "Sex", Nullable = false });
            manager.AddField(new FieldDefinition() { DataType = DataType.LittleString, FieldName = "Barcode", Nullable = false });
            manager.AddField(new FieldDefinition() { DataType = DataType.LittleString, FieldName = "LabLocation", Nullable = false });
            manager.AddField(new FieldDefinition() { DataType = DataType.DateTime, FieldName = "TimeCreated", Nullable = false });
            manager.AddField(new FieldDefinition() { DataType = DataType.DateTime, FieldName = "TimeUpdated", Nullable = false });

            //manager.InsertRecord(model.)

            //DbRecord<TModel> record = new DbRecord<TModel>(model);
            //      string serializedModel = Newtonsoft.Json.JsonConvert.SerializeObject(model);
            //      byte[] modelBytes = Encoding.UTF8.GetBytes(serializedModel);

            //if (header.BlocksInFile > 0)
            //      {
            //          bool blockFound = false;
            //          long blockSequence = 1;
            //          while(!blockFound)
            //          {
            //              BlockHeader block = manager.ReadBlockHeader(blockSequence);
            //              if (block.FreeBytes > modelBytes.Length)
            //              {
            //                  manager.WriteRecordToBlock(blockSequence, modelBytes);
            //                  blockFound = true;
            //              }
            //              blockSequence++;
            //          }


            //      }
            //      else
            //      {
            //          manager.WriteNewBlock();
            //          manager.WriteRecordToBlock(1, modelBytes);
            //      }

        }

		

		

        
    }
}

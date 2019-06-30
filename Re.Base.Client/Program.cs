using Re.Base.Client.Model;
using Re.Base.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Re.Base.Data.Models;
using Re.Base.Data.Logic;
using System.Diagnostics;

namespace Re.Base.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Run().Wait();

        }


        static async Task Run()
        {


            //var instructions = StorageBuilder.CreateFileBasedStorage("D:\\Temp\\ReBase")
            //    .GetDbSet<Instruction>();

            Data.DataStore manager = new Data.DataStore(@"C:\Temp\ReBase", "Instruction");
            //await SaveInstructions(manager);
            //var query = manager.Query(r => (int)r.Fields.First().Value > 5000 && (int)r.Fields.First().Value < 6000);
            var timer = Stopwatch.StartNew();

            timer.Start();
            var record = manager.ReadRecord(1);
            timer.Stop();

            Console.Write(timer.ElapsedMilliseconds);

            var record2 = manager.ReadRecord(5);
            var record3 = manager.ReadRecord(6);
            var record4 = manager.ReadRecord(7);
            var record5 = manager.ReadRecord(8);
            var record6 = manager.ReadRecord(250000);
            var record7 = manager.ReadRecord(10);
            var record8 = manager.ReadRecord(11);
            var record9 = manager.ReadRecord(12);

            var records = manager.ReadAllRecords();

            var record10 = manager.Query(x => (int)x.Fields[0].Value == 350000);

            

			//var people = (from Person person in persons
			//			  where person.Tags.Contains("ipsum")
			//			  select person.Tags).ToList();

            //var peopleThatHaveIpsumTwice = (from Person person in persons
            //                                where person.Tags.Count(x => x == "ipsum") > 1
            //                                select person).ToList();

            //var p = persons.Find(new Guid("932c9159-f8b7-42ce-8a18-3e6388ace0d3"));

            Console.ReadLine();
          

        }


        public static async Task SaveInstructions(Data.DataStore store)
        {
            //string sample_data = System.IO.File.ReadAllText("D:\\Temp\\ReBase\\sample_instructions.json");
            //var instruction_data = Newtonsoft.Json.JsonConvert.DeserializeObject<Instruction[]>(sample_data);


            store.AddField(new FieldDefinition() { DataType = DataType.Int32, FieldName = "Id", Nullable = false });
            store.AddField(new FieldDefinition() { DataType = DataType.Boolean, FieldName = "Test", Nullable = false });
            store.AddField(new FieldDefinition() { DataType = DataType.DateTime, FieldName = "DateOfBirth", Nullable = false });
            store.AddField(new FieldDefinition() { DataType = DataType.LittleString, FieldName = "Sex", Nullable = false });
            store.AddField(new FieldDefinition() { DataType = DataType.LittleString, FieldName = "Barcode", Nullable = false });
            store.AddField(new FieldDefinition() { DataType = DataType.LittleString, FieldName = "LabLocation", Nullable = false });
            store.AddField(new FieldDefinition() { DataType = DataType.DateTime, FieldName = "TimeCreated", Nullable = false });
            store.AddField(new FieldDefinition() { DataType = DataType.DateTime, FieldName = "TimeUpdated", Nullable = false });


            DateTime dateOfBirth = new DateTime(1995, 4, 23);

            var timer = Stopwatch.StartNew();

            timer.Start();
            for (int i = 0; i < 5000000; i++)
            {
                store.InsertRecord(i + 1, true, dateOfBirth, "F", "XXXXXXXXXX", "LCA", DateTime.Now, DateTime.Now);
            }
            timer.Stop();

            Console.Write(timer.ElapsedMilliseconds);
        }


        public static async Task SetupSomeData(DbSet<Person> persons)
        {
			string sample_data = System.IO.File.ReadAllText("D:\\Temp\\ReBase\\sample_people.json");

			var people = Newtonsoft.Json.JsonConvert.DeserializeObject<Person[]>(sample_data);

            persons.AddRange(people);

		}
	}
}

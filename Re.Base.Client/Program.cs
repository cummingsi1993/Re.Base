using Re.Base.Client.Model;
using Re.Base.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

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


            var persons = StorageBuilder.CreateFileBasedStorage("D:\\Temp\\ReBase")
                .GetDbSet<Person>();

            //await SetupSomeData(persons);

			var people = (from Person person in persons
						  where person.Tags.Contains("ipsum")
						  select person.Tags).ToList();

            var peopleThatHaveIpsumTwice = (from Person person in persons
                                            where person.Tags.Count(x => x == "ipsum") > 1
                                            select person).ToList();

            var p = persons.Find(new Guid("932c9159-f8b7-42ce-8a18-3e6388ace0d3"));

            Console.ReadLine();
          

        }





        public static async Task SetupSomeData(DbSet<Person> persons)
        {
			string sample_data = System.IO.File.ReadAllText("D:\\Temp\\ReBase\\sample_people.json");

			var people = Newtonsoft.Json.JsonConvert.DeserializeObject<Person[]>(sample_data);

			foreach (Person person in people)
			{
				persons.Add(person);
			}

		}
	}
}

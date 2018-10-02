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

            var name = "Isaac";

            persons.Add(new Person() { FirstName = "Michael", LastName = "Jordan", DateOfBirth = DateTime.Now });

            var stuff = (from Person person in persons
                         where person.LastName.StartsWith("Cummings")
                         select new
                         {
                             person.FirstName,
                             person.LastName,
                             CurrentDate = DateTime.Now
                         }).ToList();

            var otherStuff = (from Person person in persons
                              select person).ToList();

            //var stuff = persons.Where(p => p.FirstName == name).ToList();

            foreach(var p in stuff)
            {
                Console.WriteLine(p.FirstName);
            }




            Console.ReadLine();
          

        }





        public static async Task SetupSomeData(FileDatabase database)
        {

        }
    }
}

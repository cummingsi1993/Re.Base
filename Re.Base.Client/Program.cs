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

            DataAccess.DAL dal = new DataAccess.DAL();

            var orderItem1 = dal.OrderItems.ToArray();

            var orderItem5 = dal.OrderItems.Where(o => o.Id == 5).ToArray();


            dal.Orders.Add(new Order() { Id = 1 });

            dal.OrderItems.Add(new OrderItem() { Id = 1, Quantity = 2, UnitPrice = 3 });
            dal.OrderItems.Add(new OrderItem() { Id = 2, Quantity = 5, UnitPrice = 65 });
            dal.OrderItems.Add(new OrderItem() { Id = 3, Quantity = 2, UnitPrice = 23 });
            dal.OrderItems.Add(new OrderItem() { Id = 4, Quantity = 22, UnitPrice = 7 });
            dal.OrderItems.Add(new OrderItem() { Id = 5, Quantity = 1, UnitPrice = 2 });
            dal.OrderItems.Add(new OrderItem() { Id = 6, Quantity = 23, UnitPrice = 6 });
            dal.OrderItems.Add(new OrderItem() { Id = 7, Quantity = 7, UnitPrice = 1 });
            dal.OrderItems.Add(new OrderItem() { Id = 8, Quantity = 6, UnitPrice = 2 });
            dal.OrderItems.Add(new OrderItem() { Id = 9, Quantity = 28, UnitPrice = 4 });
            //dal.OrderItems.Add(new OrderItem() { Id = 10, Quantity = 250, UnitPrice = 1 });

        }

	}
}

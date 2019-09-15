using Re.Base.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Client.DataAccess
{
    class DAL
    {

        StorageBuilder _storage;

        public DAL()
        {
            _storage = StorageBuilder.CreateFileBasedStorage(@"C:\Temp\ReBase")
                .MapSchema<Model.Person>()
                .MapSchema<Model.Order>()
                .MapSchema<Model.OrderItem>();

            People = _storage.GetDbSet<Model.Person>();
            Orders = _storage.GetDbSet<Model.Order>();
            OrderItems = _storage.GetDbSet<Model.OrderItem>();
        }

        public DbSet<Model.Person> People { get; private set; }

        public DbSet<Model.Order> Orders { get; private set; }

        public DbSet<Model.OrderItem> OrderItems { get; private set; }

    }
}

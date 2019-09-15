using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Re.Base.Data;
using Re.Base.Mapping.Logic;
using Re.Base.Queryables.File;
using Re.Base.Storage;

namespace Re.Base
{
    public class StorageBuilder
    {
        string fileLocation;

        Dictionary<Type, DataStore> _stores;

        public static StorageBuilder CreateFileBasedStorage(string fileLocation)
        {
            return new StorageBuilder(fileLocation);
        }

        public StorageBuilder(string fileLocation)
        {
            this.fileLocation = fileLocation;
            _stores = new Dictionary<Type, DataStore>();
        }

        private bool StoreExists<TModel>()
        {
            return _stores.ContainsKey(typeof(TModel));
        }

        private DataStore GetCreateStore<TModel>()
        {
            if (StoreExists<TModel>())
            {
                return _stores[typeof(TModel)];
            }
            else
            {
                DataStore store = new DataStore(fileLocation, typeof(TModel).Name);
                _stores.Add(typeof(TModel), store);
                return store;
            }
        }

        public StorageBuilder MapSchema<TModel>()
            where TModel : new()
        {
            SchemaMapper mapper = new SchemaMapper();
            DataStore store = GetCreateStore<TModel>();
            if (store.GetSchema().Fields.Count() == 0)
            {
                mapper.AddTableTypeToSchema<TModel>(store);
            }
            else
            {
                //Merge TODO:
            }

            return this;
        }

        public DbSet<TModel> GetDbSet<TModel>()
            where TModel : class
        {
            DataStore store = GetCreateStore<TModel>();
            return new DbSet<TModel>(store);
        }

    }

    




}

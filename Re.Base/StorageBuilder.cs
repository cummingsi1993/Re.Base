using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Re.Base.Queryables.File;
using Re.Base.Storage;

namespace Re.Base
{
    public class StorageBuilder
    {
        string fileLocation;
        public static StorageBuilder CreateFileBasedStorage(string fileLocation)
        {
            return new StorageBuilder(fileLocation);
        }

        public StorageBuilder(string fileLocation)
        {
            this.fileLocation = fileLocation;
        }

        public DbSet<TModel> GetDbSet<TModel>()
            where TModel : class
        {
            return new DbSet<TModel>(fileLocation);
        }

    }

    




}

using Re.Base.Data;
using Re.Base.Mapping.Logic;
using Re.Base.Queryables.Data;
using Re.Base.Queryables.File;
using Re.Base.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace Re.Base.Storage
{
    public class DbSet<TModel> : DataQueryable<TModel>
        where TModel : class
    {
        private DataStore _store;
        

        public DbSet(DataQueryProvider provider, Expression expression) : base(provider, expression)
        {
        }

        public DbSet(DataStore store) : base(store)
        {
            _store = store;
        }

		public void AddRange(IEnumerable<TModel> models)
		{
            var mapper = new RecordMapper(_store.GetSchema());

			foreach (TModel model in models)
			{
                _store.InsertRecord(mapper.MapToFields(model));
            }
		}

        public void Add(TModel model)
        {
            var mapper = new RecordMapper(_store.GetSchema());
            _store.InsertRecord(mapper.MapToFields(model));
        }


        

    }
}

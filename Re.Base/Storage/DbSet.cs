using Re.Base.Logic;
using Re.Base.Models;
using Re.Base.Queryables.File;
using Re.Base.Readers;
using Re.Base.Writers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace Re.Base.Storage
{
    public class DbSet<TModel> : FileQueryable<TModel>
        where TModel : class
    {
        private string databaseLocation;
        private Index<TModel> index;

        public DbSet(FileQueryProvider provider, Expression expression) : base(provider, expression)
        {
            this.index = new Index<TModel>();
        }

        public DbSet(string databaseLocation) : base(databaseLocation)
        {
            this.databaseLocation = databaseLocation;
            this.index = new Index<TModel>();
        }

        public TModel Find(Guid recordKey)
        {
            var query = new Queryables.RebaseQuery();
            query.AddSource(typeof(TModel));

            FileReader<TModel> reader = new FileReader<TModel>(databaseLocation, query);
            return reader.FindById(recordKey);
        }

        public TModel Find(long recordKey)
        {
            var query = new Queryables.RebaseQuery();
            query.AddSource(typeof(TModel));

            FileReader<TModel> reader = new FileReader<TModel>(databaseLocation, query);
            return reader.FindById(recordKey);
        }

		public void AddRange(IEnumerable<TModel> models)
		{
			FileWriter<TModel> writer = new FileWriter<TModel>(databaseLocation);

			List<string> serializedModels = new List<string>();

			foreach (TModel model in models)
			{
                writer.WriteNewModel(model);
			}

		}

        public void Add(TModel model)
        {
            string sourceName = typeof(TModel).Name;
            string fileName = $"{databaseLocation}/data_{sourceName}.rbs";

            DbRecord<TModel> record = new DbRecord<TModel>(model);

			string serializedModel = Newtonsoft.Json.JsonConvert.SerializeObject(record);

            if (!System.IO.File.Exists(fileName))
            {
                System.IO.File.Create(fileName).Close();
            }

            System.IO.File.AppendAllLines(fileName, new string[] { serializedModel });

        }


        

    }
}

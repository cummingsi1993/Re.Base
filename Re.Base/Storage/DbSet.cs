using Re.Base.Models;
using Re.Base.Queryables.File;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Re.Base.Storage
{
    public class DbSet<TModel> : FileQueryable<TModel>
        where TModel : class
    {
        private string databaseLocation;
        public DbSet(FileQueryProvider provider, Expression expression) : base(provider, expression)
        {
        }

        public DbSet(string databaseLocation) : base(databaseLocation)
        {
            this.databaseLocation = databaseLocation;
        }

		public void AddRange(IEnumerable<TModel> models)
		{
			string sourceName = typeof(TModel).Name;
			string fileName = $"{databaseLocation}/data_{sourceName}.rbs";

			List<string> serializedModels = new List<string>();

			foreach (TModel model in models)
			{
				DbRecord<TModel> record = new DbRecord<TModel>(model);

				string serializedModel = Newtonsoft.Json.JsonConvert.SerializeObject(record);
				serializedModels.Add(serializedModel);
			}

			if (!System.IO.File.Exists(fileName))
			{
				System.IO.File.Create(fileName).Close();
			}

			System.IO.File.AppendAllLines(fileName, serializedModels);
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

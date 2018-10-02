using Re.Base.Models;
using Re.Base.Queryables;
using Re.Base.Queryables.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Re.Base.Storage
{
    public class FileDatabase
    {
        private string fileLocation;
        public FileDatabase(string fileLocation)
        {
            this.fileLocation = fileLocation;
        }


        public async Task<IQueryable<TModel>> Get<TModel>()
        {

            throw new NotImplementedException();


        }

        public async Task Save<TModel>(TModel model)
            where TModel : class
        {

            int hash = typeof(TModel).GetHashCode();
            string filelocation = $"{fileLocation}/data_{hash}.rbs";

            DbRecord<TModel> record = new DbRecord<TModel>(model);

            //string serializedModel = Json.JsonParser.Serialize(record);
            string serializedModel = Newtonsoft.Json.JsonConvert.SerializeObject(record);
            //serializer.Serialize()

            //byte[] serializedBytes = Encoding.ASCII.GetBytes(serializedModel);
            

            if (!System.IO.File.Exists(filelocation))
            {
                System.IO.File.Create(filelocation).Close();
            }

            System.IO.File.AppendAllLines(filelocation, new string[] { serializedModel });
        }

        //public IQueryable<TModel> DbSet<TModel>()
        //{
        //    return new FileQueryable<TModel>();
        //}


        public async Task<IEnumerable<TModel>> Where<TModel>(Func<TModel, Boolean> filter)
        {
            int hash = typeof(TModel).GetHashCode();
            string filelocation = $"{fileLocation}/data_{hash}.rbs";

            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

            List<TModel> models = new List<TModel>();

            using (System.IO.StreamReader stream = System.IO.File.OpenText(filelocation))
            {
                Newtonsoft.Json.JsonTextReader reader = new Newtonsoft.Json.JsonTextReader(stream);
                reader.SupportMultipleContent = true;

                while (reader.Read())
                {
                    if (reader.Value?.ToString() == "Id")
                    {
                        var id = reader.ReadAsString();

                        while (reader.Read())
                        {
                            if (reader.Value?.ToString() == "Model")
                            {
                                reader.Read();

                                var model = serializer.Deserialize<TModel>(reader);
                                if (filter(model))
                                {
                                    models.Add(model);
                                }
                            }
                        }

                    }
                    else
                    {
                        //reader.Skip();
                    }

                }

                return models;
            }
        }
        
        public TModel Search<TModel>(Expression expression)
        {
            int hash = typeof(TModel).GetHashCode();
            string filelocation = $"{fileLocation}/data_{hash}.rbs";

            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

            List<TModel> models = new List<TModel>();

            using (System.IO.StreamReader stream = System.IO.File.OpenText(filelocation))
            {
                Newtonsoft.Json.JsonTextReader reader = new Newtonsoft.Json.JsonTextReader(stream);
                reader.SupportMultipleContent = true;

                while (reader.Read())
                {
                    if (reader.Value?.ToString() == "Id")
                    {
                        var id = reader.ReadAsString();
                        
                        while (reader.Read())
                        {
                            if (reader.Value?.ToString() == "Model")
                            {
                                reader.Read();

                                var model = serializer.Deserialize<TModel>(reader);
                                models.Add(model);
                                
                            }
                        }
                        
                    }
                    else
                    {
                        //reader.Skip();
                    }

                }

                return default(TModel); //Not found
            }

        }

        public async Task<TModel> Find<TModel>(Guid key)
            where TModel : class
        {

            int hash = typeof(TModel).GetHashCode();
            string filelocation = $"{fileLocation}/data_{hash}.rbs";

            string keyString = key.ToString();

            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

            using (System.IO.StreamReader stream = System.IO.File.OpenText(filelocation))
            {
                Newtonsoft.Json.JsonTextReader reader = new Newtonsoft.Json.JsonTextReader(stream);
                reader.SupportMultipleContent = true;

                TModel model = null;

                while (reader.Read())
                {
                    
                    if (reader.Value?.ToString() == "Id")
                    {
                        var id = reader.ReadAsString();
                        if (id == keyString)
                        {
                            while (reader.Read())
                            {
                                if (reader.Value?.ToString() == "Model")
                                {
                                    reader.Read();

                                    model = serializer.Deserialize<TModel>(reader);
                                    reader.Close();
                                    return model;
                                }
                            }
                        }
                        else
                        {
                            reader.Skip();                            
                        }
                    }
                    else
                    {
                        //reader.Skip();
                    }

                }

                return null; //Not found
            }
        }

        internal IEnumerable<object> Query(Type type, RebaseQuery query)
        {
            
            int hash = type.GetHashCode();
            string filelocation = $"{fileLocation}/data_{hash}.rbs";

            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

            List<object> models = new List<object>();

            using (System.IO.StreamReader stream = System.IO.File.OpenText(filelocation))
            {
                Newtonsoft.Json.JsonTextReader reader = new Newtonsoft.Json.JsonTextReader(stream);
                reader.SupportMultipleContent = true;

                while (reader.Read())
                {
                    if (reader.Value?.ToString() == "Id")
                    {
                        var id = reader.ReadAsString();

                        while (reader.Read())
                        {
                            if (reader.Value?.ToString() == "Model")
                            {
                                reader.Read();

                                var model = serializer.Deserialize(reader, type);

                                if (query.ApplyFilter(model))
                                {
                                    models.Add(model);
                                }
                            }
                        }

                    }
                    else
                    {
                        //reader.Skip();
                    }

                }

                return models;
            }
        }

        internal IEnumerable<TModel> Query<TModel>(RebaseQuery query)
        {
            int hash = typeof(TModel).GetHashCode();
            string filelocation = $"{fileLocation}/data_{hash}.rbs";

            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

            List<TModel> models = new List<TModel>();

            using (System.IO.StreamReader stream = System.IO.File.OpenText(filelocation))
            {
                Newtonsoft.Json.JsonTextReader reader = new Newtonsoft.Json.JsonTextReader(stream);
                reader.SupportMultipleContent = true;

                while (reader.Read())
                {
                    if (reader.Value?.ToString() == "Id")
                    {
                        var id = reader.ReadAsString();

                        while (reader.Read())
                        {
                            if (reader.Value?.ToString() == "Model")
                            {
                                reader.Read();

                                var model = serializer.Deserialize<TModel>(reader);

                                if (query.ApplyFilter(model))
                                {
                                    models.Add(model);
                                }
                            }
                        }

                    }
                    else
                    {
                        //reader.Skip();
                    }

                }

                return models;
            }
        }


    }
}

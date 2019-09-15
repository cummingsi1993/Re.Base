using Re.Base.Data;
using Re.Base.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Re.Base.Mapping.Logic
{
    public class SchemaMapper
    {

        public void MergeColumns<TModel>(DataStore store)
            where TModel : new()
        {
            var modelSchema = Helpers.GetModelSchema<TModel>();
            var storedSchema = store.GetSchema();

            foreach (Models.FieldSchema field in modelSchema.Fields)
            {
                var storedField = storedSchema.Fields.FirstOrDefault(f => f.FieldName == field.Name);

                if(storedField == null)
                {
                    FieldDefinition newField = new FieldDefinition()
                    {
                        FieldName = field.Name,
                        DataType = field.DataType,
                        Nullable = field.Nullable
                    };

                    store.AddField(newField);
                }
                else
                {
                    //maybe i need to make a change? 

                }
            }


        }

        public void AddTableTypeToSchema<TModel>(DataStore store)
            where TModel : new()
        {
            var type = typeof(TModel);

            var schemaFields = new List<Tuple<int, FieldDefinition>>();
            var indexes = new List<IndexDefinition>();


            var index = 0;

            foreach (var prop in type.GetProperties())
            {
                var fieldAttribute =(Attributes.FieldAttribute)System.Attribute.GetCustomAttributes(prop).FirstOrDefault(a => a is Attributes.FieldAttribute);
                var indexAttribute = (Attributes.IndexAttribute)System.Attribute.GetCustomAttributes(prop).FirstOrDefault(a => a is Attributes.IndexAttribute);

                if (fieldAttribute == null) continue;

                
                DataType fieldType = fieldAttribute.GetDataType();
                Boolean nullable = fieldAttribute.GetNullable();

                schemaFields.Add(new Tuple<int, FieldDefinition>(index, new Data.Models.FieldDefinition() { DataType = fieldType, FieldName = prop.Name, Nullable = nullable }));

                if (indexAttribute != null)
                {
                    indexes.Add(new IndexDefinition() { FieldId = index, IndexName = $"{type.Name}_{prop.Name}", IndexType = Indexing.Enums.IndexType.InMemoryIndex });
                }
                

                index++;
            }

            foreach(var field in schemaFields.OrderBy(x => x.Item1))
            {
                store.AddField(field.Item2);
            }

            foreach (var i in indexes)
            {
                store.AddIndex(i.FieldId, i.IndexName, i.IndexType);
            }
                
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Re.Base.Mapping.Logic
{
    public class RecordMapper
    {
        Re.Base.Data.Models.DataStructure _schema;
        public RecordMapper(Re.Base.Data.Models.DataStructure schema)
        {
            _schema = schema;
        }
        

        public object[] MapToFields<TModel>(TModel model)
        {
            Type type = model.GetType();

            List<object> fields = new List<object>();
            var schema = Helpers.GetModelSchema<TModel>();

            foreach (var fieldSchema in _schema.Fields)
            {
                var fieldModelSchema = schema.Fields.FirstOrDefault(f => f.Name == fieldSchema.FieldName);

                fields.Add(fieldModelSchema.GetValue(model));

            }
            return fields.ToArray();
        }

        public TModel MapToModel<TModel>(object[] fields)
            where TModel : new()
        {
            var model = new TModel();
            var schema = Helpers.GetModelSchema<TModel>();

            if (fields.Length != schema.Fields.Count)
                throw new Exception();

            for (int i = 0; i < _schema.Fields.Count; i++)
            {
                var fieldvalue = fields[i];
                var fieldSchema = _schema.Fields[i];
                var fieldModelSchema = schema.Fields.FirstOrDefault(f => f.Name == fieldSchema.FieldName);

                fieldModelSchema.SetValue(model, fieldvalue);

            }

            return model; 
        }

    }
}

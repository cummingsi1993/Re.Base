using System;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Mapping
{
    internal static class Helpers
    {

        public static Models.ModelSchema GetModelSchema<TModel>()
        {
            var schema = new Models.ModelSchema();
            Type type = typeof(TModel);

            var props = type.GetProperties();


            foreach (var prop in props)
            {
                var attr = (Attributes.FieldAttribute)Attribute.GetCustomAttribute(prop, typeof(Attributes.FieldAttribute));
                if (attr == null) { continue; }

                Models.FieldSchema field = new Models.FieldSchema(
                    model => prop.GetValue(model),
                    (model, value) => prop.SetValue(model, value)
                );

                field.Nullable = attr.GetNullable();
                field.Name = attr.GetFieldName() ?? prop.Name;
                field.DataType = attr.GetDataType();

                schema.Fields.Add(field);
            }

            return schema;
        }

    }
}

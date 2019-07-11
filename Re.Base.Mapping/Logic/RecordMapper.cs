using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Re.Base.Mapping.Logic
{
    public class RecordMapper
    {

        public object[] MapToFields<TModel>(TModel model)
        {

            Type type = model.GetType();

            return type.GetProperties()
                .Where(prop => prop.CustomAttributes
                    .Any(attr => attr.AttributeType == typeof(Attributes.FieldAttribute)))
                .Select(prop => prop.GetValue(model))
                .ToArray();
        }

        public TModel MapToModel<TModel>(object[] fields)
            where TModel : new()
        {
            return new TModel();
        }

    }
}

using Re.Base.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Mapping.Models
{
    public class FieldSchema
    {

        private Func<object, object> _get;
        private Action<object, object> _set;

        public FieldSchema(Func<object, object> get, Action<object, object> set)
        {
            _get = get;
            _set = set;
        }

        public bool Nullable { get; set; }

        public DataType DataType { get; set; }

        public bool Indexed { get; set; }

        public string Name { get; set; }

        public object GetValue(object model)
        {
            return _get(model);
        }

        public void SetValue(object model, object value)
        {
            _set(model, value);
        }

    }
}

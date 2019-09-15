using System;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Mapping.Models
{
    public class ModelSchema
    {
        public ModelSchema()
        {
            Fields = new List<FieldSchema>();
        }

        public List<FieldSchema> Fields { get; set; }



    }
}

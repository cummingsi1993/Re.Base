using System;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Models
{
    public class DbRecord<TModel>
        where TModel : class
    {

        public DbRecord(TModel model)
        {
            this.Model = model;
            this.Id = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }

        public TModel Model { get; set; }
    }
}

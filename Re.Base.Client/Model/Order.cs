using Re.Base.Mapping.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Client.Model
{
    public class Order
    {
        [Field(Data.Models.DataType.Int32)]
        public int Id { get; set; }

        public List<OrderItem> OrderItems {get; set;}

    }
}

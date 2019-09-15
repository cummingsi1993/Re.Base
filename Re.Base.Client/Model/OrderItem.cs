using Re.Base.Mapping.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Client.Model
{
    public class OrderItem
    {
        
        [Index]
        [Field(Data.Models.DataType.Int32)]
        public int Id { get; set; }

        public Order Order { get; set; }

        [Field(Data.Models.DataType.Int32)]
        public int UnitPrice { get; set; }

        [Field(Data.Models.DataType.Int32)]
        public int Quantity { get; set; }

    }
}

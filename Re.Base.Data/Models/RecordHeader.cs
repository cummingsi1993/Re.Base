using System;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Data.Models
{
    public class RecordHeader
    {

        public RecordHeader()
        {
        }

        public Guid Id { get; set; }
        public int BytesInRecord { get; set; }
        public bool Ghosted { get; set; }

        
    }
}

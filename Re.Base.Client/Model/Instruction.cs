using System;
using System.Collections.Generic;
using System.Text;
using Re.Base.Mapping.Attributes;

namespace Re.Base.Client.Model
{
    public class Instruction
    {
        [Field]
        public int Id { get; set; }

        [Field]
        public bool Test { get; set; }

        [Field]
        public DateTime? Date_Of_Birth { get; set; }

        [Field]
        public string Sex { get; set; }

        [Field]
        public string Barcode { get; set; }

        [Field]
        public string Lab_Location { get; set; }

        [Field]
        public DateTime Time_Created { get; set; }

        [Field]
        public DateTime Time_Updated { get; set; }

    }
}

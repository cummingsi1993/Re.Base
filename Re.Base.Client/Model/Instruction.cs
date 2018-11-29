using System;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Client.Model
{
    public class Instruction
    {

        public int Id { get; set; }

        public bool Test { get; set; }

        public DateTime? Date_Of_Birth { get; set; }

        public string Sex { get; set; }

        public string Barcode { get; set; }

        public string Lab_Location { get; set; }

        public DateTime Time_Created { get; set; }

        public DateTime Time_Updated { get; set; }

    }
}

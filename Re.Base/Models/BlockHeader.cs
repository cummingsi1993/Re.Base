using System;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Models
{
    public class BlockHeader
    {

        public long BlockSequence { get; set; }
        public long RecordCount { get; set; }
        public long FreeBytes { get; set; }
		public bool BlockFragmented { get; set; }



    }
}

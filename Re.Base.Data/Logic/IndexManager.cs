using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Re.Base.Data.Logic
{
    public class IndexManager
    {
		private FileStream _stream;

		public IndexManager(FileStream indexStream)
		{
			_stream = indexStream;
		}

    }
}

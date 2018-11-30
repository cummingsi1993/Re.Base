using System;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Data.Constants
{
    public class Lengths
    {
        public const int FileHeaderLength = 1024;
        public const int BlockHeaderLength = 128;
        public const int RecordHeaderLength = 128;
        public const int BlockLength = 8000;

        public const int DateTimeLength = 8;
        public const int LittleStringLength = 100;
        public const int BigStringLength = 1000;

    }
}

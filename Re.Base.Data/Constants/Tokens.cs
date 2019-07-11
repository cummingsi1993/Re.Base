using System;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Data.Constants
{
    public class Tokens
    {

        public const byte DateTimeTypeToken = 0x001A;
        public const byte Int64TypeToken = 0x001B;
        public const byte Int32TypeToken = 0x001C;
        public const byte Int16TypeToken = 0x001D;
        public const byte DecimalTypeToken = 0x001E;
        public const byte BigStringTypeToken = 0x001F;
        public const byte LittleStringTypeToken = 0x002A;
        public const byte BooleanTypeToken = 0x002B;


        public const byte FileBeginToken = 0x000A;
        public const byte BlockBeginToken = 0x000B;
        public const byte RecordBeginToken = 0x000C;
        public const byte SchemaSegmentSeperationToken = 0x00D;

    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Indexing.Creation
{
    public static class IndexFactory
    {

        public static IIndex<byte[]> CreateIndex(Enums.IndexType type)
        {
            switch(type)
            {
                case Enums.IndexType.InMemoryIndex: return new Indexes.InMemoryIndex();
                default: throw new Exception();
            }
        }

    }
}

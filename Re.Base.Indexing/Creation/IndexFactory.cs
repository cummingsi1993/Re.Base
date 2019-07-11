using System;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Indexing.Creation
{
    public static class IndexFactory
    {

        public static IIndex<TKey> CreateIndex<TKey>(Enums.IndexType type)
            where TKey : IComparable
        {
            switch(type)
            {
                case Enums.IndexType.InMemoryIndex: return new Indexes.InMemoryIndex<TKey>();
                default: throw new Exception();
            }
        }

    }
}

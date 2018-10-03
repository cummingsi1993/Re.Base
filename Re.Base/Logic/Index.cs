using System;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Logic
{
    public class Index<TModel>
    {
        Dictionary<Guid, TModel> primaryKeyIndex;
        Dictionary<Guid, long> primaryByteLocator;
        public Index()
        {
            primaryKeyIndex = new Dictionary<Guid, TModel>();
            primaryByteLocator = new Dictionary<Guid, long>();
        }

        public bool KeyExistsInIndex(Guid key)
        {
            return primaryKeyIndex.ContainsKey(key);
        }
        

    }
}

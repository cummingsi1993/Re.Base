using System;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Indexing.Exceptions
{
    public class DuplicateKeyException<TKey> : Exception
    {
        public TKey Key { get; }

        public DuplicateKeyException(TKey key)
            : base($"They key: ${key} already exists in the index.")
        {
            Key = key;
        }
    }
}
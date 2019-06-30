using System;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Indexing.Exceptions
{
    public class KeyNotFoundException<TKey> : Exception
    {
        public TKey Key { get; }

        public KeyNotFoundException(TKey key)
            : base($"They key: ${key} was expected to be in the index but was not found.")
        {
            Key = key;
        }

    }
}

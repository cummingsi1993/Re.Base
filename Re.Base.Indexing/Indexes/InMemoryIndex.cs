using Re.Base.Indexing.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Re.Base.Indexing.Indexes
{
    public class InMemoryIndex<TKey> : IIndex<TKey>
    {
        private Dictionary<TKey, long> _index;

        #region Private Methods 

        /// <summary>
        /// Throws an exception if the key already exists in the index.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private async Task CheckDuplicateKey(TKey key)
        {
            if (await KeyExists(key))
            {
                throw new KeyNotFoundException<TKey>(key);
            }
        }


        /// <summary>
        /// Throws an exception if the key does not exist in the index.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private async Task CheckKeyExists(TKey key)
        {
            if (!await KeyExists(key))
            {
                throw new KeyNotFoundException<TKey>(key);
            }
        }

        #endregion

        public InMemoryIndex()
        {
            _index = new Dictionary<TKey, long>();

        }

        public async Task DeleteRecord(TKey key)
        {
            await CheckKeyExists(key);

            _index.Remove(key);
        }

        public async Task<long> GetRecordLocation(TKey key)
        {
            await CheckKeyExists(key);

            return _index[key];
        }

        public async Task InsertRecord(TKey key, long location)
        {
            await CheckDuplicateKey(key);

            _index.Add(key, location);
        }

        public async Task<bool> KeyExists(TKey key)
        {
            return _index.ContainsKey(key);
        }

        public async Task ReassignKey(TKey oldKey, TKey newKey)
        {
            await CheckDuplicateKey(newKey);
            await CheckKeyExists(oldKey);

        }
    }

    public class InMemoryIndex : InMemoryIndex<byte[]>
    {



    }

}

using Re.Base.Indexing.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Re.Base.Indexing.Indexes
{
    public class InMemoryIndex<TKey> : IIndex<TKey>
        where TKey : IComparable
    {
        private SortedDictionary<TKey, long> _index;

        #region Private Methods 

        /// <summary>
        /// Throws an exception if the key already exists in the index.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private void CheckDuplicateKey(TKey key)
        {
            if (KeyExists(key))
            {
                throw new KeyNotFoundException<TKey>(key);
            }
        }


        /// <summary>
        /// Throws an exception if the key does not exist in the index.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private void CheckKeyExists(TKey key)
        {
            if (!KeyExists(key))
            {
                throw new KeyNotFoundException<TKey>(key);
            }
        }

        #endregion

        public InMemoryIndex()
        {
            _index = new SortedDictionary<TKey, long>();

        }

        public void DeleteRecord(TKey key)
        {
            CheckKeyExists(key);

            _index.Remove(key);
        }

        public long GetRecordLocation(TKey key)
        {
            CheckKeyExists(key);

            return _index[key];
        }

        public long CountRecords()
        {
            return _index.Count;
        }

        public long[] GetRecordsAfter(TKey key)
        {
            //Super slow, needs refactor.
            //We should be able to leverage the fact that the underlying structure of a sorted dictionary is a b-tree.
            TKey[] keysAfter = _index.Keys
                .Where(x => x.CompareTo(key) >= 0)
                .ToArray();

            long[] results = new long[keysAfter.Length];

            for (int i = 0; i < keysAfter.Length; i++)
            {
                results[i] = _index[keysAfter[i]];
            }

            return results;
        }

        public long[] GetRecordsBefore(TKey key)
        {
            //Super slow, needs refactor.
            //We should be able to leverage the fact that the underlying structure of a sorted dictionary is a b-tree.
            TKey[] keysAfter = _index.Keys
                .Where(x => x.CompareTo(key) <= 0)
                .ToArray();

            long[] results = new long[keysAfter.Length];

            for (int i = 0; i < keysAfter.Length; i++)
            {
                results[i] = _index[keysAfter[i]];
            }

            return results;
        }

        public long[] GetRecordsBetween(TKey key1, TKey key2)
        {
            //Super slow, needs refactor.
            //We should be able to leverage the fact that the underlying structure of a sorted dictionary is a b-tree.
            TKey[] keysAfter = _index.Keys
                .Where(x => x.CompareTo(key1) >= 0 && x.CompareTo(key2) <= 0)
                .ToArray();

            long[] results = new long[keysAfter.Length];

            for (int i = 0; i < keysAfter.Length; i++)
            {
                results[i] = _index[keysAfter[i]];
            }

            return results;
        }

        public void InsertRecord(TKey key, long location)
        {
            CheckDuplicateKey(key);

            _index.Add(key, location);
        }

        public bool KeyExists(TKey key)
        {
            return _index.ContainsKey(key);
        }

        public void ReassignKey(TKey oldKey, TKey newKey)
        {
            CheckDuplicateKey(newKey);
            CheckKeyExists(oldKey);

        }
    }

}

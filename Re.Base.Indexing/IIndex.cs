using System;
using System.Threading.Tasks;

namespace Re.Base.Indexing
{
    public interface IIndex<TKey>
    {
        void InsertRecord(TKey key, Int64 location);

        void DeleteRecord(TKey key);

        void ReassignKey(TKey oldKey, TKey newKey);

        long CountRecords();

        Int64 GetRecordLocation(TKey key);

        Int64[] GetRecordsAfter(TKey key);

        Int64[] GetRecordsBefore(TKey key);

        Int64[] GetRecordsBetween(TKey key1, TKey key2);

        bool KeyExists(TKey key);

    }
}

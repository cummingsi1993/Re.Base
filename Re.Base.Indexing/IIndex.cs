using System;
using System.Threading.Tasks;

namespace Re.Base.Indexing
{
    public interface IIndex<TKey>
    {
        Task InsertRecord(TKey key, Int64 location);

        Task DeleteRecord(TKey key);

        Task ReassignKey(TKey oldKey, TKey newKey);

        Task<Int64> GetRecordLocation(TKey key);

        Task<bool> KeyExists(TKey key);

    }
}

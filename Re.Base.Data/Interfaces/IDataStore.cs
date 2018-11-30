using Re.Base.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Data.Interfaces
{
    public interface IDataStore
    {
        #region Schema
        void AddField(FieldDefinition field);
        void ModifyField(string fieldName, FieldDefinition field);
        void RemoveField(string fieldName);

        DataStructure GetSchema();
        #endregion

        #region Data

        void InsertRecord(params object[] fields);

        Record ReadRecord(long index);

        Record[] Query(Func<Record, bool> query);

        #endregion

    }
}

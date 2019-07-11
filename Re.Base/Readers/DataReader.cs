using Re.Base.Data;
using Re.Base.Mapping.Logic;
using Re.Base.Queryables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Readers
{
    internal class DataReader<TModel> : IEnumerable<TModel>, IEnumerable
        where TModel : new()
    {
        RebaseQuery _query;
        DataStore _store;

        public DataReader(DataStore store, RebaseQuery query)
        {
            _store = store;
            _query = query;
        }

        public IEnumerator<TModel> GetEnumerator()
        {
            return new Enumerator(_store);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(_store);
        }

        class Enumerator : IEnumerator<TModel>, IEnumerator, IDisposable
        {
            private DataStore _store;
            private int _currentIndex = 0;
            private RecordMapper _mapper;

            public Enumerator(DataStore store)
            {
                _store = store;
                _mapper = new RecordMapper();
            }

            public TModel Current => _mapper.MapToModel<TModel>(_store.ReadRecord(_currentIndex).Fields);

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                _currentIndex++;
                return true;
            }

            public void Reset()
            {
                _currentIndex = 0;
            }

            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: dispose managed state (managed objects).
                    }

                    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                    // TODO: set large fields to null.

                    disposedValue = true;
                }
            }

            // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
            // ~Enumerator() {
            //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            //   Dispose(false);
            // }

            // This code added to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
                // TODO: uncomment the following line if the finalizer is overridden above.
                // GC.SuppressFinalize(this);
            }
            #endregion
        }
    }
}

using Re.Base.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Readers
{
    internal class FileReader<TModel> : IEnumerable<TModel>, IEnumerable
    {

        //Maximum block size in bytes
        private long _maxBlockSize = 8000;
        
        private string fileName;
        Queryables.RebaseQuery query;

        public FileReader(string fileLocation, Queryables.RebaseQuery query)
        {
            string sourceName = query.GetSourceType().Name;
            fileName = $"{fileLocation}/data_{sourceName}.rbs";

            this.query = query;
        }

        IEnumerator<TModel> IEnumerable<TModel>.GetEnumerator()
        {
            return new Enumerator(fileName, query);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(fileName, query);
        }

        internal TModel FindById(long recordNumber)
        {

            System.IO.StreamReader stream = System.IO.File.OpenText(fileName);
            long lineNumber = 0;
            while (lineNumber < (recordNumber - 1))
            {
                stream.ReadLine();
                lineNumber++;
            }
            
            using (IEnumerator<TModel> enumerator = new Enumerator(stream, query))
            {
                enumerator.MoveNext();
                return enumerator.Current;
            }
            
        }

        internal TModel FindById(Guid recordId)
        {

            System.IO.StreamReader stream = System.IO.File.OpenText(fileName);

            using (Enumerator enumerator = new Enumerator(stream, query))
            {
                enumerator.MoveToBlock(0);
                if (enumerator.MoveToId(recordId))
                {
                    enumerator.MoveNext();   
                    return enumerator.Current;
                }
                else
                {
                    //TODO create exceptions
                    throw new InvalidOperationException();
                }
                
            }

        }

        class Enumerator : IEnumerator<TModel>, IEnumerator, IDisposable
        {
            Newtonsoft.Json.JsonSerializer serializer;
            System.IO.StreamReader stream;
            Newtonsoft.Json.JsonTextReader reader;
            Queryables.RebaseQuery query;

            BlockHeader currentBlock;
            FileHeader file;

            public Enumerator(System.IO.StreamReader stream, Queryables.RebaseQuery query)
            {
                this.stream = stream;
                serializer = new Newtonsoft.Json.JsonSerializer();

                reader = new Newtonsoft.Json.JsonTextReader(stream);
                reader.SupportMultipleContent = true;

                this.query = query;

                this.ReadFileHeader();
            }

            public Enumerator(string fileName, Queryables.RebaseQuery query)
            {
                stream = System.IO.File.OpenText(fileName);
                serializer = new Newtonsoft.Json.JsonSerializer();

                reader = new Newtonsoft.Json.JsonTextReader(stream);
                reader.SupportMultipleContent = true;

                this.query = query;

                this.ReadFileHeader();
            }

            private void ReadFileHeader()
            {
                byte[] header = new byte[1];
                byte[] blockCountBytes = new byte[8];

                stream.BaseStream.Read(header, 0, 1);
                stream.BaseStream.Read(blockCountBytes, 0, 8);

                //The file header is corrupt
                if (header[0] != 0x000A)
                {
                    throw new InvalidOperationException();
                }

                //if (BitConverter.IsLittleEndian)
                //{
                //    Array.Reverse(blockCountBytes);
                //}

                long blockCount = BitConverter.ToInt64(blockCountBytes, 0);
                file = new FileHeader() { BlocksInFile = blockCount };
            }

            public Guid? MoveToNextId()
            {
                bool shouldContinue = true;

                while (shouldContinue)
                {
                    shouldContinue = reader.Read();
                    
                    if (reader.Value?.ToString() == "Id")
                    {
                        var id = reader.ReadAsString();
                        return new Guid(id);
                    }
                }
                //A null id means we have reached EOF
                return null;
            }

            public bool MoveToId(Guid id)
            {
                Guid? currentId = MoveToNextId();
                while (currentId.HasValue)
                {
                    currentId = MoveToNextId();

                    if (currentId == id)
                    {
                        return true;
                    }
                }

                return false;
            }

            public TModel Current { get; private set; }

            object IEnumerator.Current { get { return Current; } }

            public bool MoveNext()
            {
                Guid? currentId = this.MoveToNextId();
                while (currentId.HasValue)
                {
                    reader.Read();
                    if (reader.Value?.ToString() == "Model")
                    {
                        reader.Read();

                        var model = serializer.Deserialize(reader, query.GetSourceType());

                        if (query.ApplyFilter(model))
                        {
                            this.Current = (TModel)query.ApplyProjection(model);
                            return true;
                        }
                    }


                    currentId = this.MoveToNextId();
                }

                return false;
            }

            void IEnumerator.Reset()
            {
                
            }

            public bool MoveToBlock(long blockSequence)
            {
                BlockHeader header = serializer.Deserialize<BlockHeader>(reader);
                

                //Read block
                return true;
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
                        stream.Close();
                        stream.Dispose();
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
            void IDisposable.Dispose()
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

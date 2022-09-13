using System;
using System.Runtime.Serialization;

namespace FastDataLoader
{
    public class DataLoaderTooManyRecordsException : DataLoaderException
    {
        public DataLoaderTooManyRecordsException()
        {
        }

        public DataLoaderTooManyRecordsException( string message ) : base( message )
        {
        }

        public DataLoaderTooManyRecordsException( string message, Exception innerException ) : base( message, innerException )
        {
        }

        protected DataLoaderTooManyRecordsException( SerializationInfo info, StreamingContext context ) : base( info, context )
        {
        }
    }
}

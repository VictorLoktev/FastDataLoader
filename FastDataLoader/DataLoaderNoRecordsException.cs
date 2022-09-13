using System;
using System.Runtime.Serialization;

namespace FastDataLoader
{
    public class DataLoaderNoRecordsException : DataLoaderException
    {
        public DataLoaderNoRecordsException()
        {
        }

        public DataLoaderNoRecordsException( string message ) : base( message )
        {
        }

        public DataLoaderNoRecordsException( string message, Exception innerException ) : base( message, innerException )
        {
        }

        protected DataLoaderNoRecordsException( SerializationInfo info, StreamingContext context ) : base( info, context )
        {
        }
    }
}

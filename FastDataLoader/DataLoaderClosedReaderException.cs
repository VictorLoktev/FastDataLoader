using System;
using System.Runtime.Serialization;

namespace FastDataLoader
{
    public class DataLoaderClosedReaderException : DataLoaderException
    {
        public DataLoaderClosedReaderException()
        {
        }

        public DataLoaderClosedReaderException( string message ) : base( message )
        {
        }

        public DataLoaderClosedReaderException( string message, Exception innerException ) : base( message, innerException )
        {
        }

        protected DataLoaderClosedReaderException( SerializationInfo info, StreamingContext context ) : base( info, context )
        {
        }
    }
}

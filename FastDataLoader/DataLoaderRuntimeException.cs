using System;
using System.Runtime.Serialization;

namespace FastDataLoader
{
    public class DataLoaderRuntimeException : DataLoaderException
    {
        public DataLoaderRuntimeException()
        {
        }

        public DataLoaderRuntimeException( string message ) : base( message )
        {
        }

        public DataLoaderRuntimeException( string message, Exception innerException ) : base( message, innerException )
        {
        }

        protected DataLoaderRuntimeException( SerializationInfo info, StreamingContext context ) : base( info, context )
        {
        }
    }
}

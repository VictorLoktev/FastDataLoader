using System;
using System.Runtime.Serialization;

namespace FastDataLoader
{
    public class DataLoaderMetadataException: DataLoaderException
    {
        public DataLoaderMetadataException()
        {
        }

        public DataLoaderMetadataException( string message ) : base( message )
        {
        }

        public DataLoaderMetadataException( string message, Exception innerException ) : base( message, innerException )
        {
        }

        protected DataLoaderMetadataException( SerializationInfo info, StreamingContext context ) : base( info, context )
        {
        }
    }
}

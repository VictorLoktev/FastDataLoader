using System;
using System.Runtime.Serialization;

namespace FastDataLoader
{
    /// <summary>
    /// <para>Исключение кидается, когда обнаруживается проблема
    /// с построением соответствия между метаданными IDataReader
    /// и заполняемым типом.</para>
    /// </summary>
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

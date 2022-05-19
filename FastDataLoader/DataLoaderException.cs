using System;
using System.Runtime.Serialization;

namespace FastDataLoader
{
    public class FastDataLoaderException : Exception
    {
        public FastDataLoaderException()
        {
        }

        public FastDataLoaderException( string message ) : base( message )
        {
        }

        public FastDataLoaderException( string message, Exception innerException ) : base( message, innerException )
        {
        }

        protected FastDataLoaderException( SerializationInfo info, StreamingContext context ) : base( info, context )
        {
        }
    }
}

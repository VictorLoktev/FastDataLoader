using System;
using System.Runtime.Serialization;

namespace FastDataLoader
{
	/// <summary>
	/// <para>Базовой исключение для библиотеки.</para>
    /// <para>Самой библиотекой ни где не кидается.</para>
	/// </summary>
	public class DataLoaderException : Exception
    {
        public DataLoaderException()
        {
        }

        public DataLoaderException( string message ) : base( message )
        {
        }

        public DataLoaderException( string message, Exception innerException ) : base( message, innerException )
        {
        }

        protected DataLoaderException( SerializationInfo info, StreamingContext context ) : base( info, context )
        {
        }
    }
}

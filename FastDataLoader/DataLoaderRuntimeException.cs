using System;
using System.Runtime.Serialization;

namespace FastDataLoader
{
    /// <summary>
    /// <para>Исключение кидается при обнаружении проблемы вовремя
    /// инициализации экземпляра заполняемого типа,
    /// например из БД пришел null, когда тип может его принять.</para>
    /// </summary>
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

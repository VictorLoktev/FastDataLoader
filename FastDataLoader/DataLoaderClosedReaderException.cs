using System;
using System.Runtime.Serialization;

namespace FastDataLoader
{
	/// <summary>
	/// <para>Исключение видается при попытке чтения из закрытого IDataReader,
    /// или когда набор пустой (0 колонок).</para>
    /// <para>В любом случае по данному исключению можно полагать,
    /// что выборка данных не могла быть успешной.</para>
	/// </summary>
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

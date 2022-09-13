using System;
using System.Runtime.Serialization;

namespace FastDataLoader
{
	/// <summary>
	/// <para>Методы
	/// <see cref="DataLoaderLoadContext.Load1{T}"/>,
	/// <see cref="DataLoaderLoadContext.Load1{T}(DataLoaderOptions){T}"/>,
	/// <see cref="DataLoaderToContext.To{T}(out T)"/>
	/// возвращают единичное экземпляр типа, что налагает ограничение на выборку данных из БД:
	/// выборка должна содержать ровно одну строку данных, не больше и не меньше</para>
	/// <para>Если выборка содержала более одной строки,
	/// вызывается делегат <see cref="DataLoaderOptions.TooManyRecordsExceptionDelegate"/>,
	/// который должен вернуть экземпляр исключения для его бросания библиотекой.
	/// Если делегат не задан или не был переопределен, библиотека кидает данное исключение.</para>
	/// </summary>
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

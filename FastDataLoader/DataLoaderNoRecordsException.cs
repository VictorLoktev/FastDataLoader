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
	/// <para>Если выборка содержала менее одной строки (не было строк в выборке, но колонки были),
	/// вызывается делегат <see cref="DataLoaderOptions.NoRecordsExceptionDelegate"/>,
	/// который должен вернуть экземпляр исключения для его бросания библиотекой.
	/// Если делегат не задан или не был переопределен, библиотека кидает данное исключение.</para>
	/// <para>Ситуация, когда выборка не содержит колонок,
	/// расценивается как чтение из закрытого IDataReader, в этом случае будет другое
	/// исключение - <see cref="DataLoaderClosedReaderException"/></para>
	/// </summary>
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

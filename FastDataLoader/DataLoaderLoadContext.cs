using System;
using System.Data;

namespace FastDataLoader
{
	/// <summary>
	/// <para>Реализация абстрактного класса <see cref="DataLoaderLoadContext"/> должна включать методы</para>
	/// <para><see cref="GetDataReader"/> и <see cref="DumpSql"/>.</para>
	/// <para>Метод <see cref="DumpSql"/> вызывается в случае исключения при при вызове метода <see cref="GetDataReader"/>.
	/// <see cref="DumpSql"/>Формирует предназначен для выдачу в лог сообщения об ошибке,
	/// включающем подробности и детали выполняемой команды.
	/// Если выдача деталей ошибки в лог не требуется, реализаци метода может быть пустой.</para>
	/// <para>Методы классов <see cref="DataLoaderLoadContext"/> и <see cref="DataLoaderToContext"/>
	/// не делают Dispose ни для IDataReader, возвращаемого методом <see cref="GetDataReader"/>,
	/// ни для команды, выполняющей SQL, к которой привязан IDataReader.
	/// Поэтому ответственность делать Dispose для IDataReader и команды лежит на коде,
	/// использующем классы <see cref="DataLoaderLoadContext"/> и <see cref="DataLoaderToContext"/>.</para>
	/// </summary>
	[System.Diagnostics.DebuggerNonUserCode()]
	public abstract class DataLoaderLoadContext
    {
		/// <summary>
		/// <para>Данный метод путем вызова <see cref="GetDataReader"/>() активирует выполнение SQL-команды и чтения данных из БД.</para>
		/// <para>Затем формирует контекст <see cref="DataLoaderToContext"/> для последующих вызовов методов <see cref="DataLoaderToContext.To"/>.</para>
		/// <para>Параметры чтения данных задаются по умолчанию.</para>
		/// </summary>
		/// <returns></returns>
		public DataLoaderToContext Load()
		{
			return Load( new DataLoaderOptions() );
		}

		/// <summary>
		/// <para>Данный метод путем вызова <see cref="GetDataReader"/>() активирует выполнение SQL-команды и чтения данных из БД.</para>
		/// <para>Затем формирует контекст <see cref="DataLoaderToContext"/> для последующих вызовов методов <see cref="DataLoaderToContext.To"/>.</para>
		/// <para>Параметры чтения данных задаются в параметре <see cref="options"/> данного конструктора.</para>
		/// </summary>
		/// <returns></returns>
		/// <param name="options">Опции работы размещения данных в классах и структурах.</param>
		/// <returns></returns>
		public DataLoaderToContext Load( DataLoaderOptions options )
		{
			try
			{
				return DataLoaderToContextFabric( GetDataReader(), options );
			}
			catch( Exception ex )
			{
				DumpSql( ex );
				throw;
			}
		}

		/// <summary>
		/// Метод перекрывается в случае, если от класса <see cref="DataLoaderToContext"/> надо наследовать другой класс,
		/// чтобы методы Load и To возвращали отнаследованный от <see cref="DataLoaderToContext"/> класс.
		/// </summary>
		/// <param name="reader">Чтение одного или серии выборок из БД.</param>
		/// <param name="options">Параметры настроек.</param>
		/// <returns></returns>
		public virtual DataLoaderToContext DataLoaderToContextFabric( IDataReader reader, DataLoaderOptions options )
		{
			return new DataLoaderToContext( GetDataReader(), options );
		}


		public T Load1<T>( string ifNotExactlyOneRecordExceptionMessage = null )
		{
			Load( new DataLoaderOptions() )
				.To( out T result, ifNotExactlyOneRecordExceptionMessage )
				.End();
			return result;
		}

		public T Load1<T>( DataLoaderOptions options, string ifNotExactlyOneRecordExceptionMessage = null )
		{
			Load( options )
				.To( out T result, ifNotExactlyOneRecordExceptionMessage )
				.End();
			return result;
		}

		public T[] Load<T>()
		{
			Load( new DataLoaderOptions() )
				.To( out T[] result )
				.End();
			return result;
		}

		public T[] Load<T>( DataLoaderOptions options )
		{
			Load( options )
				.To( out T[] result )
				.End();
			return result;
		}

		public abstract IDataReader GetDataReader();

		/// <summary>
		/// <para>Через вызов данного метода осуществляется выдача информации об ошибках при сбоях в методе <see cref="Load"/></para>
		/// <para>Перекрытие метода позволяет сохранять в логах дополнительную информацию, непоказываемую обычным пользователям.</para>
		/// </summary>
		/// <param name="exception">Исключение, произошедшее при работе с SQL</param>
		public abstract void DumpSql( Exception exception );
	}
}

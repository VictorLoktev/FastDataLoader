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
	/// <para><see cref="GetDataReader"/>Должен выполнить команду и вернуть IDataReader с одним или несколькими наборами данных.
	/// Способы реализации ограничены только двумя условиями:</para>
	/// <list type="bullet">
	/// <item>IDataReader, формируемый методом <see cref="GetDataReader"/> должен быть активным до вызова
	/// метода <see cref="DataLoaderToContext.DisposeContext"/>.</item>
	/// <item>В случае, если к команде привязан другой активный IDataReader
	/// (например, не все ранее выбираемые из БД данные прочитаны или не был вызван метод <see cref="DataLoaderToContext.End"/>),
	/// методом <see cref="GetDataReader"/> должен правильно закрыть прошлый IDataReader и команду,
	/// затем выполнить новую команду и выдать новый IDataReader.</item>
	/// </list>
	/// </summary>
	[System.Diagnostics.DebuggerNonUserCode()]
	public abstract class DataLoaderLoadContext
    {
		/// <summary>
		/// <para>Вызовом метода <see cref="GetDataReader"/> активирует выполнение команды и чтения данных из БД.</para>
		/// <para>Формирует контекст для последующих вызовов методов <see cref="DataLoaderToContext.To"/>.</para>
		/// <para>Применяются опции <see cref="DataLoaderOptions"/> по умолчанию.</para>
		/// <para>ВАЖНО!</para>
		/// <para>Последним в цепочки вызовов вида x.Load().To(..).To(..).End()
		/// обязательно должен быть вызов метода <see cref="DataLoaderToContext.End"/>!</para>
		/// </summary>
		/// <returns></returns>
		public DataLoaderToContext Load()
		{
			return Load( new DataLoaderOptions() );
		}

		/// <summary>
		/// <para>Вызовом метода <see cref="GetDataReader"/> активирует выполнение команды и чтения данных из БД.</para>
		/// <para>Формирует контекст для последующих вызовов методов <see cref="DataLoaderToContext.To"/>.</para>
		/// <para>Применяются опции <see cref="DataLoaderOptions"/>, заданные в параметре.</para>
		/// <para>ВАЖНО!</para>
		/// <para>Последним в цепочки вызовов вида x.Load().To(..).To(..).End()
		/// обязательно должен быть вызов метода <see cref="DataLoaderToContext.End"/>!</para>
		/// </summary>
		/// <returns></returns>
		/// <param name="options">Опции работы размещения данных в классах и структурах.</param>
		/// <returns></returns>
		public DataLoaderToContext Load( DataLoaderOptions options )
		{
			try
			{
				return new DataLoaderToContext( GetDataReader(), options );
			}
			catch( Exception ex )
			{
				DumpSql( ex );
				throw;
			}
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
		public abstract void DumpSql( Exception exception );
	}
}

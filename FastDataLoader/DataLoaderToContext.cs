using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace FastDataLoader
{
#if !DEBUG
	[System.Diagnostics.DebuggerNonUserCode()]
#endif
	public class DataLoaderToContext
	{
		private IDataReader Reader;
		private readonly DataLoaderOptions Options;

		/// <summary>
		/// Данный конструктор предназначен для вызова только из класса <see cref="DataLoaderLoadContext"/>.
		/// </summary>
		/// <param name="reader">Чтение одного или серии выборок из БД.</param>
		/// <param name="options">Параметры настроек.</param>
		internal DataLoaderToContext( IDataReader reader, DataLoaderOptions options )
		{
			Reader = reader ?? throw new ArgumentNullException( nameof( reader ) );
			Options = options ?? throw new ArgumentNullException( nameof( options ) );
		}

		/// <summary>
		/// <para>Сохранение выборки в массив.</para>
		/// <para>Не возвращает null, при пустом наборе возвращается <see cref="T[]"/></para>
		/// <para>Количество записей в массиве от 0 до бесконечности.</para>
		/// <para>Исключения включены, выдаются стандартные сообщения.</para>
		/// </summary>
		/// <typeparam name="T">Массив с результатом</typeparam>
		/// <param name="result">Контекст чтобы сделать следующий вызов метода <see cref="To"/> или <see cref="End"/></param>
		/// <returns></returns>
		public DataLoaderToContext To<T>( out T[] result )
		{
			if( Reader == null || Reader.IsClosed || Reader.FieldCount == 0)
				throw new FastDataLoaderException( "There is a try to load from already closed DataReader" );

			List<T> list = DataLoader<T>.LoadOneResultSet( Reader, Options, int.MaxValue );
			//if( !Reader.NextResult() )
			//	End();
			Reader.NextResult();

			result = list?.ToArray() ?? Array.Empty<T>();

			return this;
		}

		/// <summary>
		/// <para>Сохранение выборки в массив.</para>
		/// <para>Не возвращает null, при пустом наборе возвращается <see cref="T[]"/></para>
		/// <para>Количество записей в массиве от 0 до бесконечности.</para>
		/// <para>Исключения включены, выдаются стандартные сообщения.</para>
		/// </summary>
		/// <typeparam name="T">Список с результатом</typeparam>
		/// <param name="result">Контекст чтобы сделать следующий вызов метода <see cref="To"/> или <see cref="End"/></param>
		/// <returns></returns>
		public DataLoaderToContext To<T>( out List<T> result )
		{
			if( Reader == null || Reader.IsClosed || Reader.FieldCount == 0 )
				throw new FastDataLoaderException( "There is a try to load from already closed DataReader" );

			List<T> list = DataLoader<T>.LoadOneResultSet( Reader, Options, int.MaxValue );
			result = list ?? new List<T>( 1 );

			Reader.NextResult();

			return this;
		}

		/// <summary>
		/// <para>Сохранение выборки в переменную (не массив).</para>
		/// <para>Не возвращает null.</para>
		/// <para>Количество записей в выборке должно быть равно 1, в противном случае выдается исключение.</para>
		/// <para>Текст исключения настраивается опциями в параметре конструктора.</para>
		/// <para>Параметр <see cref="nameof(DataLoaderOptions)"/>.<see cref="nameof(DataLoaderOptions.NoRecordsExceptionMessage)"/>
		/// задает текст сообщения для исключения, когда результат содержит менее 1 записи.</para>
		/// <para>Параметр <see cref="nameof(DataLoaderOptions)"/>.<see cref="nameof(DataLoaderOptions.TooManyRecordsExceptionMessage)"/>
		/// задает текст сообщения для исключения, когда результат содержит более 1 записи.</para>
		/// </summary>
		/// <typeparam name="T">Инициализируемый данными тип</typeparam>
		/// <param name="result">Контекст чтобы сделать следующий вызов метода <see cref="To"/> или <see cref="End"/></param>
		/// <returns></returns>
		public DataLoaderToContext To<T>( out T result )
		{
			if( Reader == null || Reader.IsClosed || Reader.FieldCount == 0 )
				throw new FastDataLoaderException( "There is a try to load from already closed DataReader" );

			List<T> list = DataLoader<T>.LoadOneResultSet( Reader, Options, 1 );

			if( list == null || list.Count < 1 )
			{
				throw new FastDataLoaderException(
					Options.NoRecordsExceptionMessage ??
					$"Нарушение при выборке данных из БД - набор данных пуст " +
					$"(ожидаемый тип - {DataLoader<T>.GetCSharpTypeName( typeof( T ) )})" );
			}
			else
			if( list.Count > 1 || Reader.Read() )
			{
				throw new FastDataLoaderException(
					Options.TooManyRecordsExceptionMessage ??
					$"Нарушение при выборке данных из БД - выбрано более одной строки данных " +
					$"(ожидаемый тип - {DataLoader<T>.GetCSharpTypeName( typeof( T ) )})" );
			}
			else
			{
				result = list[ 0 ];
			}

			Reader.NextResult();

			return this;
		}

		public void End()
		{
			try
			{
				Reader?.Dispose();
			}
			catch { }
			Reader = null;
		}
	}
}

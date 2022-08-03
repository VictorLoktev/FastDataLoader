using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace FastDataLoader
{
#if !DEBUG
	[System.Diagnostics.DebuggerNonUserCode()]
#endif
	public class DataLoaderToContext
	{
		private IDataReader Reader;
		private readonly DataLoaderOptions Options;
		public Stopwatch Timer { get; private set; }


		/// <summary>
		/// Данный конструктор предназначен для вызова только из класса <see cref="DataLoaderLoadContext"/>.
		/// </summary>
		/// <param name="reader">Чтение одного или серии выборок из БД.</param>
		/// <param name="options">Параметры настроек.</param>
		internal DataLoaderToContext( IDataReader reader, DataLoaderOptions options )
		{
			Reader = reader ?? throw new ArgumentNullException( nameof( reader ) );
			Options = options ?? throw new ArgumentNullException( nameof( options ) );
			Timer = Stopwatch.StartNew();
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
			if( Reader == null )
				throw new FastDataLoaderException( "There is a try to load from already closed DataReader" );

			List<T> list = null;
			if( !Reader.IsClosed )
			{
				list = DataLoader<T>.LoadOne( Reader, Options );
				if( !Reader.NextResult() )
					End();
			}
			result = list?.ToArray() ?? Array.Empty<T>();

			return this;
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
		public DataLoaderToContext To<T>( out List<T> result )
		{
			if( Reader == null )
				throw new FastDataLoaderException( "There is a try to load from already closed DataReader" );

			List<T> list = null;
			if( !Reader.IsClosed )
			{
				list = DataLoader<T>.LoadOne( Reader, Options );
				if( !Reader.NextResult() )
					End();
			}
			result = list ?? new List<T>( 0 );

			return this;
		}

		/// <summary>
		/// <para>Сохранение выборки в переменную (не массив).</para>
		/// <para>Не возвращает null.</para>
		/// <para>Количество записей в выборке должно быть равно 1, в противном случае выдается исключение.</para>
		/// <para>Параметр <see cref="ifNotExactlyOneRecordExceptionMessage"/> задает текст сообщения для исключения,
		/// выдаваемого, когда прочитано записей менее или более 1.</para>
		/// <para>Если параметр <see cref="ifNotExactlyOneRecordExceptionMessage"/> не задан или null,
		/// выдает стандартное сообщение исключения.</para>
		/// </summary>
		/// <typeparam name="T">Инициализируемый данными тип</typeparam>
		/// <param name="result">Контекст чтобы сделать следующий вызов метода <see cref="To"/> или <see cref="End"/></param>
		/// <param name="ifNoRecordsExceptionMessage">Текст сообщения, когда выборки нет совсем или получено 0 записей.</param>
		/// <param name="ifManyRecordsExceptionMessage">Текст сообщения, когда записей получено более одной.</param>
		/// <returns></returns>
		public DataLoaderToContext To<T>( out T result,
			string ifNoRecordsExceptionMessage = null, string ifManyRecordsExceptionMessage = null )
		{
			if( Reader == null )
				throw new FastDataLoaderException( "There is a try to load from already closed DataReader" );

			DataLoaderOptions opt = Options.Clone();
			opt.LimitRecords = 2;

			List<T> list = null;
			if( !Reader.IsClosed )
			{
				list = DataLoader<T>.LoadOne( Reader, Options );
				if( !Reader.NextResult() )
					End();
			}

			if( list == null || list.Count < 1 )
				throw new FastDataLoaderException(
					ifNoRecordsExceptionMessage ??
					$"Нарушение при выборке данных из БД - набор данных пуст " +
					$"(ожидаемый тип - {DataLoader<T>.GetCSharpTypeName( typeof( T ) )})" );
			else
			if( list.Count > 1 )
				throw new FastDataLoaderException(
					ifManyRecordsExceptionMessage ??
					$"Нарушение при выборке данных из БД - выбрано более одной строки данных " +
					$"(ожидаемый тип - {DataLoader<T>.GetCSharpTypeName( typeof( T ) )})" );
			else
				result = list[ 0 ];

			return this;
		}

		public void End()
		{
			Timer.Stop();
			if( Reader != null )
			{
				try
				{
					Reader.Dispose();
				}
				catch { }
				Reader = null;
			}
		}

		#region Переопределяемый по необходимости метод

		/// <summary>
		/// Метод формирует stack trace для записи в лог.
		/// Стандартный текст сообщения можно заменить через перекрытие (override) метода.
		/// </summary>
		/// <param name="skipFrames">Сколько frame-ов отступить от начала чтобы не выдавать лишнюю информацию.</param>
		/// <returns></returns>
		public virtual string GetStackTrace( int skipFrames )
		{
			StringBuilder sb = new StringBuilder();
			//Get a StackTrace object for the exception
			StackTrace stackFrame = new StackTrace( skipFrames, true );
			foreach( StackFrame frame in stackFrame.GetFrames() )
			{
				//Get the file name
				string fileName = frame.GetFileName();
				//Get the method name
				//string methodName = frame.GetMethod().ToString();
				string methodName = frame.GetMethod().Name;
				//Get the line number from the stack frame
				int line = frame.GetFileLineNumber();

				if( !string.IsNullOrEmpty( fileName ) )
					fileName = System.IO.Path.GetFileName( fileName );
				else
					fileName = "[External Code]";

				sb.AppendFormat( "{0,-30} => {1}():{2}", fileName, methodName, line );
				sb.AppendLine();
			}
			return sb.ToString();
		}

		#endregion
	}
}

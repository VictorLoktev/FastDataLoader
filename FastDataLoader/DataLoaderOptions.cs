using System.Text;

namespace FastDataLoader
{
	public class DataLoaderOptions
	{
		/// <summary>
		/// Если true, формировать исключение, если в DataReader
		/// остались колонки без соответствия в заполняемом классе/структуре.
		/// По умолчанию - true.
		/// </summary>
		public bool ExceptionIfUnmappedReaderColumn { get; set; }
		/// <summary>
		/// Если true, формировать исключение, если в в заполняемом классе/структуре
		/// остались поля или свойства без соответствия колонкам в DataReader.
		/// По умолчанию - true.
		/// </summary>
		public bool ExceptionIfUnmappedFieldOrProperty { get; set; }
		/// <summary>
		/// Перечень названий колонок в DataReader, которые должны быть проигнорированы
		/// при составлении соответствия колонок и членов заполняемого класса/структуры.
		/// По умолчанию - пусто.
		/// </summary>
		public string[] IgnoreColumnNames { get { return _IgnoreColumnNames; } set { _Hash = null; _IgnoreColumnNames = value; } }
		private string[] _IgnoreColumnNames;

		/// <summary>
		/// Ограничение количества читаемых записей.
		/// По умолчанию - без ограничения.
		/// </summary>
		public int? LimitRecords { get; set; }

		/// <summary>
		/// <para>Когда значения колонки IDataReader имеют тип decimal,
		/// преобразование в строку выдает незначащие нули.</para>
		/// <para>Например, при чтении из БД cast(123.45 as numeric(18,8)
		/// с последующим и преобразовании в строку даст 123.45000000</para>
		/// <para>Подобные незначащие нули можно отрезать,
		/// если поделить значение на 1.000000000000000000000000000000000m</para>
		/// <para>И чем делать подобную операцию несколько раз при выводе,
		/// лучше сделать 1 раз при чтении значения из БД.</para>
		/// <para>True, если для значений типа decimal сразу отрезаются незначащие нули.</para>
		/// <para>False, если для значений типа decimal не делается преобразование с отрезанием нулей.</para>
		/// <para>Если тип не decimal, то изменений не делается в любом случае.</para>
		/// </summary>
		public bool RemoveTrailingZerosForDecimal { get { return _RemoveTrailingZerosForDecimal; } set { _Hash = null; _RemoveTrailingZerosForDecimal = value; } }
		private bool _RemoveTrailingZerosForDecimal;

		/// <summary>
		/// <para>Текст сообщения исключения при вызове метода,
		/// где результатом должна являться только одна строка без массива или списка,
		/// а вместо этого результат выборки пустой (получено 0 записей).</para>
		/// <para>Если текст не задан (здесь null), то выдается стандартное сообщение.</para>
		/// </summary>
		public string NoRecordsExceptionMessage { get; set; }

		/// <summary>
		/// <para>Текст сообщения исключения при вызове метода,
		/// где результатом должна являться только одна строка без массива или списка,
		/// а вместо этого результат выборки содержит более 1 строки (получено 2 или более записей).</para>
		/// <para>Если текст не задан (здесь null), то выдается стандартное сообщение.</para>
		/// </summary>
		public string TooManyRecordsExceptionMessage { get; set; }

		private int? _Hash;

		public override int GetHashCode()
		{
			if( !_Hash.HasValue )
			{
				unchecked
				{
					_Hash = RemoveTrailingZerosForDecimal.GetHashCode();
					if( IgnoreColumnNames == null )
						IgnoreColumnNames = new string[] { };
					foreach( string item in IgnoreColumnNames )
					{
						_Hash = _Hash * 23 + item.GetHashCode();
					}
				}
			}
			return _Hash.Value;
		}

		public DataLoaderOptions()
		{
			ExceptionIfUnmappedReaderColumn = true;
			ExceptionIfUnmappedFieldOrProperty = true;
			IgnoreColumnNames = new string[] { };
			LimitRecords = null;
			RemoveTrailingZerosForDecimal = true;
			NoRecordsExceptionMessage = null;
			TooManyRecordsExceptionMessage = null;

			_Hash = null;
		}
	}
}

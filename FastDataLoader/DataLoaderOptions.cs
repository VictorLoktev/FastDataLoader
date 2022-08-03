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
		public string[] IgnoreColumnNames { get; set; }

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
		public bool RemoverTrailingZerosForDecimal { get; set; }


		public DataLoaderOptions()
		{
			ExceptionIfUnmappedReaderColumn = true;
			ExceptionIfUnmappedFieldOrProperty = true;
			IgnoreColumnNames = new string[] { };
			LimitRecords = null;
			RemoverTrailingZerosForDecimal = true;
		}

		public DataLoaderOptions Clone()
		{
			return
				new DataLoaderOptions()
				{
					ExceptionIfUnmappedReaderColumn = ExceptionIfUnmappedReaderColumn,
					ExceptionIfUnmappedFieldOrProperty = ExceptionIfUnmappedFieldOrProperty,
					IgnoreColumnNames = IgnoreColumnNames,
					LimitRecords = LimitRecords,
				};
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append( "[" );
			sb.Append( nameof( ExceptionIfUnmappedReaderColumn ) );
			sb.Append( ": " );
			sb.Append( ExceptionIfUnmappedReaderColumn );
			sb.Append( "][" );
			sb.Append( nameof( ExceptionIfUnmappedFieldOrProperty ) );
			sb.Append( ": " );
			sb.Append( ExceptionIfUnmappedFieldOrProperty );
			sb.Append( "]" );

			if( IgnoreColumnNames == null )
				IgnoreColumnNames = new string[] { };
			foreach( string ignore in IgnoreColumnNames )
			{
				sb.Append( "[IgnoresColumnName:" );
				sb.Append( ignore );
				sb.Append( "]" );
			}

			return sb.ToString();
		}
	}
}

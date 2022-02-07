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
		/// Перечень названий колонок в DataReader, которые должны быть игнорировыны
		/// при составлении соответствия колонок и членов заполняемого класса/структуры.
		/// По умолчанию - пусто.
		/// </summary>
		public string[] IgnoresColumnNames { get; set; }

		/// <summary>
		/// Ограничение количества читаемых записей.
		/// По умолчанию - без ограничения.
		/// </summary>
		public int? Limit { get; set; }

		public DataLoaderOptions()
		{
			ExceptionIfUnmappedReaderColumn = true;
			ExceptionIfUnmappedFieldOrProperty = true;
			IgnoresColumnNames = new string[] { };
			Limit = null;
		}

		public DataLoaderOptions Clone()
		{
			return
				new DataLoaderOptions()
				{
					ExceptionIfUnmappedReaderColumn = true,
					ExceptionIfUnmappedFieldOrProperty = true,
					IgnoresColumnNames = new string[] { },
					Limit = null,
				};
		}
	}
}

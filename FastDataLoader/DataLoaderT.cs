using System.Collections.Generic;
using System.Data;

namespace FastDataLoader
{
//	[System.Diagnostics.DebuggerNonUserCode()]
	public partial class DataLoader<T>
	{
		public static List<T> LoadOne( IDataReader reader, DataLoaderOptions options )
		{
			if( reader.IsClosed )
				return null;

			LoaderInfo info = GetLoaderInfo( reader, options );
			if( info == null )
				return null;
			List<T> result = LoadList( reader, info, options );

			return result;
		}

		private static List<T> LoadList( IDataReader reader, LoaderInfo info, DataLoaderOptions options )
		{
			if( reader.IsClosed ||
				reader.FieldCount == 0 ||
				options.LimitRecords.HasValue && options.LimitRecords.Value <= 0 )
				return null;

			if( info.Initializer == null )
				throw new DataLoaderException( "Сбой в алгоритме, инициализатор данных не заполнен" );

			List<T> result = options.LimitRecords.HasValue
				? new List<T>( options.LimitRecords.Value )
				: new List<T>();

			while( reader.Read() )
			{
				if( options.LimitRecords == result.Count )
					break;

				result.Add( info.Initializer( reader ) );
			}

			return result;
		}
	}
}

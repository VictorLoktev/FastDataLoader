using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using Microsoft.CSharp;

namespace FastDataLoader
{
#if !DEBUG
	[System.Diagnostics.DebuggerNonUserCode()]
#endif
	public partial class DataLoader<T>
	{
		#region Структуры и декларации

		private class ErrorHolder
		{
			public int ErrorPoiter;

			public ErrorHolder()
			{
				ErrorPoiter = -1;
			}
		}

		private class LoaderInfo
		{
			/*
			 * ErrorHolder - это должен быть class, не структура, т.к. в его члене подменяется значение в цикле.
			 */
			public Func<IDataReader, ErrorHolder, T> Initializer;
			public List<string> ErrorRegister = new List<string>();
		}

		private static readonly SortedDictionary<int, LoaderInfo> LoaderDictionary = new SortedDictionary<int, LoaderInfo>();
		private static readonly SemaphoreSlim Sema = new SemaphoreSlim( 1, 1 );

		private struct DataReaderInfo
		{
			public MethodInfo GetBoolean;
			public MethodInfo GetByte;
			public MethodInfo GetChar;
			public MethodInfo GetDateTime;
			public MethodInfo GetDecimal;
			public MethodInfo GetDouble;
			public MethodInfo GetFloat;
			public MethodInfo GetGuid;
			public MethodInfo GetInt16;
			public MethodInfo GetInt32;
			public MethodInfo GetInt64;
			public MethodInfo GetString;
			public MethodInfo GetValue;
			public MethodInfo IsDBNull;

			public DataReaderInfo( IDataReader reader )
			{
				GetBoolean = null;
				GetByte = null;
				GetChar = null;
				GetDateTime = null;
				GetDecimal = null;
				GetDouble = null;
				GetFloat = null;
				GetGuid = null;
				GetInt16 = null;
				GetInt32 = null;
				GetInt64 = null;
				GetString = null;
				GetValue = null;
				IsDBNull = null;

				foreach( MethodInfo mi in reader.GetType().GetInterfaceMap( typeof( IDataRecord ) ).InterfaceMethods )
				{
					ParameterInfo[] pi = mi.GetParameters();
					if( pi.Length != 1 || pi[ 0 ].ParameterType != typeof( int ) )
						continue;
					if( mi.ReturnType == typeof( bool ) )
					{
						if( mi.Name.Equals( "GetBoolean", StringComparison.OrdinalIgnoreCase ) )
							GetBoolean = mi;
						if( mi.Name.Equals( "IsDBNull", StringComparison.OrdinalIgnoreCase ) )
							IsDBNull = mi;
					}
					if( mi.ReturnType == typeof( byte ) && mi.Name.Equals( "GetByte", StringComparison.OrdinalIgnoreCase ) )
						GetByte = mi;
					else
					if( mi.ReturnType == typeof( char ) && mi.Name.Equals( "GetChar", StringComparison.OrdinalIgnoreCase ) )
						GetChar = mi;
					else
					if( mi.ReturnType == typeof( decimal ) && mi.Name.Equals( "GetDecimal", StringComparison.OrdinalIgnoreCase ) )
						GetDecimal = mi;
					else
					if( mi.ReturnType == typeof( DateTime ) && mi.Name.Equals( "GetDateTime", StringComparison.OrdinalIgnoreCase ) )
						GetDateTime = mi;
					else
					if( mi.ReturnType == typeof( double ) && mi.Name.Equals( "GetDouble", StringComparison.OrdinalIgnoreCase ) )
						GetDouble = mi;
					else
					if( mi.ReturnType == typeof( float ) && mi.Name.Equals( "GetFloat", StringComparison.OrdinalIgnoreCase ) )
						GetFloat = mi;
					else
					if( mi.ReturnType == typeof( Guid ) && mi.Name.Equals( "GetGuid", StringComparison.OrdinalIgnoreCase ) )
						GetGuid = mi;
					else
					if( mi.ReturnType == typeof( short ) && mi.Name.Equals( "GetInt16", StringComparison.OrdinalIgnoreCase ) )
						GetInt16 = mi;
					else
					if( mi.ReturnType == typeof( int ) && mi.Name.Equals( "GetInt32", StringComparison.OrdinalIgnoreCase ) )
						GetInt32 = mi;
					else
					if( mi.ReturnType == typeof( long ) && mi.Name.Equals( "GetInt64", StringComparison.OrdinalIgnoreCase ) )
						GetInt64 = mi;
					else
					if( mi.ReturnType == typeof( string ) && mi.Name.Equals( "GetString", StringComparison.OrdinalIgnoreCase ) )
						GetString = mi;
					else
					if( mi.Name.Equals( "GetValue", StringComparison.OrdinalIgnoreCase ) )
						GetValue = mi;
				}
				if( GetBoolean == null ||
					GetByte == null ||
					GetChar == null ||
					GetDateTime == null ||
					GetDecimal == null ||
					GetDouble == null ||
					GetFloat == null ||
					GetGuid == null ||
					GetInt16 == null ||
					GetInt32 == null ||
					GetInt64 == null ||
					GetString == null ||
					GetValue == null ||
					IsDBNull == null
					)
					throw new FastDataLoaderException( "Не получилось определить MethodInfo для всех нужных методов у IDataReader" );
			}
		}

		#endregion

		public static List<T> LoadOneResultSet( IDataReader reader, DataLoaderOptions options, int maxRecords )
		{
			if( options.LimitRecords.HasValue && options.LimitRecords.Value <= 0 ||
				maxRecords <= 0 )
				return null;

			int key = typeof( T ).GetHashCode();
			unchecked
			{
				key = key * 23 + options.GetHashCode();

				int readerFieldCount = reader.FieldCount;
				for( int i = 0; i < readerFieldCount; i++ )
				{
					key = key * 23 + reader.GetFieldType( i ).GetHashCode();
					key = key * 23 + reader.GetName( i ).GetHashCode();
				}
			}

			LoaderInfo info;
			try
			{
				Sema.Wait();
				if( !LoaderDictionary.TryGetValue( key, out info ) )
					info = null;
			}
			finally
			{
				Sema.Release();
			}
			if( info == null )
			{
				info = GetLoaderInfo( reader, options );
				if( info == null || info.Initializer == null )
					throw new FastDataLoaderException( "Сбой в алгоритме, инициализатор данных не заполнен" );

				try
				{
					Sema.Wait();
					LoaderDictionary[ key ] = info;
				}
				finally
				{
					Sema.Release();
				}
			}

			if( options.LimitRecords.HasValue && maxRecords > options.LimitRecords.Value )
				maxRecords = options.LimitRecords.Value;

			ErrorHolder err = new ErrorHolder();

			try
			{
				return new List<T>( Yielder( reader, info.Initializer, err, maxRecords ) );
			}
			catch( Exception ex )
			{
				if( err != null && err.ErrorPoiter >= 0 && err.ErrorPoiter < info.ErrorRegister.Count )
					throw new FastDataLoaderException( info.ErrorRegister[ err.ErrorPoiter ], ex );
				throw;
			}
		}

		private static IEnumerable<T> Yielder( IDataReader reader,
			Func<IDataReader, ErrorHolder, T> initializer, ErrorHolder holder, int maxRecords )
		{
			for( int readCounter = 0; readCounter < maxRecords && reader.Read(); readCounter++ )
			{
				yield return initializer( reader, holder );
			}
		}

		#region Дополнительные методы

		/*
		 * По хорошему надо встроить в дерево формируемого Expression вот такую часть
		 * Expression.Divide( typedVariable, Expression.Constant( 1.000000000000000000000000000000000m ) ),
		 * но к сожалению, в некоторых случаях обрезание нулей подобным образом не срабатывает.
		 * В тестовых методах данного проекта срабатывает, а на других проектах с reference на данную библиотеку - нет.
		 * Поэтому вместо Expression.Divide в Expression встраивается вызов
		 * данного метода для деления на 1.000000000000000000000000000000000m.
		 * И в проекте, где не работает, вроде начинает работать правильно.
		 */
		public static decimal TrimZerosDecimal( decimal a )
		{
			return a / 1.000000000000000000000000000000000m;
		}

		// См. комментарий к методу TrimZerosDecimal
		public static decimal? TrimZerosNullableDecimal( decimal a )
		{
			return a / 1.000000000000000000000000000000000m;
		}

		/// <summary>
		/// Выдает название типа, соответствующего языку C#. System.String как string, System.Int32 как int
		/// </summary>
		/// <param name="type">Тип</param>
		/// <returns></returns>
		public static string GetCSharpTypeName( Type type )
		{
			if( type == null )
				return null;
			using( var provider = new CSharpCodeProvider() )
			{
				return provider.GetTypeOutput( new CodeTypeReference( type ) );
			}
		}

		public static bool IsSimple( Type type )
		{
			// Источник: https://stackoverflow.com/questions/863881/how-do-i-tell-if-a-type-is-a-simple-type-i-e-holds-a-single-value
			var typeInfo = type.GetTypeInfo();
			if( typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof( Nullable<> ) )
			{
				// nullable type, check if the nested type is simple.
				return IsSimple( typeInfo.GetGenericArguments()[ 0 ] );
			}
			return typeInfo.IsPrimitive
			  || typeInfo.IsEnum
			  || type.Equals( typeof( string ) )
			  || type.Equals( typeof( byte[] ) )
			  || type.Equals( typeof( decimal ) )
			  || type.Equals( typeof( DateTime ) )
			  || type.Equals( typeof( Guid ) );
		}

		public static bool IsNullable( Type type )
		{
			return Nullable.GetUnderlyingType( type ) != null
				|| type == typeof( string )
				|| type == typeof( byte[] );
		}

		private static bool TakeMemberIntoAccount( ColumnAttribute attr )
		{
			// Если при члене класса не задан атрибут, то член используется в выборке
			if( attr == null )
				return true;
			// Если при члене класса задан атрибут, но отображение члена на колонку пустое, то член не используется в выборке
			if( string.IsNullOrWhiteSpace( attr.Name ) )
				return false;
			// Если при члене класса задан атрибут, отображение члена на колонку не пустое, то член используется в выборке
			return true;
		}

		#endregion
	}
}

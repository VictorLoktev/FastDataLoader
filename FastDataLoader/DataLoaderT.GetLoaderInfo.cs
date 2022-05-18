﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.CSharp;

namespace FastDataLoader
{
	public partial class DataLoader<T>
	{
		private class LoaderInfo
		{
			public Func<IDataReader, T> Initializer;
		}

		private static SortedDictionary<string, LoaderInfo> LoaderDictionary = new SortedDictionary<string, LoaderInfo>();
		private static SemaphoreSlim Sema = new SemaphoreSlim( 1, 1 );

		private static LoaderInfo GetLoaderInfo( IDataReader reader, DataLoaderOptions options )
		{
			if( reader.IsClosed )
				return null;

			// Кол-во полей в выборке
			int readerFieldCount = reader.FieldCount;
			if( readerFieldCount == 0 )
				return null;

			StringBuilder signature = new StringBuilder( typeof( T ).Name );
			string[] ReaderNames = new string[ readerFieldCount ];
			Type[] ReaderTypes = new Type[ readerFieldCount ];
			for( int i = 0; i < readerFieldCount; i++ )
			{
				ReaderTypes[ i ] = reader.GetFieldType( i );
				ReaderNames[ i ] = reader.GetName( i );
				signature.Append( '|' );
				signature.Append( ReaderNames[ i ] );
				signature.Append( '~' );
				signature.Append( ReaderTypes[ i ] );
			}
			signature.Append( options.ToString() );

			//// Важно чтобы при изменении колонок в DataReader'е (тип, название, очередность),
			//// а так же при изменении игнорируемых колонок в options
			//// составлялась новая карта соответствия.
			//// Все это учитывается при формировании строки ключа в signature.
			//// Поэтому дополнительная проверка не требуется.

			Sema.Wait();
			if( LoaderDictionary.TryGetValue( signature.ToString(), out LoaderInfo info ) )
			{
				Sema.Release();
				return info;
			}
			Sema.Release();

			//if( LoaderDictionary.TryGetValue( signature.ToString(), out LoaderInfo info ) )
			//	return info;

			info = new LoaderInfo();

			/* Источники:
			 * https://stackoverflow.com/questions/19841120/generic-dbdatareader-to-listt-mapping
			 * https://stackoverflow.com/questions/20427561/checking-for-nulls-on-db-record-mapping
			 * https://stackoverflow.com/questions/390578/creating-instance-of-type-without-default-constructor-in-c-sharp-using-reflectio
			 * https://stackoverflow.com/questions/31880663/dynamically-create-delegate-for-ctor
			 * https://stackoverflow.com/questions/16678966/expression-for-read-from-property-and-write-to-private-readonly-field
			 */


			PropertyInfo[] allProperties = typeof( T )
				.GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance )
				.Where( x => UseMember( x.GetCustomAttributes<ColumnAttribute>().FirstOrDefault() ) )
				.Where( x => x.CanWrite )
				.ToArray();

			FieldInfo[] allFields = typeof( T )
				.GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance )
				.Where( x => UseMember( x.GetCustomAttributes<ColumnAttribute>().FirstOrDefault() ) )
				.Where( x => !x.Name.Contains( "BackingField" ) )   // Поля автосгенерированный из свойств
				.ToArray();

			// Названия колонок могут быть либо такими же, как название члена класса или структуры, либо задаваться атрибутом Column
			string[] allPropertiesColumnNames = new string[ allProperties.Length ];
			for( int i = 0; i < allProperties.Length; i++ )
			{
				PropertyInfo property = allProperties[ i ];
				ColumnAttribute attr = property.GetCustomAttributes<ColumnAttribute>().FirstOrDefault();
				if( attr != null )
				{
					if( !string.IsNullOrWhiteSpace( attr.Name ) )
						allPropertiesColumnNames[ i ] = attr.Name;
					else
					{
						// Но если название колонки установлено в null, пустую строку или white space, то такой член класса игнорируется
						allPropertiesColumnNames[ i ] = null;
					}
				}
				else
					allPropertiesColumnNames[ i ] = property.Name;
			}
			string[] allFieldsColumnNames = new string[ allFields.Length ];
			for( int i = 0; i < allFields.Length; i++ )
			{
				FieldInfo field = allFields[ i ];
				ColumnAttribute attr = field.GetCustomAttributes<ColumnAttribute>().FirstOrDefault();
				if( attr != null )
				{
					if( !string.IsNullOrWhiteSpace( attr.Name ) )
						allFieldsColumnNames[ i ] = attr.Name;
					else
					{
						// Но если название колонки установлено в null, пустую строку или white space, то такой член класса игнорируется
						allFieldsColumnNames[ i ] = null;
					}
				}
				else
					allFieldsColumnNames[ i ] = field.Name;
			}


			#region Поиск подходящего конструктора

			// Считаем кол-во конструкторов с 2-мя и более параметрами
			int manyParametersCtor = 0;
			ConstructorInfo foundCtor = null;
			// Пытаемся найти конструктор с параметрами, совпадающими с читаемыми из БД колонками.
			// Если читается Tuple, то названия, не проверяются.
			// Следует учитывать, что ReaderTypes содержит типы колонок не nullable, то есть int вместо int?
			ConstructorInfo[] allCtors = typeof( T )
				.GetConstructors( BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance );
			foreach( ConstructorInfo ctor in allCtors )
			{
				ParameterInfo[] parameters = ctor.GetParameters();
				if( parameters.Length > 1 )
					manyParametersCtor++;

				if( parameters.Length != readerFieldCount )
					continue;

				int[] paramToColumnIndexer = new int[ readerFieldCount ];
				for( int i = 0; i < paramToColumnIndexer.Length; i++ )
				{
					paramToColumnIndexer[ i ] = -1;
				}

				for( int index = 0; index < readerFieldCount; index++ )
				{
					ParameterInfo parameter = parameters[ index ];

					Type paramType = Nullable.GetUnderlyingType( parameter.ParameterType ) ?? parameter.ParameterType;
					Type readerType = Nullable.GetUnderlyingType( ReaderTypes[ index ] ) ?? ReaderTypes[ index ];

					// Если тип параметра конструктура - Enum, то надо сопоставить его с читаемым из БД int
					if( paramType.IsEnum )
						paramType =
							Nullable.GetUnderlyingType( parameter.ParameterType ) == null
							? paramType.GetEnumUnderlyingType()
							: typeof( Nullable<> ).MakeGenericType( paramType.GetEnumUnderlyingType() );

					if( paramType != readerType )
						break;

					paramToColumnIndexer[ index ] = index;
				}
				if( !paramToColumnIndexer.Any( x => x == -1 ) )
				{
					foundCtor = ctor;
					break;
				}
			}

			#endregion
			#region Поиск метода преобразования типа, если тип колонки и принимающего элемента не совпадают

			MethodInfo castMethod = null;

			if( readerFieldCount == 1 &&
				( Nullable.GetUnderlyingType( typeof( T ) ) ?? typeof( T ) )
				!= ( Nullable.GetUnderlyingType( ReaderTypes[ 0 ] ) ?? ReaderTypes[ 0 ] ) )
			{
				foreach( MethodInfo mi in typeof( T ).GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static ) )
				{
					// Тип, являющийся элементом массива должен иметь статический метод,
					// возвращающий массив нужного типа и принимающий ровно 1 параметр типа string
					ParameterInfo[] pi = mi.GetParameters();
					if( mi.IsStatic &&
						mi.ReturnType == typeof( T ) &&
						pi != null &&
						pi.Length == 1 &&
						pi[ 0 ].ParameterType == ReaderTypes[ 0 ] )
					{
						castMethod = mi;
					}
				}
			}

			#endregion

			/*
			 * Когда в выборке одно поле, принимается как одно значение
			 * и тип простой, то нам не нужны конструкторы
			 * или иные присвоения - IDataReader и так возвращает значение,
			 * нам лишь нужно взять значение из IDataReader[0] и вернуть его,
			 * земенив DBNull.Value на null.
			 * Все это не относится и к строке, т.к. string - это class.
			 * У строки и у Guid'а нет конструктора с соответствующим типом.
			 * У int? нет конструктора с аргументом, принимающим null.
			 */
			bool directCopy = readerFieldCount == 1 &&
				 ( typeof( T ) == ReaderTypes[ 0 ]
				 || typeof( T ).IsEnum && ReaderTypes[ 0 ] == typeof( int )
				 || Nullable.GetUnderlyingType( typeof( T ) ) == ReaderTypes[ 0 ]
				 || typeof( T ) != ReaderTypes[ 0 ] && castMethod != null );


			if( !directCopy &&
				!IsSimple( typeof( T ) ) &&
				foundCtor == null &&
				manyParametersCtor > 0 )
			{
				/* Конструкторы без параметров или с одним параметром не учитыватся,
				 * т.к. такие классы/структуры загружатся без конструкторов,
				 * простым перекладыванием из IDataReader в целевое место.
				 */

				StringBuilder sb = new StringBuilder();
				foreach( var type in ReaderTypes )
				{
					if( sb.Length > 0 )
						sb.Append( ", " );
					sb.Append( GetCSharpTypeName( type ) );
				}
				throw new DataLoaderException(
					$"Ошибка инициализации типа '{GetCSharpTypeName( typeof( T ) )}': " +
					$"класс или структура имеет конструктор(ы), но ни один из них не подходит к загружаемым из БД данным. " +
					$"Проверьте типы и порядок колонок в выборке и типы и порядок агрументов конструктора. " +
					$"Ожидаемые типы: {sb}."
					);
			}

			var exps = new List<Expression>();
			var paramExp = Expression.Parameter( typeof( IDataRecord ), "param" );
			var targetExp = Expression.Variable( typeof( T ) );
			//does int based lookup
			var indexerInfo = typeof( IDataRecord ).GetProperty( "Item", new[] { typeof( int ) } );
			var invalidParameterExpression = typeof( DataLoaderException ).GetConstructor( new Type[] { typeof( string ) } );

			if( directCopy )
			{
				Type type = typeof( T );

				var block = ExpressionForConstructor(
					0, ReaderTypes[ 0 ], ReaderNames[ 0 ],
					type, null,
					paramExp, indexerInfo, invalidParameterExpression,
					castMethod, options );

				exps.Add( block );

				info.Initializer = Expression.Lambda<Func<IDataReader, T>>(
					Expression.Block( exps ), paramExp ).Compile();
			}
			else
			if( foundCtor != null )
			{
				var ctorExps = new List<Expression>();

				for( int columnIndex = 0; columnIndex < readerFieldCount; columnIndex++ )
				{
					ParameterInfo[] paramInfo = foundCtor.GetParameters();
					Type type = paramInfo[ columnIndex ].ParameterType;

					var block = ExpressionForConstructor(
						columnIndex, ReaderTypes[ columnIndex ], ReaderNames[ columnIndex ],
						type, paramInfo[ columnIndex ].Name,
						paramExp, indexerInfo, invalidParameterExpression,
						null, options );

					ctorExps.Add( block );
				}

				exps.Add( Expression.New( foundCtor, ctorExps ) );

				info.Initializer = Expression.Lambda<Func<IDataReader, T>>(
					Expression.Block( exps ), paramExp ).Compile();
			}
			else
			{
				//if( typeof( T ) == typeof( string ) )
				//  У string нет конструктора без параметров, поэтому обрабатывается отдельно
				//	exps.Add( Expression.Assign( targetExp, Expression.Constant( string.Empty ) ) );
				//else
				if( typeof( T ).IsValueType || typeof( T ).GetConstructor( Type.EmptyTypes ) != null )
					exps.Add( Expression.Assign( targetExp, Expression.New( targetExp.Type ) ) );
				else
				{
					// Когда у класса нет дефолтного без параметров конструктора,
					// используется вызов метода FormatterServices.GetUninitializedObject
					var methodExp = typeof( System.Runtime.Serialization.FormatterServices ).GetMethod( "GetUninitializedObject" );
					var callExp = Expression.Call( methodExp, Expression.Constant( targetExp.Type ) );
					exps.Add( Expression.Assign( targetExp, Expression.Convert( callExp, targetExp.Type ) ) );
				}

				#region инициализация массивов mapped*

				bool[] mappedColumns = new bool[ readerFieldCount ];
				for( int i = 0; i < mappedColumns.Length; i++ )
				{
					mappedColumns[ i ] = false;
				}
				bool[] mappedProperties = new bool[ allProperties.Length ];
				for( int i = 0; i < mappedProperties.Length; i++ )
				{
					mappedProperties[ i ] = false;
				}
				bool[] mappedFields = new bool[ allFields.Length ];
				for( int i = 0; i < mappedFields.Length; i++ )
				{
					mappedFields[ i ] = false;
				}

				#endregion

				for( int columnIndex = 0; columnIndex < readerFieldCount; columnIndex++ )
				{
					string columnName = ReaderNames[ columnIndex ];

					if( options.IgnoresColumnNames.Any( colName => colName.Equals( columnName, StringComparison.OrdinalIgnoreCase ) ) )
					{
						// Колонка среди игнорируемых
						mappedColumns[ columnIndex ] = true;
					}
					else
					{
						for( int memberIndex = 0; memberIndex < allProperties.Length; memberIndex++ )
						{
							var member = allProperties[ memberIndex ];

							if( allPropertiesColumnNames[ memberIndex ] == ReaderNames[ columnIndex ] )
							{
								mappedColumns[ columnIndex ] = true;
								mappedProperties[ memberIndex ] = true;

								var blockExp = ExpressionForPropertyOrField(
									columnIndex, ReaderTypes[ columnIndex ], ReaderNames[ columnIndex ],
									member.PropertyType, member.Name,
									paramExp, targetExp, indexerInfo, invalidParameterExpression, options );
								exps.Add( blockExp );
								break;
							}
						}
						for( int memberIndex = 0; memberIndex < allFields.Length; memberIndex++ )
						{
							var member = allFields[ memberIndex ];

							if( allFieldsColumnNames[ memberIndex ] == ReaderNames[ columnIndex ] )
							{
								mappedColumns[ columnIndex ] = true;
								mappedFields[ memberIndex ] = true;

								var blockExp = ExpressionForPropertyOrField(
									columnIndex, ReaderTypes[ columnIndex ], ReaderNames[ columnIndex ],
									member.FieldType, member.Name,
									paramExp, targetExp, indexerInfo, invalidParameterExpression, options );
								exps.Add( blockExp );
								break;
							}
						}
					}
				}

				#region Ошибки, если есть колонки или поля/свойства без сопоставления

				if( options.ExceptionIfUnmappedReaderColumn )
				{
					StringBuilder sb = new StringBuilder();
					for( int i = 0; i < readerFieldCount; i++ )
					{
						if( !mappedColumns[ i ] )
						{
							if( sb.Length > 0 )
								sb.Append( ", " );
							sb.Append( '\'' );
							sb.Append( ReaderNames[ i ] );
							sb.Append( '\'' );
						}
					}
					if( sb.Length > 0 )
					{
						throw new DataLoaderException(
							$"Ошибка инициализации типа '{GetCSharpTypeName( typeof( T ) )}': " +
							$"В выборке из БД отсутствует отображение в поля или свойства " +
							$"заполняемого класса или структуры для следующих колонок: {sb}. " +
							$"Если какие-то колонки выборки нужно игнорировать, " +
							$"используйте {nameof( DataLoaderOptions )}.{nameof( DataLoaderOptions.IgnoresColumnNames )}." );
					}
				}
				if( options.ExceptionIfUnmappedFieldOrProperty )
				{
					StringBuilder sb = new StringBuilder();
					for( int i = 0; i < mappedProperties.Length; i++ )
					{
						if( !mappedProperties[ i ] )
						{
							if( sb.Length > 0 )
								sb.Append( ", " );
							sb.Append( '\'' );
							sb.Append( allProperties[ i ].Name );
							sb.Append( '\'' );
						}
					}
					if( sb.Length > 0 )
					{
						throw new DataLoaderException(
							$"Ошибка инициализации типа '{GetCSharpTypeName( typeof( T ) )}': " +
							$"у заполняемого класса или структуры отсутствует отображение " +
							$"на колонки выборки для следующих свойств: {sb}. " +
							$"Чтобы исключить свойство из отображения на колонку, " +
							$"используйте атрибут [Column(null)]." );
					}

					for( int i = 0; i < mappedFields.Length; i++ )
					{
						if( !mappedFields[ i ] )
						{
							if( sb.Length > 0 )
								sb.Append( ", " );
							sb.Append( '\'' );
							sb.Append( allFields[ i ].Name );
							sb.Append( '\'' );
						}
					}
					if( sb.Length > 0 )
					{
						throw new DataLoaderException(
							$"Ошибка инициализации типа '{GetCSharpTypeName( typeof( T ) )}': " +
							$"у заполняемого класса или структуры отсутствует отображение " +
							$"на колонки выборки для следующих полей: {sb}. " +
							$"Чтобы исключить поле из отображения на колонку, " +
							$"используйте атрибут [Column(null)]." );
					}
				}

				#endregion

				exps.Add( targetExp );
				info.Initializer = Expression.Lambda<Func<IDataReader, T>>(
					Expression.Block( new[] { targetExp }, exps ), paramExp ).Compile();
			}

			Sema.Wait();
			LoaderDictionary[ signature.ToString() ] = info;
			Sema.Release();

			return info;
		}

		private static Expression ExpressionForConstructor( int columnIndex,
			Type columnType, string columnName, Type argType, string argName,
			Expression paramExp, PropertyInfo indexerInfo, ConstructorInfo invalidParameterExpression,
			MethodInfo castMethod, DataLoaderOptions options )
		{
			// if it is NOT a reference type
			if( argType.IsClass == false &&
				argType.IsInterface == false )
			{
				var varExp = Expression.Variable( typeof( object ), "SQL_Column_" + columnName );
				var condDefaultExp = Expression.Condition(
						Expression.Equal( varExp, Expression.Constant( DBNull.Value ) ),
						Expression.Default( argType ),
						argType.IsValueType
							? Expression.Unbox( varExp, argType )
							: Expression.Convert( varExp, argType )
					);

				var nullCheckExp = IsNullable( argType )
					? (Expression)condDefaultExp
					: Expression.Block(
							Expression.IfThen(
							Expression.Equal(
								varExp,
								Expression.Constant( DBNull.Value )
							),
							Expression.Throw(
								Expression.New( invalidParameterExpression,
								Expression.Constant(
									$"Ошибка инициализации типа '{GetCSharpTypeName( typeof( T ) )}': " +
									( argName != null
									? $"параметр конструктора '{argName}' с типом "
									: $"тип "
									) +
									$"'{GetCSharpTypeName( argType )}' не может " +
									$"принять null из колонки '{columnName}'" ) )
								)
							),
							condDefaultExp
						);

				if( options.RemoverTrailingZerosForDecimal && argType == typeof( decimal ) )
				{
					// Если тип decimal и включена опция отсечения незначащих нулей в конце
					nullCheckExp = Expression.Divide(
						nullCheckExp,
						Expression.Constant( 1.000000000000000000000000000000000m )
						);
				}

				var indexerExp = castMethod != null
					? Expression.Assign( varExp,
							Expression.Call( castMethod,
								Expression.Convert( Expression.MakeIndex( paramExp, indexerInfo, new[] { Expression.Constant( columnIndex ) } ), columnType )
							)
						)
					: Expression.Assign( varExp,
							Expression.MakeIndex( paramExp, indexerInfo, new[] { Expression.Constant( columnIndex ) } )
						);

				var block = Expression.TryCatch(
					Expression.Block(
						new[] { varExp },
						indexerExp,
						nullCheckExp
					),
					Expression.Catch(
						typeof( InvalidCastException ),
						Expression.Block(
							Expression.Throw(
								Expression.New( typeof( DataLoaderException ).GetConstructor( new Type[] { typeof( string ) } ),
								Expression.Constant(
									$"Ошибка инициализации типа '{GetCSharpTypeName( typeof( T ) )}': " +
									$"для колонки '{columnName}' не возможно преобразовать " +
									$"из типа '{GetCSharpTypeName( columnType )}' " +
									$"в тип '{GetCSharpTypeName( argType )}'" )
								)
							),
							// Это ничего не значащий Expression, просто надо что-то вернуть, иначе при компиляции ошибка
							Expression.Default( argType )
						)
					)
				);

				return block;
			}
			else
			{
				var varExp = Expression.Variable( typeof( object ), "SQL_Column_" + columnName );

				var assignExp =
					Expression.Assign( varExp,
						Expression.MakeIndex( paramExp, indexerInfo, new[] { Expression.Constant( columnIndex ) } ) );

				if( options.RemoverTrailingZerosForDecimal && argType == typeof( decimal ) )
				{
					// Если тип decimal и включена опция отсечения незначащих нулей в конце
					assignExp = Expression.Divide(
						assignExp,
						Expression.Constant( 1.000000000000000000000000000000000m )
						);
				}

				var block = Expression.TryCatch(
					Expression.Block(
						new[] { varExp },
						assignExp,
						Expression.Condition(
							Expression.Equal( varExp, Expression.Constant( DBNull.Value ) ),
							Expression.Default( argType ),
							Expression.Convert( varExp, argType )
						)
					),
					Expression.Catch(
						typeof( InvalidCastException ),
						Expression.Block(
							Expression.Throw(
								Expression.New( typeof( DataLoaderException ).GetConstructor( new Type[] { typeof( string ) } ),
								Expression.Constant(
									$"Ошибка инициализации типа '{GetCSharpTypeName( typeof( T ) )}': " +
									$"для колонки '{columnName}' не возможно преобразовать " +
									$"из типа '{GetCSharpTypeName( columnType )}' " +
									$"в тип '{GetCSharpTypeName( argType )}'" )
								)
							),
							// Это ничего не значащий Expression.Constant, просто надо что-то вернуть, иначе при компиляции ошибка
							Expression.Default( argType )
						)
					)
				);
				return block;
			}
		}

		private static Expression ExpressionForPropertyOrField( int columnIndex,
			Type columnType, string columnName, Type memberType, string memberName,
			Expression paramExp, Expression targetExp, PropertyInfo indexerInfo,
			ConstructorInfo invalidParameterExpression, DataLoaderOptions options )
		{
			var columnIndexExp = Expression.Constant( columnIndex );
			var indexerExp = Expression.MakeIndex( paramExp, indexerInfo, new[] { columnIndexExp } );
			var varExp = Expression.Variable( typeof( object ), memberName );
			Expression block;

			Expression sourceWithNullCheckExp =
				Expression.Condition(
					Expression.Equal( varExp, Expression.Constant( DBNull.Value ) ),
					Expression.Default( columnType ),
					Expression.Convert( varExp, columnType )
				);

			if( options.RemoverTrailingZerosForDecimal && columnType == typeof( decimal ) )
			{
				// Если тип decimal и включена опция отсечения незначащих нулей в конце
				sourceWithNullCheckExp = Expression.Divide(
					sourceWithNullCheckExp,
					Expression.Constant( 1.000000000000000000000000000000000m )
					);
			}

			#region Если массив и есть метод преобразования из string в массив нужного типа

			if( memberType.IsArray && columnType == typeof( string ) )
			{
				Expression varCallExp = null;
				// Если членом класса/структуры является массив, то ищем метод,
				// принимающий xml в параметре типа string и возвращающий массив элементов типа, как у члена
				Type elementOfArrayType = memberType.GetElementType();
				foreach( MethodInfo mi in elementOfArrayType.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static ) )
				{
					// Тип, являющийся элементом массива должен иметь статический метод,
					// возвращающий массив нужного типа и принимающий ровно 1 параметр типа string
					ParameterInfo[] pi = mi.GetParameters();
					if( mi.IsStatic &&
						mi.ReturnType.IsArray &&
						mi.ReturnType.GetElementType() == elementOfArrayType &&
						pi != null &&
						pi.Length == 1 &&
						pi[ 0 ].ParameterType == columnType )
					{
						// Если метод преобразования типа найден, делается замена varNullCheckExp со встраиванием вызова
						varCallExp = Expression.Call( mi, sourceWithNullCheckExp );
					}
				}
				if( varCallExp == null )
				{
					throw new DataLoaderException( $"Ошибка инициализации типа '{GetCSharpTypeName( typeof( T ) )}': " +
						$"поле или свойство '{memberName}' имеет тип {memberType.Name}, " +
						$"но в типе {memberType.GetElementType().Name} отсутствует метод " +
						$"private|public static {memberType.Name} AnyNameYouLike(string anyParamName), " +
						$"который должен пробразовывать из строки xml в массив элементов." );
				}

				block =
					Expression.Block(
							new[] { varExp },
							Expression.Assign( varExp, indexerExp ),
							Expression.Assign( Expression.PropertyOrField( targetExp, memberName ), varCallExp )
						);

			}
			else

			#endregion
			#region Если сложный тип, например geometry, ищем статический метод преобразования

			if( !memberType.IsArray &&
				!memberType.IsEnum &&
				( Nullable.GetUnderlyingType( columnType ) ?? columnType ) != ( Nullable.GetUnderlyingType( memberType ) ?? memberType ) )
			{
				Expression varCallExp = null;
				// Если членом класса/структуры является массив, то ищем метод,
				// принимающий xml в параметре типа string и возвращающий массив элементов типа, как у члена
				foreach( MethodInfo mi in memberType.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static ) )
				{
					// Тип, являющийся элементом массива должен иметь статический метод,
					// возвращающий массив нужного типа и принимающий ровно 1 параметр типа string
					ParameterInfo[] pi = mi.GetParameters();
					if( mi.IsStatic &&
						mi.ReturnType == memberType &&
						pi != null &&
						pi.Length == 1 &&
						pi[ 0 ].ParameterType == columnType )
					{
						// Если метод преобразования типа найден, делается замена varNullCheckExp со встраиванием вызова
						varCallExp = Expression.Call( mi, sourceWithNullCheckExp );
					}
				}
				if( varCallExp == null )
				{
					throw new DataLoaderException( $"Ошибка инициализации типа '{GetCSharpTypeName( typeof( T ) )}': " +
						$"поле или свойство '{memberName}' имеет тип {memberType.Name}, " +
						$"но в типе {memberType.Name} отсутствует метод " +
						$"private|public static {memberType.Name} AnyNameYouLike({columnType} anyParamName), " +
						$"который должен делать преобразование типа." );
				}

				block =
					Expression.Block(
							new[] { varExp },
							Expression.Assign( varExp, indexerExp ),
							Expression.Assign( Expression.PropertyOrField( targetExp, memberName ), varCallExp )
						);

			}
			else

			#endregion
			{
				/*
				 * Если есть метод преобразования из string в тип члена класса (выше),
				 * то ничего дополнительно не нужно,
				 * но если метода нет, то надо делать проверку на null
				 */

				Expression unboxOrConvertExp =
					memberType.IsValueType
						? Expression.Unbox( varExp, memberType )
						: Expression.Convert( varExp, memberType );

				if( options.RemoverTrailingZerosForDecimal && columnType == typeof( decimal ) )
				{
					// Если тип decimal и включена опция отсечения незначащих нулей в конце
					unboxOrConvertExp = Expression.Divide(
						unboxOrConvertExp,
						Expression.Constant( 1.000000000000000000000000000000000m )
						);
				}


				if( IsNullable( memberType ) )
					block = Expression.Block(
						new[] { varExp },
						Expression.Assign( varExp, indexerExp ),
						Expression.Assign( Expression.PropertyOrField( targetExp, memberName ),
							Expression.Condition(
								Expression.Equal( varExp, Expression.Constant( DBNull.Value ) ),
								Expression.Default( memberType ),
								unboxOrConvertExp
							)
						)
					);
				else
					block = Expression.Block(
						new[] { varExp },
						Expression.Assign( varExp, indexerExp ),
						Expression.IfThen(
							Expression.Equal(
								varExp,
								Expression.Constant( DBNull.Value )
							),
							Expression.Throw(
								Expression.New( invalidParameterExpression,
								Expression.Constant(
									$"Ошибка инициализации типа '{GetCSharpTypeName( typeof( T ) )}': " +
									$"field '{memberName}' " +
									$"с типом '{GetCSharpTypeName( memberType )}' не может " +
									$"принять null из колонки '{columnName}'" ) )
								)
							),
						Expression.Assign( Expression.PropertyOrField( targetExp, memberName ), unboxOrConvertExp )
					);
			}

			var exp = Expression.TryCatch(
				block,
				Expression.Catch(
					typeof( InvalidCastException ),
					Expression.Block(
						Expression.Throw(
							Expression.New( typeof( DataLoaderException ).GetConstructor( new Type[] { typeof( string ) } ),
							Expression.Constant(
								$"Ошибка инициализации типа '{GetCSharpTypeName( typeof( T ) )}': " +
								$"значение типа '{GetCSharpTypeName( columnType )}' " +
								$"из колонки '{columnName}' не возможно преобразовать " +
								$"в тип '{GetCSharpTypeName( memberType )}' поля или свойства '{memberType.Name}'" )
							)
						),
						// Это ничего не значащий Expression, просто надо что - то вернуть, иначе при компиляции ошибка
						Expression.Default( memberType )
					)
				)
			);

			return exp;
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

		private static bool UseMember( ColumnAttribute attr )
		{
			// Если при члене класса не задан атрибут, то член используется в выборке
			if( attr == null )
				return true;
			// Если при члене класса задан атрибут, но маппинг пустой, то член не используется в выборке
			if( string.IsNullOrWhiteSpace( attr.Name ) )
				return false;
			// Если при члене класса задан атрибут, маппинг не пустой, то член используется в выборке
			return true;
		}
	}
}

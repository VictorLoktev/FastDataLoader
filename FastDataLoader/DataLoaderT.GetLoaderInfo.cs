using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FastDataLoader
{
	public partial class DataLoader<T>
	{
		private static LoaderInfo GetLoaderInfo( IDataReader reader, DataLoaderOptions options )
		{
			// Кол-во полей в выборке
			int readerFieldCount = reader.FieldCount;

			string[] ReaderNames = new string[ readerFieldCount ];
			Type[] ReaderTypes = new Type[ readerFieldCount ];
			for( int i = 0; i < readerFieldCount; i++ )
			{
				ReaderTypes[ i ] = reader.GetFieldType( i );
				ReaderNames[ i ] = reader.GetName( i );
			}

			LoaderInfo info = new LoaderInfo();

			/* Источники:
			 * https://stackoverflow.com/questions/19841120/generic-dbdatareader-to-listt-mapping
			 * https://stackoverflow.com/questions/20427561/checking-for-nulls-on-db-record-mapping
			 * https://stackoverflow.com/questions/390578/creating-instance-of-type-without-default-constructor-in-c-sharp-using-reflectio
			 * https://stackoverflow.com/questions/31880663/dynamically-create-delegate-for-ctor
			 * https://stackoverflow.com/questions/16678966/expression-for-read-from-property-and-write-to-private-readonly-field
			 */


			PropertyInfo[] allProperties = typeof( T )
				.GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance )
				.Where( x => TakeMemberIntoAccount( x.GetCustomAttributes<ColumnAttribute>().FirstOrDefault() ) )
				.Where( x => x.CanWrite )
				.ToArray();

			FieldInfo[] allFields = typeof( T )
				.GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance )
				.Where( x => TakeMemberIntoAccount( x.GetCustomAttributes<ColumnAttribute>().FirstOrDefault() ) )
				//-------
				// Поля автоматически созданные из свойств
				//.Where( x => !x.Name.Contains( "BackingField" ) )
				.Where( f => f.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>() == null )
				//-------
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
				{
					allPropertiesColumnNames[ i ] = property.Name;
				}
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

			// Считаем кол-во конструкторов с двумя и более параметрами
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

					// Если тип параметра конструктора - Enum, то надо сопоставить его с читаемым из БД int
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

			MethodInfo typeConvertMethod = null;

			if( readerFieldCount == 1 &&
				( Nullable.GetUnderlyingType( typeof( T ) ) ?? typeof( T ) )
				!= ( Nullable.GetUnderlyingType( ReaderTypes[ 0 ] ) ?? ReaderTypes[ 0 ] ) )
			{
				Type t1 = ( Nullable.GetUnderlyingType( ReaderTypes[ 0 ] ) ?? ReaderTypes[ 0 ] );
				Type t2 = typeof( Nullable<> ).MakeGenericType( t1 );

				foreach( MethodInfo mi in typeof( T ).GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static ) )
				{
					// Тип, являющийся элементом массива должен иметь статический метод,
					// возвращающий массив нужного типа и принимающий ровно 1 параметр типа string
					ParameterInfo[] pi = mi.GetParameters();
					if( mi.IsStatic &&
						mi.ReturnType == typeof( T ) &&
						pi != null &&
						pi.Length == 1 &&
						( pi[ 0 ].ParameterType == t1 || pi[ 0 ].ParameterType == t2 ) )
					{
						typeConvertMethod = mi;
					}
				}
			}

			#endregion

			/*
			 * Когда в выборке одно поле, принимается как одно значение
			 * и тип простой, то нам не нужны конструкторы
			 * или иные присвоения - IDataReader и так возвращает значение,
			 * нам лишь нужно взять значение из IDataReader[0] и вернуть его,
			 * предварительно заменив DBNull.Value на null.
			 * Все это не относится и к строке, т.к. string - это class.
			 * У строки и у Guid'а нет конструктора с соответствующим типом.
			 * У int? нет конструктора с аргументом, принимающим null.
			 */
			bool directCopy = readerFieldCount == 1 &&
				 ( typeof( T ) == ReaderTypes[ 0 ]
				 || Nullable.GetUnderlyingType( typeof( T ) ) == ReaderTypes[ 0 ]
				 || typeof( T ).IsEnum && ReaderTypes[ 0 ] == typeof( int )
				 || typeof( T ) != ReaderTypes[ 0 ] && typeConvertMethod != null
				 );


			if( !directCopy &&
				!IsSimple( typeof( T ) ) &&
				foundCtor == null &&
				manyParametersCtor > 0 )
			{
				/* Конструкторы без параметров или с одним параметром не учитываются,
				 * т.к. такие классы/структуры загружаются без конструкторов,
				 * простым перекладыванием из IDataReader в целевое место.
				 */

				StringBuilder sb = new StringBuilder();
				foreach( var type in ReaderTypes )
				{
					if( sb.Length > 0 )
						sb.Append( ", " );
					sb.Append( GetCSharpTypeName( type ) );
				}
				throw new FastDataLoaderException(
					$"Ошибка инициализации типа '{GetCSharpTypeName( typeof( T ) )}': " +
					$"класс или структура имеет конструктор(ы), но ни один из них не подходит к загружаемым из БД данным. " +
					$"Проверьте типы и порядок колонок в выборке и типы и порядок аргументов конструктора. " +
					$"Ожидаемые типы: {sb}."
					);
			}

			var expList = new List<Expression>();
			var dataRecordInstance = Expression.Parameter( typeof( IDataRecord ), "param" );
			var errorHolderInstance = Expression.Parameter( typeof( ErrorHolder ), "param" );
			var targetInstance = Expression.Variable( typeof( T ) );
			MethodInfo decimalDiv = typeof( DataLoader<T> ).GetMethod( nameof( DataLoader<T>.TrimZerosDecimal ) );
			MethodInfo nullableDecimalDiv = typeof( DataLoader<T> ).GetMethod( nameof( DataLoader<T>.TrimZerosNullableDecimal ) );
			DataReaderInfo readerInfo = new DataReaderInfo( reader );


			if( directCopy )
			{
				int columnIndex = 0;
				Type ctorParameterType = typeof( T );
				string ctorParameterName = null;

				var block = GetValueFromReaderExpression(
					columnIndex, ReaderTypes[ columnIndex ], ReaderNames[ columnIndex ],
					ctorParameterType, ctorParameterName,
					dataRecordInstance, errorHolderInstance, info, readerInfo, options,
					nullableDecimalDiv,
					decimalDiv );

				expList.Add( block );

				info.Initializer = Expression.Lambda<Func<IDataReader, ErrorHolder, T>>(
					Expression.Block( expList ), dataRecordInstance, errorHolderInstance ).Compile();
			}
			else
			if( foundCtor != null )
			{
				var ctorExps = new List<Expression>();
				ParameterInfo[] paramInfo = foundCtor.GetParameters();

				for( int columnIndex = 0; columnIndex < readerFieldCount; columnIndex++ )
				{
					Type ctorParameterType = paramInfo[ columnIndex ].ParameterType;
					string ctorParameterName = paramInfo[ columnIndex ].Name;

					var block = GetValueFromReaderExpression(
						columnIndex, ReaderTypes[ columnIndex ], ReaderNames[ columnIndex ],
						ctorParameterType, ctorParameterName,
						dataRecordInstance, errorHolderInstance, info, readerInfo, options,
						nullableDecimalDiv,
						decimalDiv );

					ctorExps.Add( block );
				}

				expList.Add( Expression.New( foundCtor, ctorExps ) );

				info.Initializer = Expression.Lambda<Func<IDataReader, ErrorHolder, T>>(
					Expression.Block( expList ), dataRecordInstance, errorHolderInstance ).Compile();
			}
			else
			{
				#region Инициализация массивов mapped*

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

				//if( typeof( T ) == typeof( string ) )
				//  У string нет конструктора без параметров, поэтому обрабатывается отдельно
				//	exps.Add( Expression.Assign( targetExp, Expression.Constant( string.Empty ) ) );
				//else
				if( typeof( T ).IsValueType || typeof( T ).GetConstructor( Type.EmptyTypes ) != null )
					expList.Add( Expression.Assign( targetInstance, Expression.New( targetInstance.Type ) ) );
				else
				{
					// Когда у класса нет конструктора без параметров,
					// используется вызов метода FormatterServices.GetUninitializedObject
					var methodExp = typeof( System.Runtime.Serialization.FormatterServices ).GetMethod( "GetUninitializedObject" );
					var callExp = Expression.Call( methodExp, Expression.Constant( targetInstance.Type ) );
					expList.Add( Expression.Assign( targetInstance, Expression.Convert( callExp, targetInstance.Type ) ) );
				}

				for( int columnIndex = 0; columnIndex < readerFieldCount; columnIndex++ )
				{
					string columnName = ReaderNames[ columnIndex ];

					if( options.IgnoreColumnNames.Any( colName => colName.Equals( columnName, StringComparison.OrdinalIgnoreCase ) ) )
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

								var blockExp = GetValueFromReaderExpression(
									columnIndex, ReaderTypes[ columnIndex ], ReaderNames[ columnIndex ],
									member.PropertyType, member.Name,
									dataRecordInstance, errorHolderInstance, info, readerInfo, options, nullableDecimalDiv,
									decimalDiv );

								// Регистрируем ошибку
								string errorText =
									$"Ошибка инициализации типа '{GetCSharpTypeName( typeof( T ) )}': " +
									$"значение типа '{GetCSharpTypeName( ReaderTypes[ columnIndex ] )}' " +
									$"из колонки '{ReaderNames[ columnIndex ]}' не возможно преобразовать " +
									$"в тип '{GetCSharpTypeName( member.PropertyType )}' свойства '{member.Name}'";
								info.ErrorRegister.Add( errorText );
								int errIndex = info.ErrorRegister.Count - 1;
								blockExp =
									Expression.Block(
										Expression.Assign( Expression.Field( errorHolderInstance, "ErrorPoiter" ),
											Expression.Constant( errIndex ) ),
										Expression.Assign( Expression.Property( targetInstance, member.Name ), blockExp )
									);

								//blockExp = GetMemberAssignExpression( blockExp,
								//	ReaderTypes[ columnIndex ], ReaderNames[ columnIndex ],
								//	member.PropertyType, member.Name, targetInstance, errorHolderInstance, errIndex );

								expList.Add( blockExp );
								break;
							}
						}
						for( int memberIndex = 0; memberIndex < allFields.Length; memberIndex++ )
						{
							var member = allFields[ memberIndex ];

							if( allFieldsColumnNames[ memberIndex ] == ReaderNames[ columnIndex ] )
							{
								if( member.IsInitOnly )
									throw new FastDataLoaderException(
										$"Ошибка инициализации типа '{GetCSharpTypeName( typeof( T ) )}': " +
										$"Поле '{member.Name}' помечено как readonly, используйте конструктор для заполнения" );

								mappedColumns[ columnIndex ] = true;
								mappedFields[ memberIndex ] = true;

								var blockExp = GetValueFromReaderExpression(
									columnIndex, ReaderTypes[ columnIndex ], ReaderNames[ columnIndex ],
									member.FieldType, member.Name,
									dataRecordInstance, errorHolderInstance, info, readerInfo, options, nullableDecimalDiv,
									decimalDiv );

								// Регистрируем ошибку
								string errorText =
									$"Ошибка инициализации типа '{GetCSharpTypeName( typeof( T ) )}': " +
									$"значение типа '{GetCSharpTypeName( ReaderTypes[ columnIndex ] )}' " +
									$"из колонки '{ReaderNames[ columnIndex ]}' не возможно преобразовать " +
									$"в тип '{GetCSharpTypeName( member.FieldType )}' поля '{member.Name}'";
								info.ErrorRegister.Add( errorText );
								int errIndex = info.ErrorRegister.Count - 1;
								blockExp =
									Expression.Block(
										Expression.Assign( Expression.Field( errorHolderInstance, "ErrorPoiter" ),
											Expression.Constant( errIndex ) ),
										Expression.Assign( Expression.Field( targetInstance, member.Name ), blockExp )
									);

								//blockExp = GetMemberAssignExpression( blockExp,
								//	ReaderTypes[ columnIndex ], ReaderNames[ columnIndex ],
								//	member.FieldType, member.Name, targetInstance, errorHolderInstance, errIndex );

								expList.Add( blockExp );
								break;
							}
						}
					}
				}

				#region Ошибки, если есть колонки или поля/свойства без сопоставления

				StringBuilder unusedColumns = new StringBuilder();
				StringBuilder unusedMembers = new StringBuilder();

				if( options.ExceptionIfUnmappedReaderColumn )
				{
					for( int i = 0; i < readerFieldCount; i++ )
					{
						if( !mappedColumns[ i ] )
						{
							if( unusedColumns.Length > 0 )
								unusedColumns.Append( ", " );
							unusedColumns.Append( '\'' );
							unusedColumns.Append( ReaderNames[ i ] );
							unusedColumns.Append( '\'' );
						}
					}
				}
				if( options.ExceptionIfUnmappedFieldOrProperty )
				{
					for( int i = 0; i < mappedProperties.Length; i++ )
					{
						if( !mappedProperties[ i ] )
						{
							if( unusedMembers.Length > 0 )
								unusedMembers.Append( ", " );
							unusedMembers.Append( '\'' );
							unusedMembers.Append( allProperties[ i ].Name );
							unusedMembers.Append( '\'' );
						}
					}
					for( int i = 0; i < mappedFields.Length; i++ )
					{
						if( !mappedFields[ i ] )
						{
							if( unusedMembers.Length > 0 )
								unusedMembers.Append( ", " );
							unusedMembers.Append( '\'' );
							unusedMembers.Append( allFields[ i ].Name );
							unusedMembers.Append( '\'' );
						}
					}
				}

				if( unusedColumns.Length > 0 || unusedMembers.Length > 0 )
				{
					throw new FastDataLoaderException(
						$"Ошибка инициализации типа '{GetCSharpTypeName( typeof( T ) )}': \r\n" +
						( unusedColumns.Length > 0
						? $"Неиспользуемые (без соответствия полям/свойствам) колонки в выборке из БД: {unusedColumns}. \r\n"
						: ""
						) +
						( unusedMembers.Length > 0
						? $"Неиспользуемые (без соответствия колонкам в выборке из БД)  поля/свойства: {unusedMembers}. \r\n"
						: ""
						) +
						$"Для игнорирования колонки в выборке используйте список " +
						$"{nameof( DataLoaderOptions )}.{nameof( DataLoaderOptions.IgnoreColumnNames )}. \r\n" +
						$"Для игнорирования заполнения поля/свойства используйте атрибут [Column(null)]."
						);
				}

				#endregion

				expList.Add( targetInstance );
				info.Initializer = Expression.Lambda<Func<IDataReader, ErrorHolder, T>>(
					Expression.Block( new[] { targetInstance }, expList ), dataRecordInstance, errorHolderInstance ).Compile();
			}

			//Sema.Wait();
			////LoaderDictionary[ signature.ToString() ] = info;
			//LoaderDictionary[ hashCode.ToHashCode() ] = info;
			//Sema.Release();

			return info;
		}

		private static Expression GetValueFromReaderExpression( int columnIndex,
			Type columnType, string columnName, Type memberType, string memberName,
			Expression dataReaderInstance, Expression errorHolderInstance, LoaderInfo loaderInfo,
			DataReaderInfo readerInfo, DataLoaderOptions options,
			MethodInfo nullableDecimalDiv,
			MethodInfo decimalDiv )
		{
			Type argType = memberType;
			string errorText =
				$"Ошибка инициализации типа '{GetCSharpTypeName( typeof( T ) )}': " +
				$"поле или свойство '{memberName}', " +
				$"имеющее тип '{GetCSharpTypeName( memberType )}', не может " +
				$"принять null из колонки '{columnName}'";

			MethodInfo stringToArrayMethod = null;
			MethodInfo typeConvertMethod = null;

			#region Если массив и есть метод преобразования из string в массив нужного типа

			if( memberType.IsArray && columnType == typeof( string ) )
			{
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
						stringToArrayMethod = mi;
					}
				}
				if( stringToArrayMethod == null )
				{
					throw new FastDataLoaderException(
						$"Ошибка инициализации типа '{GetCSharpTypeName( typeof( T ) )}': " +
						$"поле или свойство '{memberName}' имеет тип {GetCSharpTypeName( memberType )}, " +
						$"но в типе {GetCSharpTypeName( memberType.GetElementType() )} отсутствует метод " +
						$"private|public static {GetCSharpTypeName( memberType )} AnyNameYouLike(string anyParamName), " +
						$"который должен преобразовывать из строки xml в массив элементов." );
				}

				argType = typeof( string );
				errorText =
					$"Ошибка инициализации типа '{GetCSharpTypeName( typeof( T ) )}': " +
					$"метод {stringToArrayMethod.Name}, преобразующий из строки, содержащей xml, " +
					$"в массив типов {GetCSharpTypeName( memberType.GetElementType() )} " +
					$"для поля, свойства или аргумента '{memberName}', не может " +
					$"принять null из колонки '{columnName}'";
			}
			else

			#endregion
			#region Если сложный тип, например geometry, ищем статический метод преобразования

			if( !memberType.IsArray &&
				!memberType.IsEnum &&
				( Nullable.GetUnderlyingType( columnType ) ?? columnType ) != ( Nullable.GetUnderlyingType( memberType ) ?? memberType ) )
			{
				Type t1 = ( Nullable.GetUnderlyingType( columnType ) ?? columnType );
				Type t2 = typeof( Nullable<> ).MakeGenericType( t1 );

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
						pi.Length == 1 )
					{
						if( pi[ 0 ].ParameterType == t1 )
							argType = t1;
						else
						if( pi[ 0 ].ParameterType == t2 )
							argType = t2;
						else
							continue;
						typeConvertMethod = mi;
					}
				}
				if( typeConvertMethod == null )
				{
					throw new FastDataLoaderException( $"Ошибка инициализации типа '{GetCSharpTypeName( typeof( T ) )}': " +
						$"поле или свойство '{memberName}' имеет тип {GetCSharpTypeName( memberType )}, " +
						$"но в декларации типа {GetCSharpTypeName( memberType )} отсутствует метод " +
						$"private|public static {GetCSharpTypeName( memberType )} AnyNameYouLike({columnType} anyParamName), " +
						$"который должен делать преобразование типа." );
				}
				errorText =
					$"Ошибка инициализации типа '{GetCSharpTypeName( typeof( T ) )}': " +
					$"метод {typeConvertMethod.Name}, " +
					$"преобразующий из типа {GetCSharpTypeName( columnType )} " +
					$"в тип {GetCSharpTypeName( memberType )} " +
					$"для поля, свойства или аргумента '{memberName}', не может " +
					$"принять null из колонки '{columnName}'";
			}

			#endregion

			#region Получение значения из IDatReader

			Expression callToReader;
			if( columnType == typeof( int ) )
				callToReader = Expression.Call( dataReaderInstance, readerInfo.GetInt32, Expression.Constant( columnIndex ) );
			else
			if( columnType == typeof( short ) )
				callToReader = Expression.Call( dataReaderInstance, readerInfo.GetInt16, Expression.Constant( columnIndex ) );
			else
			if( columnType == typeof( long ) )
				callToReader = Expression.Call( dataReaderInstance, readerInfo.GetInt64, Expression.Constant( columnIndex ) );
			else
			if( columnType == typeof( string ) )
				callToReader = Expression.Call( dataReaderInstance, readerInfo.GetString, Expression.Constant( columnIndex ) );
			else
			if( columnType == typeof( bool ) )
				callToReader = Expression.Call( dataReaderInstance, readerInfo.GetBoolean, Expression.Constant( columnIndex ) );
			else
			if( columnType == typeof( decimal ) )
			{
				callToReader = Expression.Call( dataReaderInstance, readerInfo.GetDecimal, Expression.Constant( columnIndex ) );

				// Если у decimal надо отбросить незначащие нули
				if( options.RemoveTrailingZerosForDecimal )
				{
					// Если тип decimal и включена опция отсечения незначащих нулей в конце
					if( argType == typeof( decimal ) )
					{
						// Так делать нельзя, т.к. в некоторых проектах такое не срабатывает, не понятно почему, см. комментарий при методе TrimZerosDecimal
						// !! argTypedValue = Expression.Divide( typedVariable, Expression.Constant( 1.000000000000000000000000000000000m ) );
						callToReader = Expression.Call( decimalDiv, callToReader );
					}
					else
					if( argType == typeof( decimal? ) )
					{
						// Так делать нельзя, т.к. в некоторых проектах такое не срабатывает, не понятно почему, см. комментарий при методе TrimZerosDecimal
						// !! argTypedValue = Expression.Convert(
						// Expression.Divide(
						//					Expression.Property( typedVariable, "Value" ),
						//					Expression.Constant( 1.000000000000000000000000000000000m )
						//				), argType );
						callToReader = Expression.Call( nullableDecimalDiv, callToReader );
					}
				}
			}
			else
			if( columnType == typeof( DateTime ) )
				callToReader = Expression.Call( dataReaderInstance, readerInfo.GetDateTime, Expression.Constant( columnIndex ) );
			else
			if( columnType == typeof( double ) )
				callToReader = Expression.Call( dataReaderInstance, readerInfo.GetDouble, Expression.Constant( columnIndex ) );
			else
			if( columnType == typeof( float ) )
				callToReader = Expression.Call( dataReaderInstance, readerInfo.GetFloat, Expression.Constant( columnIndex ) );
			else
			if( columnType == typeof( char ) )
				callToReader = Expression.Call( dataReaderInstance, readerInfo.GetChar, Expression.Constant( columnIndex ) );
			else
			if( columnType == typeof( byte ) )
				callToReader = Expression.Call( dataReaderInstance, readerInfo.GetByte, Expression.Constant( columnIndex ) );
			else
			{
				// Если тип не из перечисленных выше, то данные достаются через тип object с преобразованием типа.
				// К таким типам относятся: byte[] и другие
				callToReader =
					Expression.Convert(
						Expression.Call( dataReaderInstance, readerInfo.GetValue, Expression.Constant( columnIndex ) ),
						argType
					);
			}

			#endregion
			#region Если есть метод преобразования, надо его вызвать

			var convertedValue =
				stringToArrayMethod != null
					? Expression.Call( stringToArrayMethod, callToReader )
					: typeConvertMethod != null
						? Expression.Call( typeConvertMethod, callToReader )
						: callToReader;

			#endregion
			#region Проверка на DBNull

			Expression preparedValue;
			Expression nullCheck = Expression.Call( dataReaderInstance, readerInfo.IsDBNull, Expression.Constant( columnIndex ) );

			if( IsNullable( argType ) )
			{
				preparedValue =
					Expression.Condition(
						nullCheck,
						Expression.Default( memberType ),
						Expression.Convert( convertedValue, memberType )
					);
			}
			else
			{
				//preparedValue =
				//	Expression.Condition(
				//		nullCheck,
				//		Expression.Block(
				//			Expression.Throw( Expression.New( invalidParameterExpression, errExp ) ),
				//			// надо что-то вернуть, иначе компиляция выдаст ошибку, т.к. тип expression иной
				//			Expression.Default( memberType )
				//		),
				//		memberType.IsEnum
				//		? Expression.Convert( convertedValue, memberType )
				//		: convertedValue
				//	);

				// Регистрируем ошибку
				loaderInfo.ErrorRegister.Add( errorText );
				int errIndex = loaderInfo.ErrorRegister.Count - 1;

				preparedValue =
						Expression.Block(
							Expression.Assign( Expression.Field( errorHolderInstance, "ErrorPoiter" ),
								Expression.Constant( errIndex ) ),
							memberType.IsEnum
							? Expression.Convert( convertedValue, memberType )
							: convertedValue
					);
			}

			#endregion

			var memberTypedVar = Expression.Variable( memberType );

			return
				Expression.Block(
					memberType,
					new[] { memberTypedVar },
					Expression.Assign(
						memberTypedVar,
						preparedValue
					),
					memberTypedVar
				);
		}
	}
}

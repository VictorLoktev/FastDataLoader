using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using FastDataLoader;

namespace PerformaceTest
{
	public class RecordCls
	{
		[Column( "object_id" )]
		public int Id { get; set; }

		[Column( "TextData" )]
		public string Text { get; set; }
	}
	public struct RecordStruct
	{
		[Column( "object_id" )]
		public int Id;

		[Column( "TextData" )]
		public string Text;
	}

	internal class CompareSpeed
	{
		public static void Run( int topN, int nTimes )
		{
			#region Подготовка

			Console.WriteLine( $"CompareSpeed: read {nTimes} times top {topN} records" );
			using SqlConnection connection = new( "Data Source=127.0.0.1;Initial Catalog=master;Integrated Security=True" );
			connection.Open();

			{
				// Prepare temp table
				using SqlCommand prepareCmd = connection.CreateCommand();
				prepareCmd.CommandTimeout = 300;
				prepareCmd.CommandText = @"
					create table #temp ( object_id int, TextData varchar(10) );

					insert into #temp (object_id, TextData)
					select	a.object_id
						,	TextData = cast( a.object_id as varchar(10) )
					from	sys.objects a, sys.objects b, sys.objects c;

					select	top " + topN.ToString() + @"
							object_id
						,	TextData
					from	#temp;
					";
				prepareCmd.ExecuteNonQuery();

			}
			using SqlCommand cmd = connection.CreateCommand();
			cmd.CommandTimeout = 300;
			cmd.CommandText = @"
				select	top " + topN.ToString() + @"
						object_id
					,	TextData
				from	#temp;
				";

			// Let's give all other programs to finish CPU and disk operations
			System.Threading.Thread.Sleep( 2000 );

			Stopwatch innerTimer = new();
			Stopwatch outerTimer = new();

			DataLoaderOptions options;

			#endregion

			{
				GC.Collect();
				System.Threading.Thread.Sleep( 1000 );
				options = new()
				{
					RemoverTrailingZerosForDecimal = false
				};

				outerTimer.Reset();
				innerTimer.Reset();

				outerTimer.Start();
				for( int i = 0; i < nTimes; i++ )
				{
					// Execute command and get reader
					TestReader readerContext = new( cmd );
					var context = readerContext.Load( options );

					// Here we have DataReader full of data from executed command

					innerTimer.Start();
					context
						.To( out List<RecordStruct> ids );
					innerTimer.Stop();

					context.End();

					readerContext.Clear();
				}
				outerTimer.Stop();
				Console.WriteLine( $"Inner timer: {innerTimer.Elapsed}. Outer timer: {outerTimer.Elapsed}. FastDataLoader. Struct." );
			}

			{
				GC.Collect();
				System.Threading.Thread.Sleep( 1000 );

				outerTimer.Reset();
				innerTimer.Reset();

				outerTimer.Start();
				for( int i = 0; i < nTimes; i++ )
				{
					var testReader = new TestReader( cmd );
					var reader = testReader.GetDataReader();

					// Here we have DataReader full of data from executed command

					List<RecordStruct> list = new();

					innerTimer.Start();
					while( reader.Read() )
					{
						RecordStruct item = new()
						{
							Id = reader.GetInt32( 0 ),
							Text = reader.GetString( 1 )
						};

						list.Add( item );
					};
					innerTimer.Stop();

					testReader.Clear();
				}
				outerTimer.Stop();
				Console.WriteLine( $"Inner timer: {innerTimer.Elapsed}. Outer timer: {outerTimer.Elapsed}. Classic reading. Struct." );
			}
		}
	}
}

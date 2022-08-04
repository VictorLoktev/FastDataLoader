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

		[Column( "TestDecimal" )]
		public decimal Dec { get; set; }

		[Column( "TextData" )]
		public string Text { get; set; }
	}
	public struct RecordStruct
	{
		[Column( "object_id" )]
		public int Id;

		[Column( "TestDecimal" )]
		public decimal Dec;

		[Column( "TextData" )]
		public string Text;
	}

	internal class CompareSpeed
	{
		public static void Run( int topN, int nTimes )
		{
			#region Подготовка

			Console.WriteLine( $"\r\nCompareSpeed: read {nTimes} times top {topN} records" );
			using SqlConnection connection = new( "Data Source=127.0.0.1;Initial Catalog=master;Integrated Security=True" );
			connection.Open();

			{
				// Prepare temp table
				using SqlCommand prepareCmd = connection.CreateCommand();
				prepareCmd.CommandTimeout = 300;
				prepareCmd.CommandText = @"
					create table #temp ( object_id int, TextData varchar(10), TestDecimal decimal(18,8) );

					insert into #temp (object_id, TextData, TestDecimal)
					select	a.object_id
						,	TextData = cast( a.object_id as varchar(10) )
						,	TestDecimal = cast( 12345.6700 as decimal(18,8) )
					from	sys.objects a, sys.objects b, sys.objects c;

					select	top " + topN.ToString() + @"
							object_id
						,	TextData
						,	TestDecimal
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
					,	TestDecimal
				from	#temp;
				";

			// Let's give all other programs to finish CPU and disk operations
			System.Threading.Thread.Sleep( 2000 );

			Stopwatch innerTimer1 = new();
			Stopwatch outerTimer1 = new();
			Stopwatch innerTimer2 = new();
			Stopwatch outerTimer2 = new();

			DataLoaderOptions options;

			#endregion

			{
				GC.Collect();
				System.Threading.Thread.Sleep( 1000 );
				options = new()
				{
					RemoveTrailingZerosForDecimal = false
				};

				outerTimer1.Reset();
				innerTimer1.Reset();

				outerTimer1.Start();
				for( int i = 0; i < nTimes; i++ )
				{
					// Execute command and get reader
					TestReader readerContext = new( cmd );
					var context = readerContext.Load( options );

					// Here we have DataReader full of data from executed command

					innerTimer1.Start();
					context
						.To( out List<RecordStruct> ids );
					innerTimer1.Stop();

					context.End();

					readerContext.Clear();
				}
				outerTimer1.Stop();
				Console.WriteLine( $"Inner timer: {innerTimer1.Elapsed}. Outer timer: {outerTimer1.Elapsed}. FastDataLoader. Struct." );
			}

			{
				GC.Collect();
				System.Threading.Thread.Sleep( 1000 );

				outerTimer2.Reset();
				innerTimer2.Reset();

				outerTimer2.Start();
				for( int i = 0; i < nTimes; i++ )
				{
					var testReader = new TestReader( cmd );
					var reader = testReader.GetDataReader();

					// Here we have DataReader full of data from executed command

					List<RecordStruct> list = new();

					innerTimer2.Start();
					while( reader.Read() )
					{
						RecordStruct item = new()
						{
							Id = reader.GetInt32( 0 ),
							Text = reader.GetString( 1 ),
							Dec = reader.GetDecimal( 2 ),
						};

						list.Add( item );
					};
					innerTimer2.Stop();

					testReader.Clear();
				}
				outerTimer2.Stop();
				Console.WriteLine( $"Inner timer: {innerTimer2.Elapsed}. Outer timer: {outerTimer2.Elapsed}. Classic reading. Struct." );
				Console.WriteLine(
					$"Inner timer: {GetPercent( innerTimer1, innerTimer2 )}%. " +
					$"Outer timer: {GetPercent( outerTimer1, outerTimer2 )}%." );
			}
		}

		private static string GetPercent( Stopwatch timer1, Stopwatch timer2 )
		{
			return Math.Round( 100M * timer1.ElapsedTicks / timer2.ElapsedTicks, 2, MidpointRounding.ToZero ).ToString( "G" );
		}
	}
}

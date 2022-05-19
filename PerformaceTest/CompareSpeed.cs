using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using FastDataLoader;

namespace PerformaceTest
{
	public class Record
	{
		[Column( "object_id" )]
		public int Id { get; set; }

		[Column( "TextData" )]
		public string Text { get; set; }
	}

	internal class CompareSpeed
	{
		public static void Run( int n )
		{
			Console.WriteLine( $"CompareSpeed: read 1'000'000 times top {n} records" );
			using SqlConnection connection = new( "Data Source=localhost;Initial Catalog=master;Integrated Security=True" );
			connection.Open();

			// Let's give all other programs to finish CPU and disk operations
			System.Threading.Thread.Sleep( 5000 );

			Stopwatch innerTimer = new();
			Stopwatch outerTimer = new();

			outerTimer.Start();
			for( int i = 0; i < 1_000_000; i++ )
			{
                using SqlCommand cmd = new();
                InitCommand( connection, cmd, n );

                TestReader reader = new( cmd );
                var context = reader.Load();

                // Here we have DataReader full of data from executed command

                innerTimer.Start();
                context
                    .To( out Record[] ids );
                innerTimer.Stop();

                // End disposes reader of sql command
                context.End();
            }
			outerTimer.Stop();

			Console.WriteLine( $"Using DataLoader: Inner timer: {innerTimer.Elapsed}. Outer timer: {outerTimer.Elapsed}." );

			outerTimer.Reset();
			innerTimer.Reset();

			outerTimer.Start();
			for( int i = 0; i < 1_000_000; i++ )
			{
                using SqlCommand cmd = new();
                InitCommand( connection, cmd, n );

                IDataReader reader = new TestReader( cmd ).GetDataReader();

                // Here we have DataReader full of data from executed command

                List<Record> list = new();

                innerTimer.Start();
                while( reader.Read() )
                {
                    Record item = new()
                    {
                        Id = reader.GetInt32( reader.GetOrdinal( "object_id" ) ),
                        Text = reader.GetString( reader.GetOrdinal( "TextData" ) )
                    };

                    list.Add( item );
                };
                innerTimer.Stop();

                reader.Dispose();
            }
            outerTimer.Stop();

			Console.WriteLine( $"Standard  method: Inner timer: {innerTimer.Elapsed}. Outer timer: {outerTimer.Elapsed}." );
			Console.WriteLine();
		}

		private static  void InitCommand( SqlConnection connection, SqlCommand command, int topN )
		{
			command.Connection = connection;
			command.CommandTimeout = 300;
			command.CommandText = @"
					select	top " + topN.ToString() + @"
							a.object_id
						,	TextData = cast( a.object_id as varchar(10) )
					from	sys.objects a"
			// Just one sys.objects table has around 100 records only. We need to clone expracted records.
			+ ( topN > 100 ? ", sys.objects b" : "" )
			;
		}
	}
}

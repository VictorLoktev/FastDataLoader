# Fast DataLoader

Load data classes/structures from SQL server easy and fast!

Replace long code of extracting every column from DataReader into fields or members
with just 2 calls to Load() and To() methods of Fast DataReader.

Fast DataReader maps columns of SQL request to fields and properties of class or structure,
builds array of elements, fills the elements with data from DataReader
and does it in maximum performance way using compiled Expressions.


It's free to use and modify.


---
[[_TOC_]]
---

## License
[Apache 2.0](LICENSE)

----

## Performance tests

Overall working time is measured by outerTimer.

innerTimer measures time of coping data from IDataReader to list/arrays.


####Fast DataLoader


`
			outerTimer.Start();
			for( int i = 0; i < 1_000_000; i++ )
			{
				using( SqlCommand cmd = new SqlCommand() )
				{
					InitCommand( connection, cmd, n );

					TestReader reader = new TestReader( cmd );
					var context = reader.Load();

					innerTimer.Start();
					context
						.To( out Record[] ids );
					innerTimer.Stop();

					context.End();
				}
			}
			outerTimer.Stop();
`

####Standard method

`
			outerTimer.Start();
			for( int i = 0; i < 1_000_000; i++ )
			{
				using( SqlCommand cmd = new SqlCommand() )
				{
					InitCommand( connection, cmd, n );

					IDataReader reader = new TestReader( cmd ).GetDataReader();

					List<Record> list = new List<Record>();

					innerTimer.Start();
					while( reader.Read() )
					{
						Record item = new Record();
						item.Id = reader.GetInt32( reader.GetOrdinal( "object_id" ) );
						item.Text = reader.GetString( reader.GetOrdinal( "TextData" ) );

						list.Add( item );
					};
					innerTimer.Stop();

					reader.Dispose();
				}
`

###CompareSpeed: read 1'000'000 times top 1 records

Using DataLoader: Inner timer: 00:00:06.9385924. Outer timer: 00:07:52.1697057.

Standard  method: Inner timer: 00:00:03.7933012. Outer timer: 00:07:45.1947366.

Using DataLoader: Inner timer: 00:00:06.2047660. Outer timer: 00:07:39.8613013.

Standard  method: Inner timer: 00:00:03.9451829. Outer timer: 00:07:45.6672652.

Using DataLoader: Inner timer: 00:00:06.3842963. Outer timer: 00:07:47.6996809.

Standard  method: Inner timer: 00:00:04.0797611. Outer timer: 00:07:45.0388535.

Using DataLoader: Inner timer: 00:00:06.6125721. Outer timer: 00:07:45.2741402.

Standard  method: Inner timer: 00:00:03.8825388. Outer timer: 00:07:41.4163803.

Using DataLoader: Inner timer: 00:00:06.2717159. Outer timer: 00:08:00.4443943.

Standard  method: Inner timer: 00:00:03.7903551. Outer timer: 00:08:00.1455061.



###CompareSpeed: read 1'000'000 times top 100 records

Using DataLoader: Inner timer: 00:00:34.9465023. Outer timer: 00:08:59.3660514.

Standard  method: Inner timer: 00:00:37.9452129. Outer timer: 00:09:03.1303205.

Using DataLoader: Inner timer: 00:00:34.1894785. Outer timer: 00:08:57.2302271.

Standard  method: Inner timer: 00:00:36.5574130. Outer timer: 00:08:59.2287383.

Using DataLoader: Inner timer: 00:00:35.3638946. Outer timer: 00:09:00.6853421.

Standard  method: Inner timer: 00:00:37.1277408. Outer timer: 00:09:02.1684993.

Using DataLoader: Inner timer: 00:00:36.1503260. Outer timer: 00:08:57.2432409.

Standard  method: Inner timer: 00:00:38.5351804. Outer timer: 00:09:01.0128626.

Using DataLoader: Inner timer: 00:00:36.5856987. Outer timer: 00:09:06.8530580.

Standard  method: Inner timer: 00:00:38.0171344. Outer timer: 00:09:10.1004365.


###CompareSpeed: read 1'000'000 times top 10000 records
Using DataLoader: Inner timer: 01:16:05.0324473. Outer timer: 01:33:24.1436326.

Standard  method: Inner timer: 01:04:26.2259246. Outer timer: 01:20:45.0832207.

Using DataLoader: Inner timer: 01:16:27.2857545. Outer timer: 01:33:47.6104038.

Standard  method: Inner timer: 01:03:26.1543051. Outer timer: 01:19:47.2364844.

Using DataLoader: Inner timer: 01:24:05.5499788. Outer timer: 01:43:14.1684023.

Standard  method: Inner timer: 01:07:53.9964862. Outer timer: 01:25:16.7780470.

Using DataLoader: Inner timer: 01:14:46.0345437. Outer timer: 01:32:00.9475822.

Standard  method: Inner timer: 01:06:01.5976622. Outer timer: 01:22:24.4597343.

Using DataLoader: Inner timer: 01:23:27.6484370. Outer timer: 01:41:55.7999793.

Standard  method: Inner timer: 01:11:00.9113613. Outer timer: 01:29:29.6347107.


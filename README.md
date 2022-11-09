# Fast DataLoader

Load data classes/structures from SQL server easy and fast!

Replace long code of extracting every column from DataReader into fields or members
with just 2 calls to Load() and To() methods of Fast DataReader.

Fast DataReader maps columns of SQL request to fields and properties of class or structure,
builds array of elements, fills the elements with data from DataReader
and does it in maximum performance way using compiled Expressions.


It's free to use and modify.

----
The assembly uses System.CodeDom messages where System.Int32 is replaced by int.

Do not forget copy System.CodeDom.dll to bin folder!

----

## License
[MIT No Attribution](LICENSE)

---

## Performance tests

Overall working time is measured by outerTimer.

The time of coping data from IDataReader to list/arrays is measured by innerTimer.


#### Fast DataLoader


```
				outerTimer1.Start();
				for( int i = 0; i < nTimes; i++ )
				{
					// Execute command and get reader
					var testReader = new TestReader( cmd );
					var context = testReader.Load( options );

					// Here we have DataReader full of data from executed command

					innerTimer1.Start();
					context
						.To( out List<RecordStruct> ids );
					innerTimer1.Stop();

					testReader.Clear();
				}
				outerTimer1.Stop();
```

#### Standard method

```
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
```

### CompareSpeed: read 1000000 times top 1 records

Inner timer: 00:00:03.9613705. Outer timer: 00:06:01.0328361. FastDataLoader. Struct.

Inner timer: 00:00:02.1351468. Outer timer: 00:05:52.4128517. Classic reading. Struct.

Inner timer: 185,53%. Outer timer: 102,44%. FastDataLoader to Classic %%.

### CompareSpeed: read 1000000 times top 100 records

Inner timer: 00:00:45.3589232. Outer timer: 00:07:35.7143343. FastDataLoader. Struct.

Inner timer: 00:00:38.9100288. Outer timer: 00:07:26.1131845. Classic reading. Struct.

Inner timer: 116,57%. Outer timer: 102,15%. FastDataLoader to Classic %%.

### CompareSpeed: read 100000 times top 10000 records

Inner timer: 00:06:29.6088946. Outer timer: 00:07:17.1933223. FastDataLoader. Struct.

Inner timer: 00:06:09.9340869. Outer timer: 00:06:56.8428905. Classic reading. Struct.

Inner timer: 105,31%. Outer timer: 104,88%. FastDataLoader to Classic %%.


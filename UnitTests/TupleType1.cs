using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastDataLoader;

namespace UnitTests
{
    [TestClass]
    public class TupleType1
    {
        [TestMethod]
        public void Array1()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 12345 as int )" +
                "   ,   B = cast( 12345 as int )" +
                "   ,   C = cast( '12345' as varchar(10) )" +
                "   ,   D = cast( null as bit )" +
                "   ,   E = cast( 123.45 as money )" +
                "   ,   F = cast( null as int )" +
                "   ,   G = cast( 0x1234 as varbinary )"
                );
            reader
                .Load()
                .To( out Tuple<int, int?, string, bool?, decimal, int?, byte[]>[] value )
                .End();

            Assert.AreEqual( 1, value.Length );
            Assert.AreEqual( 12345, value[ 0 ].Item1 );
            Assert.AreEqual( 12345, value[ 0 ].Item2 );
            Assert.AreEqual( "12345", value[ 0 ].Item3 );
            Assert.AreEqual( null, value[ 0 ].Item4 );
            Assert.AreEqual( 123.45m, value[ 0 ].Item5 );
            Assert.AreEqual( null, value[ 0 ].Item6 );
            Assert.AreEqual( 2, value[ 0 ].Item7.Length );
            Assert.AreEqual( 0x12, value[ 0 ].Item7[ 0 ] );
            Assert.AreEqual( 0x34, value[ 0 ].Item7[ 1 ] );
        }

        [TestMethod]
        public void Array2()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 12345 as int )" +
                "   ,   B = cast( 12345 as int )" +
                "   ,   C = cast( '12345' as varchar(10) )" +
                "   ,   D = cast( 1 as bit )" +
                "   ,   E = cast( 123.45 as money )" +
                "   ,   F = cast( '51CE512F-1E4F-4995-BE95-A4F7388C88A8' as uniqueidentifier )" +
                "   ,   G = cast( 0x1234 as varbinary )" +
                " union all " +
                "select	A = cast( 12345 as int )" +
                "   ,   B = cast( null as int )" +
                "   ,   C = cast( '12345' as varchar(10) )" +
                "   ,   D = cast( null as bit )" +
                "   ,   E = cast( 123.45 as money )" +
                "   ,   F = cast( null as uniqueidentifier )" +
                "   ,   G = cast( null as varbinary )"
                );
            reader
                .Load()
                .To( out Tuple<int, int?, string, bool?, decimal, Guid?, byte[]>[] value )
                .End();

            Assert.AreEqual( 2, value.Length );

            Assert.AreEqual( 12345, value[ 0 ].Item1 );
            Assert.AreEqual( 12345, value[ 0 ].Item2 );
            Assert.AreEqual( "12345", value[ 0 ].Item3 );
            Assert.AreEqual( true, value[ 0 ].Item4 );
            Assert.AreEqual( 123.45m, value[ 0 ].Item5 );
            Assert.AreEqual( new Guid( "51CE512F-1E4F-4995-BE95-A4F7388C88A8" ), value[ 0 ].Item6 );
            Assert.AreEqual( 2, value[ 0 ].Item7.Length );
            Assert.AreEqual( 0x12, value[ 0 ].Item7[0] );
            Assert.AreEqual( 0x34, value[ 0 ].Item7[1] );

            Assert.AreEqual( 12345, value[ 1 ].Item1 );
            Assert.AreEqual( null, value[ 1 ].Item2 );
            Assert.AreEqual( "12345", value[ 1 ].Item3 );
            Assert.AreEqual( null, value[ 1 ].Item4 );
            Assert.AreEqual( 123.45m, value[ 1 ].Item5 );
            Assert.AreEqual( null, value[ 1 ].Item6 );
            Assert.AreEqual( null, value[ 1 ].Item7 );
        }

        [TestMethod]
        public void Array0()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 12345 as int )" +
                "   ,   B = cast( 12345 as int )" +
                "   ,   C = cast( '12345' as varchar(10) )" +
                "   ,   D = cast( null as bit )" +
                "   ,   E = cast( 123.45 as money )" +
                "   ,   F = cast( '51CE512F-1E4F-4995-BE95-A4F7388C88A8' as uniqueidentifier )" +
                "   ,   G = cast( 0x1234 as varbinary )" +
                "where 1=0"
                );
            reader
                .Load()
                .To( out Tuple<int, int?, string, bool, decimal, Guid?, byte[]>[] value )
                .End();

            Assert.AreEqual( 0, value.Length );
        }

        [TestMethod]
        public void Tuple1()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 12345 as int )" +
                "   ,   B = cast( 12345 as int )" +
                "   ,   C = cast( '12345' as varchar(10) )" +
                "   ,   D = cast( null as bit )" +
                "   ,   E = cast( 123.45 as money )" +
                "   ,   F = cast( '51CE512F-1E4F-4995-BE95-A4F7388C88A8' as uniqueidentifier )" +
                "   ,   G = cast( 0x1234 as varbinary )"
                );

            var value = reader
                .Load1<Tuple<int, int?, string, bool?, decimal, Guid?, byte[]>>();

            Assert.AreEqual( 12345, value.Item1 );
            Assert.AreEqual( 12345, value.Item2 );
            Assert.AreEqual( "12345", value.Item3 );
            Assert.AreEqual( null, value.Item4 );
            Assert.AreEqual( 123.45m, value.Item5 );
            Assert.AreEqual( new Guid( "51CE512F-1E4F-4995-BE95-A4F7388C88A8" ), value.Item6 );
            Assert.AreEqual( 2, value.Item7.Length );
            Assert.AreEqual( 0x12, value.Item7[ 0 ] );
            Assert.AreEqual( 0x34, value.Item7[ 1 ] );
        }

        [TestMethod]
        public void Error0()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 12345 as int )" +
                "   ,   B = cast( 12345 as int )" +
                "   ,   C = cast( '12345' as varchar(10) )" +
                "   ,   D = cast( null as bit )" +
                "   ,   E = cast( 123.45 as money )" +
                "   ,   F = cast( '51CE512F-1E4F-4995-BE95-A4F7388C88A8' as uniqueidentifier )" +
                "   ,   G = cast( 0x1234 as varbinary )" +
                "where 1=0"
                );

            try
            {
                var value = reader
                    .Load1<Tuple<int, int?, string, bool, decimal, Guid?, byte[]>>();

                Assert.Fail();
            }
            catch( DataLoaderException )
            {
            }
        }

        [TestMethod]
        public void IntError2()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 12345 as int )" +
                "   ,   B = cast( 12345 as int )" +
                "   ,   C = cast( '12345' as varchar(10) )" +
                "   ,   D = cast( null as bit )" +
                "   ,   E = cast( 123.45 as money )" +
                "   ,   F = cast( '51CE512F-1E4F-4995-BE95-A4F7388C88A8' as uniqueidentifier )" +
                "   ,   G = cast( 0x1234 as varbinary )" +
                " union all " +
                "select	A = cast( 12345 as int )" +
                "   ,   B = cast( null as int )" +
                "   ,   C = cast( '12345' as varchar(10) )" +
                "   ,   D = cast( null as bit )" +
                "   ,   E = cast( 123.45 as money )" +
                "   ,   F = cast( null as uniqueidentifier )" +
                "   ,   G = cast( null as varbinary )"
                );

            try
            {
                var value = reader
                    .Load1<Tuple<int, int?, string, bool, decimal, Guid?, byte[]>>();

                Assert.Fail();
            }
            catch( DataLoaderException )
            {
            }
        }

        [TestMethod]
        public void IntErrorNull()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 12345 as int )" +
                "   ,   B = cast( 12345 as int )" +
                "   ,   C = cast( '12345' as varchar(10) )" +
                "   ,   D = cast( null as bit )" +
                "   ,   E = cast( 123.45 as money )" +
                "   ,   F = cast( '51CE512F-1E4F-4995-BE95-A4F7388C88A8' as uniqueidentifier )" +
                "   ,   G = cast( 0x1234 as varbinary )"
                );

            try
            {
                var value = reader
                    .Load1<Tuple<int, int?, string, bool, decimal, Guid?, byte[]>>();

                Assert.Fail();
            }
            catch( DataLoaderException )
            {
            }
        }

        [TestMethod]
        public void IntErrorArrayNull()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 12345 as int )" +
                "   ,   B = cast( 12345 as int )" +
                "   ,   C = cast( '12345' as varchar(10) )" +
                "   ,   D = cast( null as bit )" +
                "   ,   E = cast( 123.45 as money )" +
                "   ,   F = cast( '51CE512F-1E4F-4995-BE95-A4F7388C88A8' as uniqueidentifier )" +
                "   ,   G = cast( 0x1234 as varbinary )" +
                " union all " +
                "select	A = cast( 12345 as int )" +
                "   ,   B = cast( null as int )" +
                "   ,   C = cast( '12345' as varchar(10) )" +
                "   ,   D = cast( null as bit )" +
                "   ,   E = cast( 123.45 as money )" +
                "   ,   F = cast( null as uniqueidentifier )" +
                "   ,   G = cast( null as varbinary )"
                );
            try
            {
                reader
                    .Load()
                .To( out Tuple<int, int, string, bool, decimal, Guid, byte[]>[] value )
                .End();

                Assert.Fail();
            }
            catch( DataLoaderException )
            {
            }
        }

    }
}

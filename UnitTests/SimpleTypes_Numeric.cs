using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastDataLoader;

namespace UnitTests
{
    [TestClass]
    public class SimpleTypes_Numeric
    {
        public struct Record
        {
            public decimal A;

            public string B;
        }


        [TestMethod]
        public void DecimalArray1()
        {
            using DbReader reader = new(
                "select	A = cast( 12345.67 as numeric(18,8) )"
                );
            reader
                .Load()
                .To( out decimal[] value )
                .End();

            Assert.AreEqual( 1, value.Length );
            Assert.AreEqual( 12345.67m, value[ 0 ] );
        }

        [TestMethod]
        public void DecimalArray2()
        {
            using DbReader reader = new(
                "select	A = cast( 12345.67 as numeric(18,8) )" +
                " union all " +
                "select A = cast( 67890.12 as numeric(18,8) )"
                );
            reader
                .Load()
                .To( out decimal[] value )
                .End();

            Assert.AreEqual( 2, value.Length );
            Assert.AreEqual( 12345.67m, value[ 0 ] );
            Assert.AreEqual( 67890.12m, value[ 1 ] );
        }

        [TestMethod]
        public void DecimalArray0()
        {
            using DbReader reader = new(
                "select	A = cast( 12345.67 as numeric(18,8) ) " +
                "where 1=0"
                );
            reader
                .Load()
                .To( out decimal[] value )
                .End();

            Assert.AreEqual( 0, value.Length );
        }

        [TestMethod]
        public void Decimal1()
        {
            using DbReader reader = new(
                "select	A = cast( 12345.67 as numeric(18,8) )"
                );

            decimal value = reader
                .Load1<decimal>();

            Assert.AreEqual( 12345.67m, value );
        }

        [TestMethod]
        public void DecimalError0()
        {
            using DbReader reader = new(
                "select	A = cast( 12345.67 as numeric(18,8) ) " +
                "where 1=0"
                );

            try
            {
                decimal value = reader
                    .Load1<decimal>();

                Assert.Fail();
            }
            catch( FastDataLoaderException )
            {
            }
        }

        [TestMethod]
        public void DecimalError2()
        {
            using DbReader reader = new(
                "select	A = cast( 12345.67 as numeric(18,8) )" +
                " union all " +
                "select	A = cast( 12345.67 as numeric(18,8) )"
                );

            try
            {
                decimal value = reader
                    .Load1<decimal>();

                Assert.Fail();
            }
            catch( FastDataLoaderException )
            {
            }
        }

        [TestMethod]
        public void DecimalErrorNull()
        {
            using DbReader reader = new(
                "select	A = cast( null as numeric(18,8) )"
                );

            try
            {
                decimal value = reader
                    .Load1<decimal>();

                Assert.Fail();
            }
            catch( FastDataLoaderException )
            {
            }
        }

        [TestMethod]
        public void DecimalErrorArrayNull()
        {
            using DbReader reader = new(
                "select	A = cast( 12345.67 as numeric(18,8) )" +
                " union all " +
                "select A = cast( null as numeric(18,8) )"
                );
            try
            {
                reader
                    .Load()
                .To( out decimal[] value )
                .End();

                Assert.Fail();
            }
            catch( FastDataLoaderException )
            {
            }
        }


        [TestMethod]
        public void TrailingZerosOn()
        {
            using DbReader reader = new(
                "select	A = cast( 12345.6700 as numeric(18,8) )"
                );

            var value = reader
                .Load1<decimal>();

            Assert.AreEqual( "12345.67", value.ToString( "G", System.Globalization.CultureInfo.GetCultureInfo( "en-US" ) ) );
        }

        [TestMethod]
        public void TrailingZerosOffDecimalNullable()
        {
            using DbReader reader = new(
                "select	A = cast( 12345.6700 as decimal(18,8) )"
                );

            var value = reader
                .Load1<decimal?>( new DataLoaderOptions() { RemoveTrailingZerosForDecimal = false } );

            Assert.AreEqual( "12345.67000000", value.Value.ToString( "G", System.Globalization.CultureInfo.GetCultureInfo( "en-US" ) ) );
        }

        [TestMethod]
        public void TrailingZerosOffDecimal()
        {
            using DbReader reader = new(
                "select	A = cast( 12345.6700 as decimal(18,8) )"
                );

            var value = reader
                .Load1<decimal>( new DataLoaderOptions() { RemoveTrailingZerosForDecimal = false } );

            Assert.AreEqual( "12345.67000000", value.ToString( "G", System.Globalization.CultureInfo.GetCultureInfo( "en-US" ) ) );
        }

        [TestMethod]
        public void TrailingZerosOff3()
        {
            using DbReader reader = new(
                "select	A = cast( 12345.6700 as decimal(18,8) )" +
				"   ,   B = '1'"
                );

            var value = reader
                .Load1<Record>( new DataLoaderOptions() { RemoveTrailingZerosForDecimal = false } );

            Assert.AreEqual( "12345.67000000", value.A.ToString( "G", System.Globalization.CultureInfo.GetCultureInfo( "en-US" ) ) );
        }


    }
}

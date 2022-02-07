using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastDataLoader;

namespace UnitTests
{
    [TestClass]
    public class SimpleTypes_NumericN
    {
        [TestMethod]
        public void DecimalArray1()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 12345.67 as numeric(18,8) )"
                );
            reader
                .Load()
                .To( out decimal?[] value )
                .End();

            Assert.AreEqual( 1, value.Length );
            Assert.AreEqual( 12345.67m, value[ 0 ] );
        }

        [TestMethod]
        public void DecimalArray2()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 12345.67 as numeric(18,8) )" +
                " union all " +
                "select A = cast( 67890.12 as numeric(18,8) )" +
                " union all " +
                "select A = cast( null as numeric(18,8) )"
                );
            reader
                .Load()
                .To( out decimal?[] value )
                .End();

            Assert.AreEqual( 3, value.Length );
            Assert.AreEqual( 12345.67m, value[ 0 ] );
            Assert.AreEqual( 67890.12m, value[ 1 ] );
            Assert.AreEqual( null, value[ 2 ] );
        }

        [TestMethod]
        public void DecimalArray0()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 12345.67 as numeric(18,8) ) " +
                "where 1=0"
                );
            reader
                .Load()
                .To( out decimal?[] value )
                .End();

            Assert.AreEqual( 0, value.Length );
        }

        [TestMethod]
        public void Decimal1()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 12345.67 as numeric(18,8) )"
                );

            decimal? value = reader
                .Load1<decimal?>();

            Assert.AreEqual( 12345.67m, value );
        }

        [TestMethod]
        public void Decimal12()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( null as numeric(18,8) )"
                );

            decimal? value = reader
                .Load1<decimal?>();

            Assert.AreEqual( null, value );
        }

        [TestMethod]
        public void DecimalError0()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 12345 as numeric(18,8) ) " +
                "where 1=0"
                );

            try
            {
                decimal value = reader
                    .Load1<decimal>();

                Assert.Fail();
            }
            catch( DataLoaderException )
            {
            }
        }

        [TestMethod]
        public void DecimalError2()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 12345 as numeric(18,8) )" +
                " union all " +
                "select	A = cast( 12345 as numeric(18,8) )"
                );

            try
            {
                decimal value = reader
                    .Load1<decimal>();

                Assert.Fail();
            }
            catch( DataLoaderException )
            {
            }
        }
    }
}

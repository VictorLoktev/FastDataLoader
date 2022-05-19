using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastDataLoader;

namespace UnitTests
{
    [TestClass]
    public class SimpleTypes_Money
    {
        [TestMethod]
        public void DecimalArray1()
        {
            using DbReader reader = new(
                "select	A = cast( 12345.67 as money )"
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
                "select	A = cast( 12345.67 as money )" +
                " union all " +
                "select A = cast( 67890.12 as money )"
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
                "select	A = cast( 12345.67 as money ) " +
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
                "select	A = cast( 12345.67 as money )"
                );

            decimal value = reader
                .Load1<decimal>();

            Assert.AreEqual( 12345.67m, value );
        }

        [TestMethod]
        public void DecimalError0()
        {
            using DbReader reader = new(
                "select	A = cast( 12345.67 as money ) " +
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
                "select	A = cast( 12345.67 as money )" +
                " union all " +
                "select	A = cast( 12345.67 as money )"
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
                "select	A = cast( null as money )"
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
                "select	A = cast( 12345.67 as money )" +
                " union all " +
                "select A = cast( null as money )"
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

    }
}

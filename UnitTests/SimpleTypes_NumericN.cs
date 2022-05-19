using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastDataLoader;

namespace UnitTests
{
    [TestClass]
    public class SimpleTypes_NumericN
    {
        class TestClass1
        {
            public decimal A;
            public decimal? B;
            public decimal C { get; private set; }
            public decimal? D { get; private set; }
        }

        [TestMethod]
        public void Decimal1TestClass1()
        {
            using DbReader reader = new(
                "select	A = cast( 12345.67000 as numeric(18,8) )" +
                "   ,   B = cast( 12345.67 as numeric(18,8) )" +
                "   ,   C = cast( 12345.67 as numeric(18,8) )" +
                "   ,   D = cast( 12345.67 as numeric(18,8) )"
                );

            var value = reader
                .Load1<TestClass1>();

            Assert.AreEqual( 12345.67m, value.A );
            Assert.AreEqual( 12345.67m, value.B );
            Assert.AreEqual( 12345.67m, value.C );
            Assert.AreEqual( 12345.67m, value.D );
        }

        [TestMethod]
        public void Decimal2TestClass1()
        {
            using DbReader reader = new(
                "select	A = cast( 12345.67 as numeric(18,8) )" +
                "   ,   B = cast( null as numeric(18,8) )" +
                "   ,   C = cast( 12345.67 as numeric(18,8) )" +
                "   ,   D = cast( null as numeric(18,8) )"
                );

            var value = reader
                .Load1<TestClass1>();

            Assert.AreEqual( 12345.67m, value.A );
            Assert.AreEqual( null, value.B );
            Assert.AreEqual( 12345.67m, value.C );
            Assert.AreEqual( null, value.D );
        }

        public class TestClass2
        {
            public decimal A;
            public decimal? B;
            public decimal C { get; private set; }
            public decimal? D { get; private set; }

            // Supress warning IDE0051 Private member 'TestClass2..ctor' is unused UnitTests in code or in project
#pragma warning disable IDE0051 // Remove unused private members
            TestClass2( decimal a, decimal? b, decimal c, decimal? d )
#pragma warning restore IDE0051 // Remove unused private members
            {
                A = a;
                B = b;
                C = c;
                D = d;
            }
        }

        [TestMethod]
        public void Decimal1TestClass2()
        {
            using DbReader reader = new(
                "select	A = cast( 12345.67 as numeric(18,8) )" +
                "   ,   B = cast( 12345.670000 as numeric(18,8) )" +
                "   ,   C = cast( 12345.67 as numeric(18,8) )" +
                "   ,   D = cast( 12345.67 as numeric(18,8) )"
                );

            var value = reader
                .Load1<TestClass2>( new DataLoaderOptions() { RemoverTrailingZerosForDecimal = true } );

            Assert.AreEqual( 12345.67m, value.A );
            Assert.AreEqual( 12345.67m, value.B );
            Assert.AreEqual( 12345.67m, value.C );
            Assert.AreEqual( 12345.67m, value.D );
        }

        [TestMethod]
        public void Decimal2TestClass2()
        {
            using DbReader reader = new(
                "select	A = cast( 12345.67 as numeric(18,8) )" +
                "   ,   B = cast( null as numeric(18,8) )" +
                "   ,   C = cast( 12345.67 as numeric(18,8) )" +
                "   ,   D = cast( null as numeric(18,8) )"
                );

            var value = reader
                .Load1<TestClass2>();

            Assert.AreEqual( 12345.67m, value.A );
            Assert.AreEqual( null, value.B );
            Assert.AreEqual( 12345.67m, value.C );
            Assert.AreEqual( null, value.D );
        }

        [TestMethod]
        public void DecimalArray1()
        {
            using DbReader reader = new(
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
            using DbReader reader = new(
                "select	A = cast( 12345.67 as numeric(18,8) )" +
                " union all " +
                "select A = cast( 67890.12000 as numeric(18,8) )" +
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
            using DbReader reader = new(
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
            using DbReader reader = new(
                "select	A = cast( 12345.67 as numeric(18,8) )"
                );

            decimal? value = reader
                .Load1<decimal?>();

            Assert.AreEqual( 12345.67m, value );
        }

        [TestMethod]
        public void Decimal12()
        {
            using DbReader reader = new(
                "select	A = cast( null as numeric(18,8) )"
                );

            decimal? value = reader
                .Load1<decimal?>();

            Assert.AreEqual( null, value );
        }

        [TestMethod]
        public void DecimalError0()
        {
            using DbReader reader = new(
                "select	A = cast( 12345 as numeric(18,8) ) " +
                "where 1=0"
                );

            try
            {
                decimal value = reader
                    .Load1<decimal>();

                // ќжидаетс€ 1 строка, приходит 0 - должна быть ошибка
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
                "select	A = cast( 12345 as numeric(18,8) )" +
                " union all " +
                "select	A = cast( 12345 as numeric(18,8) )"
                );

            try
            {
                decimal value = reader
                    .Load1<decimal>();

                // ќжидаетс€ 1 строка, приходит две - должна быть ошибка
                Assert.Fail();
            }
            catch( FastDataLoaderException )
            {
            }
        }
    }
}

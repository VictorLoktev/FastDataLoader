using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastDataLoader;

namespace UnitTests
{
    [TestClass]
    public class SimpleTypes_DateTime
    {
        [TestMethod]
        public void DateTimeArray1()
        {
            using DbReader reader = new(
                "select	A = cast( '2020-01-01' as date )"
                );
            reader
                .Load()
                .To( out DateTime[] value )
                .End();

            Assert.AreEqual( 1, value.Length );
            Assert.AreEqual( new DateTime( 2020, 1, 1 ), value[ 0 ] );
        }

        [TestMethod]
        public void DateTimeArray2()
        {
            using DbReader reader = new(
                "select	A = cast( '2020-01-01' as date )" +
                " union all " +
                "select A = cast( '2022-12-22' as date )"
                );
            reader
                .Load()
                .To( out DateTime[] value )
                .End();

            Assert.AreEqual( 2, value.Length );
            Assert.AreEqual( new DateTime( 2020, 1, 1 ), value[ 0 ] );
            Assert.AreEqual( new DateTime( 2022, 12, 22 ), value[ 1 ] );
        }

        [TestMethod]
        public void DateTimeArray0()
        {
            using DbReader reader = new(
                "select	A = cast( '2020-01-01' as date ) " +
                "where 1=0"
                );
            reader
                .Load()
                .To( out DateTime[] value )
                .End();

            Assert.AreEqual( 0, value.Length );
        }

        [TestMethod]
        public void DateTime1()
        {
            using DbReader reader = new(
                "select	A = cast( '2020-01-01' as date )"
                );

            DateTime value = reader
                .Load1<DateTime>();

            Assert.AreEqual( new DateTime( 2020, 1, 1 ), value );
        }

        [TestMethod]
        public void DateTimeError0()
        {
            using DbReader reader = new(
                "select	A = cast( '2020-01-01' as date ) " +
                "where 1=0"
                );

            try
            {
                DateTime value = reader
                    .Load1<DateTime>();

                Assert.Fail();
            }
			catch( DataLoaderNoRecordsException )
			{
			}
			catch( Exception )
			{
				Assert.Fail( "¬озвращен неправильный тип Exception" );
			}
		}

		[TestMethod]
        public void DateTimeError2()
        {
            using DbReader reader = new(
                "select	A = cast( '2020-01-01' as date )" +
                " union all " +
                "select A = cast( '2022-01-22' as date )"
                );

            try
            {
                DateTime value = reader
                    .Load1<DateTime>();

                Assert.Fail();
            }
			catch( DataLoaderTooManyRecordsException )
			{
			}
			catch( Exception )
			{
				Assert.Fail( "¬озвращен неправильный тип Exception" );
			}
		}

		[TestMethod]
        public void DateTimeErrorNull()
        {
            using DbReader reader = new(
                "select	A = cast( null as date )"
                );

            try
            {
                DateTime value = reader
                    .Load1<DateTime>();

				// null нельз€ преобразовать в DateTime - ошибка, если получилось
				Assert.Fail();
			}
			catch( DataLoaderRuntimeException )
			{
			}
			catch( Exception )
			{
				Assert.Fail( "¬озвращен неправильный тип Exception" );
			}
		}

		[TestMethod]
        public void DateTimeErrorArrayNull()
        {
            using DbReader reader = new(
                "select	A = cast( '2020-01-01' as date )" +
                " union all " +
                "select A = cast( null as date )"
                );
            try
            {
                reader
                    .Load()
                .To( out DateTime[] value )
                .End();

                Assert.Fail();
				// null нельз€ преобразовать в DateTime - ошибка, если получилось
				Assert.Fail();
			}
			catch( DataLoaderRuntimeException )
			{
			}
			catch( Exception )
			{
				Assert.Fail( "¬озвращен неправильный тип Exception" );
			}
		}

	}
}

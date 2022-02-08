using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastDataLoader;

namespace UnitTests
{
    [TestClass]
    public class SimpleTypes_DateTimeN
    {
        [TestMethod]
        public void DateTimeArray1()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( '2020-01-01' as date )"
                );
            reader
                .Load()
                .To( out DateTime?[] value )
                .End();

            Assert.AreEqual( 1, value.Length );
            Assert.AreEqual( new DateTime( 2020, 1, 1 ), value[ 0 ] );
        }

        [TestMethod]
        public void DateTimeArray2()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( '2020-01-01' as date )" +
                " union all " +
                "select A = cast( '2022-12-22' as date )" +
                " union all " +
                "select A = cast( null as date )"
                );
            reader
                .Load()
                .To( out DateTime?[] value )
                .End();

            Assert.AreEqual( 3, value.Length );
            Assert.AreEqual( new DateTime( 2020, 1, 1 ), value[ 0 ] );
            Assert.AreEqual( new DateTime( 2022, 12, 22 ), value[ 1 ] );
            Assert.AreEqual( null, value[ 2 ] );
        }

        [TestMethod]
        public void DateTimeArray0()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( '2020-01-01' as date ) " +
                "where 1=0"
                );
            reader
                .Load()
                .To( out DateTime?[] value )
                .End();

            Assert.AreEqual( 0, value.Length );
        }

        [TestMethod]
        public void DateTime1()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( '2020-01-01' as date )"
                );

            DateTime? value = reader
                .Load1<DateTime?>();

            Assert.AreEqual( new DateTime( 2020, 1, 1 ), value );
        }

        [TestMethod]
        public void DateTime12()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( null as date )"
                );

            DateTime? value = reader
                .Load1<DateTime?>();

            Assert.AreEqual( null, value );
        }

        [TestMethod]
        public void DateTimeError0()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( '2020-01-01' as date ) " +
                "where 1=0"
                );

            try
            {
                DateTime value = reader
                    .Load1<DateTime>();

                Assert.Fail();
            }
            catch( DataLoaderException )
            {
            }
        }

        [TestMethod]
        public void DateTimeError2()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( '2020-01-01' as date )" +
                " union all " +
                "select	A = cast( '2020-01-01' as date )"
                );

            try
            {
                DateTime value = reader
                    .Load1<DateTime>();

                Assert.Fail();
            }
            catch( DataLoaderException )
            {
            }
        }
    }
}

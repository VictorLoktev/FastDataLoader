using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastDataLoader;

namespace UnitTests
{
    [TestClass]
    public class SimpleTypes_IntN
    {
        [TestMethod]
        public void IntArray1()
        {
            using DbReader reader = new(
                "select	A = cast( 12345 as int )"
                );
            reader
                .Load()
                .To( out int?[] value )
                .End();

            Assert.AreEqual( 1, value.Length );
            Assert.AreEqual( 12345, value[ 0 ] );
        }

        [TestMethod]
        public void IntArray2()
        {
            using DbReader reader = new(
                "select	A = cast( 12345 as int )" +
                " union all " +
                "select A = cast( 67890 as int )" +
                " union all " +
                "select A = cast( null as int )"
                );
            reader
                .Load()
                .To( out int?[] value )
                .End();

            Assert.AreEqual( 3, value.Length );
            Assert.AreEqual( 12345, value[ 0 ] );
            Assert.AreEqual( 67890, value[ 1 ] );
            Assert.AreEqual( null, value[ 2 ] );
        }

        [TestMethod]
        public void IntArray0()
        {
            using DbReader reader = new(
                "select	A = cast( 12345 as int ) " +
                "where 1=0"
                );
            reader
                .Load()
                .To( out int?[] value )
                .End();

            Assert.AreEqual( 0, value.Length );
        }

        [TestMethod]
        public void Int1()
        {
            using DbReader reader = new(
                "select	A = cast( 12345 as int )"
                );

            int? value = reader
                .Load1<int?>();

            Assert.AreEqual( 12345, value );
        }

        [TestMethod]
        public void Int12()
        {
            using DbReader reader = new(
                "select	A = cast( null as int )"
                );

            int? value = reader
                .Load1<int?>();

            Assert.AreEqual( null, value );
        }

        [TestMethod]
        public void IntError0()
        {
            using DbReader reader = new(
                "select	A = cast( 12345 as int ) " +
                "where 1=0"
                );

            try
            {
                int value = reader
                    .Load1<int>();

                Assert.Fail();
            }
            catch( FastDataLoaderException )
            {
            }
        }

        [TestMethod]
        public void IntError2()
        {
            using DbReader reader = new(
                "select	A = cast( 12345 as int )" +
                " union all " +
                "select	A = cast( 12345 as int )"
                );

            try
            {
                int value = reader
                    .Load1<int>();

                Assert.Fail();
            }
            catch( FastDataLoaderException )
            {
            }
        }
    }
}

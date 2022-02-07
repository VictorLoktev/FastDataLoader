using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastDataLoader;

namespace UnitTests
{
    [TestClass]
    public class SimpleTypes_BoolN
    {
        [TestMethod]
        public void BoolArray1()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 1 as bit )"
                );
            reader
                .Load()
                .To( out bool?[] value )
                .End();

            Assert.AreEqual( 1, value.Length );
            Assert.AreEqual( true, value[ 0 ] );
        }

        [TestMethod]
        public void BoolArray2()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 1 as bit )" +
                " union all " +
                "select A = cast( 0 as bit )" +
                " union all " +
                "select A = cast( null as bit )"
                );
            reader
                .Load()
                .To( out bool?[] value )
                .End();

            Assert.AreEqual( 3, value.Length );
            Assert.AreEqual( true, value[ 0 ] );
            Assert.AreEqual( false, value[ 1 ] );
            Assert.AreEqual( null, value[ 2 ] );
        }

        [TestMethod]
        public void BoolArray0()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 1 as bit ) " +
                "where 1=0"
                );
            reader
                .Load()
                .To( out bool?[] value )
                .End();

            Assert.AreEqual( 0, value.Length );
        }

        [TestMethod]
        public void Bool1()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 1 as bit )"
                );

            bool? value = reader
                .Load1<bool?>();

            Assert.AreEqual( true, value );
        }

        [TestMethod]
        public void Bool12()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( null as bit )"
                );

            bool? value = reader
                .Load1<bool?>();

            Assert.AreEqual( null, value );
        }

        [TestMethod]
        public void BoolError0()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 1 as bit ) " +
                "where 1=0"
                );

            try
            {
                bool value = reader
                    .Load1<bool>();

                Assert.Fail();
            }
            catch( DataLoaderException )
            {
            }
        }

        [TestMethod]
        public void BoolError2()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 1 as bit )" +
                " union all " +
                "select	A = cast( 1 as bit )"
                );

            try
            {
                bool value = reader
                    .Load1<bool>();

                Assert.Fail();
            }
            catch( DataLoaderException )
            {
            }
        }
    }
}

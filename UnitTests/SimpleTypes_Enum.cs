using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastDataLoader;

namespace UnitTests
{
    [TestClass]
    public class SimpleTypes_Enum
    {

        private enum TestEnum
        {
            Zero = 0,
            One = 1,
            Two = 2,
            Three = 3,
            Four = 4,
            Five = 5,
            Six = 6,
            Seven = 7
        }

        class TestEnumClass
        {
            public TestEnum A;
            public int B;
        }

        [TestMethod]
        public void Test1()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 4 as int )"
                );

            var value = reader.Load1<TestEnum>();

            Assert.AreEqual( TestEnum.Four, value );
        }

        [TestMethod]
        public void Test2()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 4 as int )" +
                "   ,   B = cast( 4 as int )"
                )
            ;

            var value = reader.Load1<TestEnumClass>();

            Assert.AreEqual( TestEnum.Four, value.A );
            Assert.AreEqual( 4, value.B );
        }

        [TestMethod]
        public void Test3()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 4 as int )" +
                "   ,   B = cast( 4 as int )"
                )
            ;

            var value = reader.Load1<Tuple<TestEnum, int>>();

            Assert.AreEqual( TestEnum.Four, value.Item1 );
            Assert.AreEqual( 4, value.Item2 );
        }
    }
}

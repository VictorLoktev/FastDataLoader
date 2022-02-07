using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastDataLoader;

namespace UnitTests
{
    [TestClass]
    public class Inheritance
    {
        public class Test1
        {
            public int A;
        }

        public class Test2 : Test1
        {
            public string B;
        }

        public class Test3 : Test2
        {
            public bool C;
        }

        [TestMethod]
        public void Load()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 12345 as int )" +
                "   ,   B = cast( '12345' as varchar(100) )" +
                "   ,   C = cast( 1 as bit )"
                );
            reader
                .Load()
                .To( out Test3 data )
                .End();

            Assert.AreEqual( 12345, data.A );
            Assert.AreEqual( "12345", data.B );
            Assert.AreEqual( true, data.C );
        }
    }
}

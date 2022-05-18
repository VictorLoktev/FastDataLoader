using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastDataLoader;

namespace UnitTests
{
    [TestClass]
    public class One
    {

        public class Test4
        {
            // Map to column A
            public int? A;

            public int? B { get; set; }
        }



        [TestMethod]
        public void Load5()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( null as int )" +
                "   ,   B = cast( 12345 as int )"
                );
            reader
                .Load()
                .To( out Test4 data )
                .End();
            Assert.AreEqual( null, data.A );
            Assert.AreEqual( 12345, data.B );
        }

        [TestMethod]
        public void Load6()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 12345  as int )" +
                "   ,   B = cast( null as int )"
                );
            reader
                .Load()
                .To( out Test4 data )
                .End();
            Assert.AreEqual( 12345, data.A );
            Assert.AreEqual( null, data.B );
        }

    }
}

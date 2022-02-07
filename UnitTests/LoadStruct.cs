using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastDataLoader;

namespace UnitTests
{
    [TestClass]
    public class LoadStruct
    {
        public struct Test1
        {
            // Map to column A
            public int A;

            // Map to column A too
            [Column("A")]
            public int A2 { get; set; }


            [Column( "B" )]
            public string B;

            // It's ok to map two members to the same one column
            [Column( "B" )]
            public string B2 { get; set; }
        }

        private struct Test2
        {
            private int A;

            public int AResult => A;
        }

        private struct Test3
        {
            private int A;

            public int AResult => A;

            private string B;

            // The constrcutor is used to initialize instances
            // Important! Constructor arguments must be in order of result columns!
            private Test3( int a, string b )
            {
                A = a;
                B = b;
            }
        }


        [TestMethod]
        public void Load1()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 12345 as int )" +
                "   ,   B = cast( '12345' as varchar(100) )"
                );
            reader
                .Load()
                .To( out Test1 data )
                .End();
            Assert.AreEqual( 12345, data.A );
            Assert.AreEqual( 12345, data.A2 );
            Assert.AreEqual( "12345", data.B );
            Assert.AreEqual( "12345", data.B2 );
        }


        [TestMethod]
        public void Load2()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 12345 as int )"
                );
            reader
                .Load()
                .To( out Test2 data )
                .End();
            // Load into private member of private class, then use public method
            Assert.AreEqual( 12345, data.AResult );
        }

        [TestMethod]
        public void LoadConstructor()
        {
            using DbReader reader = new DbReader(
                "select	A = cast( 12345 as int )" +
                "   ,   B = cast( '12345' as varchar(100) )"
                );
            reader
                .Load()
                .To( out Test3 data )
                .End();

            // Here the constructor is used to initialize the instance
            Assert.AreEqual( 12345, data.AResult );
        }
    }
}

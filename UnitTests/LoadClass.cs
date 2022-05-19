using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastDataLoader;

namespace UnitTests
{
    [TestClass]
    public class LoadClass
    {
        public class Test1
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

        // Disable warning IDE0044 'Make field readonly' in code or in project, otherwise use properties
#pragma warning disable IDE0044 // Add readonly modifier
        private class Test2
        {
            private int A;

            public int AResult => A;
        }
#pragma warning restore IDE0044 // Add readonly modifier

        // Disable warning IDE0044 'Make field readonly' in code or in project, otherwise use properties
#pragma warning disable IDE0044 // Add readonly modifier
        private class Test3
        {
            private int A;

            public int AResult => A;

            private string B;

            public string GetValueOfB()
            {
                return B;
            }

            // The constrcutor is used to initialize instances
            // Important! Constructor arguments must be in order of result columns!
            // You should ignore warning: Message IDE0051 Private member 'Test3..ctor' is unused UnitTests
#pragma warning disable IDE0051 // Remove unused private members
            private Test3( int a, string b )
#pragma warning restore IDE0051 // Remove unused private members
            {
                A = a;
                B = b;
            }
        }
#pragma warning restore IDE0044 // Add readonly modifier


        public class Test4
        {
            // Map to column A
            public int? A;

            public int? B { get; set; }
        }

        [TestMethod]
        public void Load1()
        {
            using DbReader reader = new(
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
            using DbReader reader = new(
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
            using DbReader reader = new(
                "select	A = cast( 12345 as int )" +
                "   ,   B = cast( '12345' as varchar(100) )"
                );
            reader
                .Load()
                .To( out Test3 data )
                .End();

            // Here the constructor is used to initialize the instance
            Assert.AreEqual( 12345, data.AResult );
            // Field B is private, use method to check it's value
            Assert.AreEqual( "12345", data.GetValueOfB() );
        }

        [TestMethod]
        public void Load4()
        {
            using DbReader reader = new(
                "select	A = cast( 12345 as int )" +
                "   ,   B = cast( 12345 as int )"
                );
            reader
                .Load()
                .To( out Test4 data )
                .End();
            Assert.AreEqual( 12345, data.A );
            Assert.AreEqual( 12345, data.B );
        }

        [TestMethod]
        public void Load5()
        {
            using DbReader reader = new(
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
            using DbReader reader = new(
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

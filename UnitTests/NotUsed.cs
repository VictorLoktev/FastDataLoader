using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastDataLoader;


namespace UnitTests
{
    [TestClass]
    public class NotUsed
    {
#pragma warning disable IDE0051 // Remove unused private members
        private struct Test1
        {
            public int A;
            public string B;

            // The constrcutor is used to initialize instances
            // Important! Constructor arguments must be in order of result columns!

            /*
             * You should ignore warning: IDE0051 Private member 'Test1..ctor' is unused UnitTests
             * Put
             * #pragma warning disable IDE0051
             * and
             * #pragma warning restore IDE0051
             * around the class or structure!
             */

            private Test1( int a, string b )
            {
                A = a;
                B = b;
            }
        }
#pragma warning restore IDE0051 // Remove unused private members

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
            Assert.AreEqual( "12345", data.B );
        }
    }
}

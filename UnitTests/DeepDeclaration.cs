using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastDataLoader;

namespace UnitTests
{
    [TestClass]
    public class DeepDeclaration
    {
        public class Test3
        {
            public class Test2
            {
                // We put class inside a class inside another class.
                // The loader can fill classes declared inside other classes.
                public class Test1
                {
                    // Map to column A
                    public int A;

                    // Map to column A too
                    [Column( "A" )]
                    public int A2 { get; set; }


                    [Column( "B" )]
                    public string B;

                    // It's ok to map two members to the same one column
                    [Column( "B" )]
                    public string B2 { get; set; }
                }
            }
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
                .To( out Test3.Test2.Test1 data )
                .End();
            Assert.AreEqual( 12345, data.A );
            Assert.AreEqual( 12345, data.A2 );
            Assert.AreEqual( "12345", data.B );
            Assert.AreEqual( "12345", data.B2 );
        }
    }
}

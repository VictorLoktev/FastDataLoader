using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastDataLoader;

namespace UnitTests
{
    [TestClass]
    public class OptionTest
    {
        // Disable warning IDE0044 'Make field readonly' in code or in project
#pragma warning disable IDE0044 // Add readonly modifier
        public class Test1
        {
            private int A;

            public int AResult => A;
        }
#pragma warning restore IDE0044 // Add readonly modifier

        // Disable warning IDE0044 'Make field readonly' in code or in project
#pragma warning disable IDE0044 // Add readonly modifier
        public class Test2
        {
            private int A;

            public int AResult => A;
        }
#pragma warning restore IDE0044 // Add readonly modifier

        [TestMethod]
        public void FailTest()
        {
            try
            {
                using DbReader reader = new(
                    "select	A = cast( 12345 as int )" +
                    "   ,   B = cast( '12345' as varchar(100) )" +
                    "   ,   C = cast( 1 as bit )"
                    );
                reader
                    .Load()
                    .To( out Test1 data )
                    .End();

                // Columns B and C are not mapped to class members
                Assert.Fail();
            }
            catch( FastDataLoaderException )
            {
            }
        }

        [TestMethod]
        public void IgnoreColumns()
        {
            using DbReader reader = new(
                "select	A = cast( 12345 as int )" +
                "   ,   B = cast( '12345' as varchar(100) )" +
                "   ,   C = cast( 1 as bit )"
                );
            reader
                .Load( new DataLoaderOptions() { IgnoresColumnNames = new string[] { "B", "C" } } )
                .To( out Test1 data )
                .End();
            Assert.AreEqual( 12345, data.AResult );
        }

        [TestMethod]
        public void UnmappedReaderColumn()
        {
            using DbReader reader = new(
                "select	A = cast( 12345 as int )" +
                "   ,   B = cast( '12345' as varchar(100) )" +
                "   ,   C = cast( 1 as bit )"
                );
            reader
                .Load( new DataLoaderOptions() { ExceptionIfUnmappedReaderColumn = false } )
                .To( out Test1 data )
                .End();
            Assert.AreEqual( 12345, data.AResult );
        }

        [TestMethod]
        public void UnmappedFieldOrProperty()
        {
            using DbReader reader = new(
                "select	A = cast( 12345 as int )"
                );
            reader
                .Load( new DataLoaderOptions() { ExceptionIfUnmappedFieldOrProperty = false } )
                .To( out Test2 data )
                .End();
            Assert.AreEqual( 12345, data.AResult );
        }
    }
}

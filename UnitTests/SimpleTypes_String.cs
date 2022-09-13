using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastDataLoader;

namespace UnitTests
{
    [TestClass]
    public class SimpleTypes_String
    {
        [TestMethod]
        public void StringArray1()
        {
            using DbReader reader = new(
                "select	A = cast( '12345' as nvarchar(max) )"
                );
            reader
                .Load()
                .To( out string[] value )
                .End();

            Assert.AreEqual( 1, value.Length );
            Assert.AreEqual( "12345", value[ 0 ] );
        }

        [TestMethod]
        public void StringArray3()
        {
            using DbReader reader = new(
                "select	A = cast( '12345' as nvarchar(max) )" +
                " union all " +
                "select A = cast( '67890' as nvarchar(max) )" +
                " union all " +
                "select A = cast( null as nvarchar(max) )"
                );
            reader
                .Load()
                .To( out string[] value )
                .End();

            Assert.AreEqual( 3, value.Length );
            Assert.AreEqual( "12345", value[ 0 ] );
            Assert.AreEqual( "67890", value[ 1 ] );
            Assert.AreEqual( null, value[ 2 ] );
        }

        [TestMethod]
        public void StringArray0()
        {
            using DbReader reader = new(
                "select	A = cast( '12345' as nvarchar(max) ) " +
                "where 1=0"
                );
            reader
                .Load()
                .To( out string[] value )
                .End();

            Assert.AreEqual( 0, value.Length );
        }

        [TestMethod]
        public void String1()
        {
            using DbReader reader = new(
                "select	A = cast( '12345' as nvarchar(max) )"
                );

            string value = reader
                .Load1<string>();

            Assert.AreEqual( "12345", value );
        }

        [TestMethod]
        public void String12()
        {
            using DbReader reader = new(
                "select	A = cast( null as nvarchar(max) )"
                );

            string value = reader
                .Load1<string>();

            Assert.AreEqual( null, value );
        }

        [TestMethod]
        public void StringError0()
        {
            using DbReader reader = new(
                "select	A = cast( '12345' as nvarchar(max) ) " +
                "where 1=0"
                );

            try
            {
                string value = reader
                    .Load1<string>();

                Assert.Fail();
            }
			catch( DataLoaderNoRecordsException )
			{
			}
			catch( Exception )
			{
				Assert.Fail( "Возвращен неправильный тип Exception" );
			}
		}

		[TestMethod]
        public void StringError2()
        {
            using DbReader reader = new(
                "select	A = cast( '12345' as nvarchar(max) )" +
                " union all " +
                "select	A = cast( '12345' as nvarchar(max) )"
                );

            try
            {
                string value = reader
                    .Load1<string>();

                Assert.Fail();
            }
			catch( DataLoaderTooManyRecordsException )
			{
			}
			catch( Exception )
			{
				Assert.Fail( "Возвращен неправильный тип Exception" );
			}
		}
	}
}

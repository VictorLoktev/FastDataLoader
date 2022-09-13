using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastDataLoader;

namespace UnitTests
{
    [TestClass]
    public class ManyResults
    {
        [TestMethod]
        public void Result3()
        {
            using DbReader reader = new(
                "select	A = cast( 12345 as int );" +
                "select B = cast( '12345' as varchar(100) );" +
                "select C = cast( 1 as bit );"
                );
            reader
                .Load()
                .To( out int intValue )
                .To( out string stringValue )
                .To( out bool boolValue )
                .End();

            Assert.AreEqual( 12345, intValue );
            Assert.AreEqual( "12345", stringValue );
            Assert.AreEqual( true, boolValue );
        }

        [TestMethod]
        public void Result2_1()
        {
            try
            {
                using DbReader reader = new(
                    "select	A = cast( 12345 as int );" +
                    "select B = cast( '12345' as varchar(100) );"
                    // one result set is missing
                    );
                reader
                    .Load()
                    .To( out int intValue )
                    .To( out string stringValue )
                    // Попытка чтения из закрытого DataReader!
                    .To( out bool? boolValue )
                    .End();
                Assert.Fail();
            }
            catch( DataLoaderClosedReaderException )
            {
            }
			catch( Exception )
			{
				Assert.Fail( "Возвращен неправильный тип Exception" );
			}
		}

		[TestMethod]
        public void Result2_2()
        {
            try
            {
                using DbReader reader = new(
                "select	A = cast( 12345 as int );" +
                "select B = cast( '12345' as varchar(100) );"
                // one result set is missing
                );
                reader
                    .Load()
                    .To( out int intValue )
                    .To( out string stringValue )
                    // Попытка чтения из закрытого DataReader!
                    .To( out bool?[] boolValue )
                    .End();

                //Assert.AreEqual( 12345, intValue );
                //Assert.AreEqual( "12345", stringValue );
                //Assert.AreEqual( 0, boolValue.Length );
                Assert.Fail();
            }
			catch( DataLoaderClosedReaderException )
			{
			}
			catch( Exception )
			{
				Assert.Fail( "Возвращен неправильный тип Exception" );
			}
		}

		[TestMethod]
        public void Result4()
        {
            using DbReader reader1 = new(
                "select	A = cast( 12345 as int );" +
                "select B = cast( '12345' as varchar(100) );"
                );
            reader1
                .Load()
                .To( out int intValue )
                .End();

            Assert.AreEqual( 12345, intValue );

            // Checking for Dispose inside End()

            using DbReader reader2 = new(
                "select B = cast( '12345' as varchar(100) );" +
                "select	A = cast( 12345 as int );"
                );
            reader2
                .Load()
                .To( out string stringValue )
                .End();

            Assert.AreEqual( "12345", stringValue );
        }

    }
}

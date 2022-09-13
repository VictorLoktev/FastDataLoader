using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastDataLoader;

namespace UnitTests
{
    [TestClass]
    public class SimpleTypes_Int
    {
        [TestMethod]
        public void IntArray1()
        {
            using DbReader reader = new(
                "select	A = cast( 12345 as int )"
                );
            reader
                .Load()
                .To( out int[] value )
                .End();

            Assert.AreEqual( 1, value.Length );
            Assert.AreEqual( 12345, value[ 0 ] );
        }

        [TestMethod]
        public void IntArray2()
        {
            using DbReader reader = new(
                "select	A = cast( 12345 as int )" +
                " union all " +
                "select A = cast( 67890 as int )"
                );
            reader
                .Load()
                .To( out int[] value )
                .End();

            Assert.AreEqual( 2, value.Length );
            Assert.AreEqual( 12345, value[ 0 ] );
            Assert.AreEqual( 67890, value[ 1 ] );
        }

        [TestMethod]
        public void IntArray0()
        {
            using DbReader reader = new(
                "select	A = cast( 12345 as int ) " +
                "where 1=0"
                );
            reader
                .Load()
                .To( out int[] value )
                .End();

            Assert.AreEqual( 0, value.Length );
        }

        [TestMethod]
        public void Int1()
        {
            using DbReader reader = new(
                "select	A = cast( 12345 as int )"
                );

            int value = reader
                .Load1<int>();

            Assert.AreEqual( 12345, value );
        }

        [TestMethod]
        public void IntError0()
        {
            using DbReader reader = new(
                "select	A = cast( 12345 as int ) " +
                "where 1=0"
                );

            try
            {
                int value = reader
                    .Load1<int>();

                Assert.Fail();
            }
			catch( DataLoaderNoRecordsException )
			{
			}
			catch( Exception )
			{
				Assert.Fail( "��������� ������������ ��� Exception" );
			}
		}

		[TestMethod]
        public void IntError2()
        {
            using DbReader reader = new(
                "select	A = cast( 12345 as int )" +
                " union all " +
                "select	A = cast( 12345 as int )"
                );

            try
            {
                int value = reader
                    .Load1<int>();

                Assert.Fail();
            }
			catch( DataLoaderTooManyRecordsException )
			{
			}
			catch( Exception )
			{
				Assert.Fail( "��������� ������������ ��� Exception" );
			}
		}

		[TestMethod]
        public void IntErrorNull()
        {
            using DbReader reader = new(
                "select	A = cast( null as int )"
                );

            try
            {
                int value = reader
                    .Load1<int>();

				// null ������ ������������� � int - ������, ���� ����������
				Assert.Fail();
			}
			catch( DataLoaderRuntimeException )
			{
			}
			catch( Exception )
			{
				Assert.Fail( "��������� ������������ ��� Exception" );
			}
		}

		[TestMethod]
        public void IntErrorArrayNull()
        {
            using DbReader reader = new(
                "select	A = cast( 12345 as int )" +
                " union all " +
                "select A = cast( null as int )"
                );
            try
            {
                reader
                    .Load()
                .To( out int[] value )
                .End();

				// null ������ ������������� � int - ������, ���� ����������
				Assert.Fail();
			}
			catch( DataLoaderRuntimeException )
			{
			}
			catch( Exception )
			{
				Assert.Fail( "��������� ������������ ��� Exception" );
			}
		}

	}
}

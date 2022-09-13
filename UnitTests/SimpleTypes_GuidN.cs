using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastDataLoader;

namespace UnitTests
{
    [TestClass]
    public class SimpleTypes_GuidN
    {
        [TestMethod]
        public void GuidArray1()
        {
            using DbReader reader = new(
                "select	A = cast( '51CE512F-1E4F-4995-BE95-A4F7388C88A8' as uniqueidentifier )"
                );
            reader
                .Load()
                .To( out Guid?[] value )
                .End();

            Assert.AreEqual( 1, value.Length );
            Assert.AreEqual( new Guid( "51CE512F-1E4F-4995-BE95-A4F7388C88A8" ), value[ 0 ] );
        }

        [TestMethod]
        public void GuidArray2()
        {
            using DbReader reader = new(
                "select	A = cast( '51CE512F-1E4F-4995-BE95-A4F7388C88A8' as uniqueidentifier )" +
                " union all " +
                "select A = cast( '5A3F846F-77F9-41B0-9723-CEBE4F9065A9' as uniqueidentifier )" +
                " union all " +
                "select A = cast( null as uniqueidentifier )"
                );
            reader
                .Load()
                .To( out Guid?[] value )
                .End();

            Assert.AreEqual( 3, value.Length );
            Assert.AreEqual( new Guid( "51CE512F-1E4F-4995-BE95-A4F7388C88A8" ), value[ 0 ] );
            Assert.AreEqual( new Guid( "5A3F846F-77F9-41B0-9723-CEBE4F9065A9" ), value[ 1 ] );
            Assert.AreEqual( null, value[ 2 ] );
        }

        [TestMethod]
        public void GuidArray0()
        {
            using DbReader reader = new(
                "select	A = cast( '51CE512F-1E4F-4995-BE95-A4F7388C88A8' as uniqueidentifier ) " +
                "where 1=0"
                );
            reader
                .Load()
                .To( out Guid?[] value )
                .End();

            Assert.AreEqual( 0, value.Length );
        }

        [TestMethod]
        public void Guid1()
        {
            using DbReader reader = new(
                "select	A = cast( '51CE512F-1E4F-4995-BE95-A4F7388C88A8' as uniqueidentifier )"
                );

            Guid? value = reader
                .Load1<Guid?>();

            Assert.AreEqual( new Guid( "51CE512F-1E4F-4995-BE95-A4F7388C88A8" ), value );
        }

        [TestMethod]
        public void Guid12()
        {
            using DbReader reader = new(
                "select	A = cast( null as uniqueidentifier )"
                );

            Guid? value = reader
                .Load1<Guid?>();

            Assert.AreEqual( null, value );
        }

        [TestMethod]
        public void GuidError0()
        {
            using DbReader reader = new(
                "select	A = cast( '51CE512F-1E4F-4995-BE95-A4F7388C88A8' as uniqueidentifier ) " +
                "where 1=0"
                );

            try
            {
                Guid value = reader
                    .Load1<Guid>();

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
        public void GuidError2()
        {
            using DbReader reader = new(
                "select	A = cast( '51CE512F-1E4F-4995-BE95-A4F7388C88A8' as uniqueidentifier )" +
                " union all " +
                "select	A = cast( '51CE512F-1E4F-4995-BE95-A4F7388C88A8' as uniqueidentifier )"
                );

            try
            {
                Guid value = reader
                    .Load1<Guid>();

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

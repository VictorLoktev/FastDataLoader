using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastDataLoader;


namespace UnitTests
{
    [TestClass]
    public class Readonly
    {

        /*
         * If you mark field with 'readonly' as Visual Studio suggests,
         * you need use constructor to initialize instance of structure or class.
         * If you marked field and didn't implement constructor,
         * you going to have an error.
         */

        private struct Test1
        {
            readonly public int A;
            public string B;
        }

        [TestMethod]
        public void Load1()
        {
            using DbReader reader = new(
                "select	A = cast( 12345 as int )" +
                "   ,   B = cast( '12345' as varchar(100) )"
                );
            try
            {
                reader
                    .Load()
                    .To( out Test1 data )
                    .End();

                Assert.Fail( "������: ����� ������ ���� ���������� ��-�� ����, ����������� readonly" );
            }
            catch( DataLoaderMetadataException )
            {
				/*
                 * ����� ������:
                 * FastDataLoader.DataLoaderMetadataException: '������ ������������� ���� 'UnitTests.Readonly.Test1': ���� 'A' �������� ��� readonly, ����������� ����������� ��� ����������'
                 */
			}
			catch( Exception )
			{
				Assert.Fail( "��������� ������������ ��� Exception");
			}
		}
	}
}

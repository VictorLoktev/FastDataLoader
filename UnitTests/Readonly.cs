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

                Assert.Fail( "Ошибка: здесь должно быть исключение из-за поля, помеченного readonly" );
            }
            catch( DataLoaderMetadataException )
            {
				/*
                 * Здесь ошибка:
                 * FastDataLoader.DataLoaderMetadataException: 'Ошибка инициализации типа 'UnitTests.Readonly.Test1': Поле 'A' помечено как readonly, используйте конструктор для заполнения'
                 */
			}
			catch( Exception )
			{
				Assert.Fail( "Возвращен неправильный тип Exception");
			}
		}
	}
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastDataLoader;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace UnitTests
{
    [TestClass]
    public class TypeCaster
    {
        /*
         * Column Phone in IDataReader has type bigint. Destination member has type PhoneType.
         * We declare a method to cast value from column type to member type.
         */

        public struct PhoneType
        {
            public string NumberStr { get; private set; }

            private static PhoneType MyMethodForCastingFromLongToPhoneType( long number )
            {
                PhoneType item = new PhoneType();
                item.NumberStr = number.ToString();
                return item;
            }
        }

        public class Person
        {
            public string Name { get; private set; }

            public PhoneType Phone;
        }

        [TestMethod]
        public void LoadArray()
        {
            using DbReader reader = new DbReader(
                "select	Name = 'John'" +
                "   ,   Phone = cast(1234567890 as bigint)"
                );
            reader
                .Load()
                .To( out Person data )
                .End();

            Assert.AreEqual( "John", data.Name );
            Assert.AreEqual( "1234567890", data.Phone.NumberStr );
        }
    }
}

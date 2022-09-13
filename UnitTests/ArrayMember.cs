using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastDataLoader;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace UnitTests
{
    [TestClass]
    public class ArrayMember
    {
        // Let's declare a type for array element
        public struct PhoneType
        {
            public string Phone { get; private set; }

            // The static method inside the class.
            // The method with 1 argument type of string. The argument gets xml in string.
            // The method returns an array.
            // Column of XML type MS always converts into strings in DataReaders.
            // So, we declare string xml argument instead of SqlXml xml.

            // Suppress warning IDE0051 Private member 'PhoneType.PasseXmlIntoStringArray' is unused UnitTests in code or in project
#pragma warning disable IDE0051 // Remove unused private members
            private static PhoneType[] PasseXmlIntoStringArray( string xml )
#pragma warning restore IDE0051 // Remove unused private members
            {
                List<PhoneType> list = new();
                foreach( var item in XmlToArray<string>( xml ) )
                    list.Add( new PhoneType() { Phone = item } );
                return list.ToArray();
            }

            public static T[] XmlToArray<T>( string xml )
            {
                return XElement.Parse( xml )
                    .DescendantNodesAndSelf()
                    .OfType<XText>()
                    .Select( x => (T)Convert.ChangeType( x.Value, typeof( T ) ) )
                    .ToArray();
            }
        }

        public class Person
        {
            public string Name { get; private set; }


            // Using an element type to build an array
            public PhoneType[] Phones;
        }

        [TestMethod]
        public void LoadArray()
        {
            using DbReader reader = new(
                "select	Name = 'John'" +
                "   ,   Phones = cast('<root><home>+155512345</home><work>+155554321</work><mobile></mobile></root>' as xml)"
                );
            reader
                .Load()
                .To( out Person data )
                .End();

            Assert.AreEqual( 2, data.Phones.Length );
            Assert.AreEqual( "+155512345", data.Phones[ 0 ].Phone );
            Assert.AreEqual( "+155554321", data.Phones[ 1 ].Phone );
        }
    }
}

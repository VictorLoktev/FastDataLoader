using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastDataLoader;
using Microsoft.SqlServer.Types;

namespace UnitTests
{
    [TestClass]
    public class Geometry
    {
        public class TestGeometry
        {
            public int A;
            public SqlGeometry Geo;
        }

        [TestMethod]
        public void FillField()
        {
            using DbReader reader = new DbReader(
                "select	A = 1" +
                "   ,   Geo = geometry::STGeomFromText('POLYGON ((0 0, 150 0, 150 150, 0 150, 0 0))', 0)"
                );
            reader
                .Load()
                .To( out TestGeometry[] value )
                .End();

            Assert.AreEqual( 1, value.Length );
            Assert.AreEqual( 1, value[ 0 ].A );
        }

        [TestMethod]
        public void UseConstructor()
        {
            using DbReader reader = new DbReader(
                "select	Geo = geometry::STGeomFromText('POLYGON ((0 0, 150 0, 150 150, 0 150, 0 0))', 0)"
                );
            reader
                .Load()
                .To( out SqlGeometry value )
                .End();

            Assert.AreEqual( "POLYGON ((0 0, 150 0, 150 150, 0 150, 0 0))", value.ToString() );
        }
    }
}

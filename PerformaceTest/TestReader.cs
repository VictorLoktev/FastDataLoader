using System;
using System.Data;
using System.Data.SqlClient;
using FastDataLoader;

namespace PerformaceTest
{
    internal class TestReader : DataLoaderLoadContext
    {
        private readonly SqlCommand _Command;

        public TestReader( SqlCommand command )
        {
            _Command = command;
        }
        public override void DumpSql( Exception exception )
        {
            // do nothing
        }

        public override IDataReader GetDataReader()
        {
            return _Command.ExecuteReader();
        }
    }
}

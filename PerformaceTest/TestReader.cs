using System;
using System.Data;
using System.Data.SqlClient;
using FastDataLoader;

namespace PerformaceTest
{
    internal class TestReader : DataLoaderLoadContext
    {
        private readonly SqlCommand _Command;
        private IDataReader _Reader;

        public TestReader( SqlCommand command )
        {
            _Command = command;
            _Reader = null;
        }
        public override void DumpSql( Exception exception )
        {
            // do nothing
        }

        public override IDataReader GetDataReader()
        {
            _Reader = _Command.ExecuteReader();
            return _Reader;
        }

        public void Clear()
        {
            if( _Reader != null )
                _Reader.Dispose();
            _Reader = null;
        }
    }
}

using System;
using System.Data;
using System.Data.SqlClient;
using FastDataLoader;

namespace UnitTests
{
    internal class DbReader : DataLoaderLoadContext, IDisposable
    {
        private readonly SqlCommand _Command;
        private SqlDataReader _Reader;

        public DbReader( string sql )
        {
            SqlConnection connection = new( "Data Source=127.0.0.1;Initial Catalog=master;Integrated Security=True" );
            connection.Open();

            using SqlCommand command = new();
            command.Connection = connection;
            command.CommandTimeout = 300;
            command.CommandText = sql;
            _Command = command;
            _Reader = null;
        }

        public void Dispose()
        {
            SqlConnection connection = _Command.Connection;
            _Reader.Close();
            _Reader = null;
            _Command.Dispose();
            connection.Dispose();
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


    }
}

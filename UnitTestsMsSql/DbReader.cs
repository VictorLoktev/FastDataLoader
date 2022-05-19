using System;
using System.Data;
using System.Data.SqlClient;
using FastDataLoader;

namespace UnitTests
{
    internal class DbReader : DataLoaderLoadContext, IDisposable
    {
        private readonly SqlCommand _Command;

        public DbReader( string sql )
        {
            SqlConnection connection = new( "Data Source=127.0.0.1;Initial Catalog=master;Integrated Security=True" );
            connection.Open();

            using SqlCommand command = new();
            command.Connection = connection;
            command.CommandTimeout = 300;
            command.CommandText = sql;
            _Command = command;
        }

        public void Dispose()
        {
            SqlConnection connection = _Command.Connection;
            _Command.Dispose();
            connection.Dispose();
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

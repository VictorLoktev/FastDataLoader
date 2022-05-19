using System;
using System.Data;
using System.Data.SqlClient;
using FastDataLoader;

namespace UnitTests
{
    internal class DbReader : DataLoaderLoadContext, IDisposable
    {
        SqlCommand _Command;

        public DbReader( string sql )
        {
            SqlConnection connection = new SqlConnection( "Data Source=127.0.0.1;Initial Catalog=Mineral.MF.LegacyApp;Integrated Security=True" );
            connection.Open();

            using( SqlCommand command = new SqlCommand() )
            {
                command.Connection = connection;
                command.CommandTimeout = 300;
                command.CommandText = sql;
                _Command = command;
            }
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

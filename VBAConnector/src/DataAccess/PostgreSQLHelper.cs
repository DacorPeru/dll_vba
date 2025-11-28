using Npgsql;
using System;
using System.Data;
using VBAConnector.Interfaces;

namespace VBAConnector.DataAccess
{
    public class PostgreSQLHelper : DBHelper
    {
        // Constructor que llama al constructor base para establecer la cadena de conexión
        public PostgreSQLHelper(string connString) : base(connString) { }

        // Método para ejecutar una consulta SELECT en PostgreSQL
        public DataTable ExecutePostgreSQLSelect(string query)
        {
            using (var connection = new NpgsqlConnection(connectionString))  // Usando la cadena de conexión heredada
            {
                connection.Open();
                var command = new NpgsqlCommand(query, connection);
                var dataTable = new DataTable();
                dataTable.Load(command.ExecuteReader());
                return dataTable;
            }
        }
    }
}

using Npgsql;
using System;
using System.Data;
using VBAConnector.Interfaces;

namespace VBAConnector.DataAccess
{
    public class DBHelper : IDataAccess
    {
        // Modificado a 'protected' para que sea accesible en las clases derivadas
        protected string connectionString;

        // Constructor que configura la cadena de conexión
        public DBHelper(string connString)
        {
            connectionString = connString;
        }

        // Función para ejecutar una consulta SELECT
        public DataTable ExecuteSelect(string query)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand(query, connection);
                var dataTable = new DataTable();
                dataTable.Load(command.ExecuteReader());
                return dataTable;
            }
        }

        // Función para ejecutar consultas INSERT, UPDATE, DELETE
        public int ExecuteNonQuery(string query)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand(query, connection);
                return command.ExecuteNonQuery();
            }
        }

        // Métodos CRUD adicionales (INSERT, UPDATE, DELETE)
        public int Insert(string table, string columns, string values)
        {
            string query = $"INSERT INTO {table} ({columns}) VALUES ({values})";
            return ExecuteNonQuery(query);
        }

        public int Update(string table, string setValues, string condition)
        {
            string query = $"UPDATE {table} SET {setValues} WHERE {condition}";
            return ExecuteNonQuery(query);
        }

        public int Delete(string table, string condition)
        {
            string query = $"DELETE FROM {table} WHERE {condition}";
            return ExecuteNonQuery(query);
        }
    }
}

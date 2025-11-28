using System;

namespace VBAConnector.DataAccess
{
    public class DBFactory
    {
        // Método para crear el objeto adecuado para la conexión a la base de datos
        public static DBHelper CreateDBHelper(string dbType, string connString)
        {
            if (dbType.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            {
                return new PostgreSQLHelper(connString);
            }

            // Aquí puedes agregar más tipos de bases de datos en el futuro (MySQL, SQL Server, etc.)
            throw new NotImplementedException($"No se soporta el tipo de base de datos: {dbType}");
        }
    }
}

using System;
using System.IO;
using Newtonsoft.Json;

namespace VBAConnector.DataAccess
{
    public class ConnectionManager
    {
        private string currentConnectionString;

        // Cargar configuración desde archivo JSON
        public void LoadConnectionConfig(string configFilePath)
        {
            var configJson = File.ReadAllText(configFilePath);
            dynamic config = JsonConvert.DeserializeObject(configJson);

            // Asumimos que el archivo JSON tiene una propiedad "connectionString"
            currentConnectionString = config.connectionString;
        }

        // Obtener la cadena de conexión actual
        public string GetConnectionString()
        {
            if (string.IsNullOrEmpty(currentConnectionString))
            {
                throw new InvalidOperationException("La cadena de conexión no ha sido cargada.");
            }
            return currentConnectionString;
        }
    }
}

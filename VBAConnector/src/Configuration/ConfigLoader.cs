using System.IO;
using Newtonsoft.Json;

namespace VBAConnector.Configuration
{
    public class ConfigLoader
    {
        public static string LoadConnectionString(string configFilePath)
        {
            var configJson = File.ReadAllText(configFilePath);
            dynamic config = JsonConvert.DeserializeObject(configJson);

            return config.connectionString;
        }
    }
}

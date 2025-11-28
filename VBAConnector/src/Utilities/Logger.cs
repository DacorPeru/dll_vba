using System;
using System.IO;

namespace VBAConnector.Utilities
{
    public class Logger
    {
        private static string logFilePath = "log.txt";

        // Función para registrar eventos
        public static void Log(string message)
        {
            using (var writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }
    }
}

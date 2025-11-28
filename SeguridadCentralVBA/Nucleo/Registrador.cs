using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SeguridadCentral.Nucleo
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class Registrador
    {
        private string rutaLog;
        // QUITAR esta línea
        // private ManejadorErrores manejadorErrores;

        public Registrador()
        {
            // QUITAR esta línea
            // manejadorErrores = new ManejadorErrores();
            InicializarRutaLog();
        }

        private void InicializarRutaLog()
        {
            try
            {
                string carpetaAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                rutaLog = Path.Combine(carpetaAppData, "SeguridadCentralVBA", "logs");
                Directory.CreateDirectory(rutaLog);
            }
            catch
            {
                rutaLog = Path.GetTempPath();
            }
        }

        [ComVisible(true)]
        public void RegistrarEvento(string mensaje)
        {
            try
            {
                string archivoLog = Path.Combine(rutaLog, $"eventos_{DateTime.Now:yyyyMMdd}.log");
                string linea = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {mensaje}{Environment.NewLine}";
                File.AppendAllText(archivoLog, linea);
            }
            catch (Exception ex)
            {
                // Usar el propio método de la clase
                RegistrarError("Registrador-Evento", ex);
            }
        }

        [ComVisible(true)]
        public void RegistrarError(string contexto, Exception excepcion)
        {
            try
            {
                string archivoLog = Path.Combine(rutaLog, $"errores_{DateTime.Now:yyyyMMdd}.log");
                string linea = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - [{contexto}] {excepcion.GetType().Name}: {excepcion.Message}{Environment.NewLine}";
                File.AppendAllText(archivoLog, linea);
            }
            catch
            {
                // Último recurso si falla el registro
            }
        }

        [ComVisible(true)]
        public string ObtenerUltimosEventos(int cantidad = 10)
        {
            try
            {
                string archivoLog = Path.Combine(rutaLog, $"eventos_{DateTime.Now:yyyyMMdd}.log");
                if (!File.Exists(archivoLog)) return "No hay eventos registrados";

                var lineas = File.ReadAllLines(archivoLog);
                int inicio = Math.Max(0, lineas.Length - cantidad);
                return string.Join(Environment.NewLine, lineas, inicio, lineas.Length - inicio);
            }
            catch (Exception ex)
            {
                return $"Error al leer eventos: {ex.Message}";
            }
        }
    }
}

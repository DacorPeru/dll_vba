using System;
using System.Runtime.InteropServices;

namespace SeguridadCentral.Nucleo
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class ManejadorErrores
    {
        private Registrador registrador;

        public ManejadorErrores()
        {
            registrador = new Registrador();
        }

        [ComVisible(true)]
        public string ManejarError(Exception excepcion, string contexto = "")
        {
            try
            {
                string mensajeError = $"Error en {contexto}: {excepcion.Message}";

                // Registrar el error
                registrador.RegistrarError(contexto, excepcion);

                // Determinar tipo de error y mensaje amigable
                if (excepcion is UnauthorizedAccessException)
                    return "Error de permisos. Ejecute como administrador.";
                else if (excepcion is System.IO.IOException)
                    return "Error de acceso a archivos. Verifique permisos.";
                else
                    return $"Error del sistema: {excepcion.Message}";
            }
            catch
            {
                return "Error crítico no recuperable";
            }
        }

        [ComVisible(true)]
        public bool EsErrorRecuperable(Exception excepcion)
        {
            return !(excepcion is OutOfMemoryException ||
                    excepcion is StackOverflowException ||
                    excepcion is AccessViolationException);
        }
    }
}
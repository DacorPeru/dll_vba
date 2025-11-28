using System;
using System.Runtime.InteropServices;

namespace SeguridadCentral.Utilidades
{
    internal static class TiempoSistema
    {
        // Llamada nativa a la API de Windows (kernel32)
        [DllImport("kernel32.dll")]
        private static extern ulong GetTickCount64();

        /// <summary>
        /// Tiempo en milisegundos desde el arranque de Windows.
        /// No depende de la fecha/hora del sistema.
        /// </summary>
        public static long ObtenerTickMs()
        {
            return unchecked((long)GetTickCount64());
        }
    }
}

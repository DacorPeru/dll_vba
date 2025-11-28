using System;
using System.Runtime.InteropServices;

namespace SeguridadCentral.Modelos
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class DetalleValidacion
    {
        [ComVisible(true)]
        public string NombreValidador { get; set; }

        [ComVisible(true)]
        public bool EsValido { get; set; }

        [ComVisible(true)]
        public string Mensaje { get; set; }

        [ComVisible(true)]
        public DateTime FechaValidacion { get; set; }

        [ComVisible(true)]
        public string InformacionAdicional { get; set; }

        public DetalleValidacion()
        {
            NombreValidador = "";
            Mensaje = "";
            InformacionAdicional = "";
            FechaValidacion = DateTime.Now;
            EsValido = true;
        }
    }
}
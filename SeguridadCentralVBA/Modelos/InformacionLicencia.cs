using System;
using System.Runtime.InteropServices;

namespace SeguridadCentral.Modelos
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class InformacionLicencia
    {
        [ComVisible(true)]
        public string Cliente { get; set; }

        [ComVisible(true)]
        public string Producto { get; set; }

        [ComVisible(true)]
        public DateTime FechaExpiracion { get; set; }

        [ComVisible(true)]
        public string Version { get; set; }

        [ComVisible(true)]
        public string TipoLicencia { get; set; }

        [ComVisible(true)]
        public bool Activa { get; set; }

        public InformacionLicencia()
        {
            Cliente = "Cliente por defecto";
            Producto = "SeguridadCentralVBA";
            FechaExpiracion = new DateTime(2025, 12, 31);
            Version = "1.0.0";
            TipoLicencia = "Standard";
            Activa = true;
        }
    }
}
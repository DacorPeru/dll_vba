using System;
using System.Runtime.InteropServices;

namespace SeguridadCentral.Modelos
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class InformacionSistema
    {
        [ComVisible(true)]
        public string NombreEquipo { get; set; }

        [ComVisible(true)]
        public string Usuario { get; set; }

        [ComVisible(true)]
        public string Dominio { get; set; }

        [ComVisible(true)]
        public string SistemaOperativo { get; set; }

        [ComVisible(true)]
        public string Arquitectura { get; set; }

        [ComVisible(true)]
        public DateTime FechaHoraSistema { get; set; }

        [ComVisible(true)]
        public string Cultura { get; set; }

        public InformacionSistema()
        {
            NombreEquipo = Environment.MachineName;
            Usuario = Environment.UserName;
            Dominio = Environment.UserDomainName;
            SistemaOperativo = Environment.OSVersion.ToString();
            Arquitectura = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
            FechaHoraSistema = DateTime.Now;
            Cultura = System.Globalization.CultureInfo.CurrentCulture.Name;
        }
    }
}
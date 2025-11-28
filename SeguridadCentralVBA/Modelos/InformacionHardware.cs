using System.Runtime.InteropServices;

namespace SeguridadCentral.Modelos
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class InformacionHardware
    {
        public string CPU { get; set; }
        public string DISK { get; set; }
        public string BIOS { get; set; }
        public string BOARD { get; set; }
        public string GUID { get; set; }
        public string MAC { get; set; }
        public string FINGERPRINT { get; set; }

        // Extras para tu formulario
        public string SistemaOperativo { get; set; }
        public string UsuarioWindows { get; set; }
        public string ZonaHoraria { get; set; }
    }
}

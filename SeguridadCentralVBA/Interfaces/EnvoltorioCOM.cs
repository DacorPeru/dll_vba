using SeguridadCentral.Modelos;
using SeguridadCentral.Nucleo;
using SeguridadCentralVBA;
using System;
using System.Runtime.InteropServices;

namespace SeguridadCentralVBA
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("SeguridadCentralVBA.EnvoltorioCOM")]
    [Guid("23456789-2345-2345-2345-23456789ABCD")]
    public class EnvoltorioCOM
    {
        private readonly InterfazVBA interfazVBA;
        private readonly Registrador registrador;

        public EnvoltorioCOM()
        {
            interfazVBA = new InterfazVBA();
            registrador = new Registrador();
        }

        // --------------------------------------------------------
        //  LICENCIA / SEGURIDAD GENERAL
        // --------------------------------------------------------

        [ComVisible(true)]
        public bool VerificarLicencia()
        {
            registrador.RegistrarEvento("Verificación de licencia vía COM.");
            return interfazVBA.VerificarSeguridad();
        }

        [ComVisible(true)]
        public string EjecutarValidacionCompleta()
        {
            registrador.RegistrarEvento("Validación completa vía COM.");
            return interfazVBA.ValidarSistemaCompleto();
        }

        // --------------------------------------------------------
        //  CRIPTO / CONFIGURACIÓN
        // --------------------------------------------------------

        [ComVisible(true)]
        public string Encriptar(string texto)
        {
            return interfazVBA.EncriptarTexto(texto);
        }

        [ComVisible(true)]
        public string Desencriptar(string textoEncriptado)
        {
            return interfazVBA.DesencriptarTexto(textoEncriptado);
        }

        [ComVisible(true)]
        public void Configurar(string clave, string valor)
        {
            interfazVBA.GuardarValor(clave, valor);
        }

        [ComVisible(true)]
        public string ObtenerConfiguracion(string clave)
        {
            return interfazVBA.LeerValor(clave);
        }

        // --------------------------------------------------------
        //  ESTADO / LOGS
        // --------------------------------------------------------

        [ComVisible(true)]
        public string ObtenerEstado()
        {
            return interfazVBA.ObtenerEstadoSistema();
        }

        [ComVisible(true)]
        public string ObtenerLogs()
        {
            return interfazVBA.ObtenerLogEventos();
        }

        // --------------------------------------------------------
        //  VALIDACIONES ESPECÍFICAS
        // --------------------------------------------------------

        [ComVisible(true)]
        public string ValidarRed()
        {
            registrador.RegistrarEvento("Validación de red vía COM.");
            return interfazVBA.ValidarRed();
        }

        [ComVisible(true)]
        public string ValidarTemporal()
        {
            registrador.RegistrarEvento("Validación temporal vía COM.");
            return interfazVBA.ValidarTemporal();
        }

        [ComVisible(true)]
        public string ValidarHardware()
        {
            registrador.RegistrarEvento("Validación de hardware vía COM.");
            return interfazVBA.ValidarHardware();
        }

        // --------------------------------------------------------
        //  OBTENER INFORMACIÓN DE HARDWARE
        // --------------------------------------------------------

        [ComVisible(true)]
        public InformacionHardware ObtenerInformacionDetalladaHardware()
        {
            // ✅ CORREGIDO: Llamada al método correcto
            return interfazVBA.ObtenerInformacionHardware();
        }

        // --------------------------------------------------------
        //  ADMIN / SOPORTE – RESET SEGURIDAD TEMPORAL
        // --------------------------------------------------------

        /// <summary>
        /// Reinicia el estado de la seguridad temporal (solo para soporte/desarrollo).
        /// Deja vacías las claves usadas por el ValidadorTemporal.
        /// </summary>
        [ComVisible(true)]
        public bool ReiniciarSeguridadTemporal()
        {
            registrador.RegistrarEvento("Reinicio de seguridad temporal vía COM.");
            return interfazVBA.ReiniciarSeguridadTemporal();
        }
    }
}
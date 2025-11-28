using System;
using System.Runtime.InteropServices;
using SeguridadCentral.Nucleo;
using SeguridadCentral.Proteccion;
using SeguridadCentral.Utilidades;
using SeguridadCentral.Validaciones;
using SeguridadCentral.Modelos;

namespace SeguridadCentralVBA
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("SeguridadCentralVBA.InterfazVBA")]
    [Guid("12345678-1234-1234-1234-123456789ABC")]
    public class InterfazVBA
    {
        private readonly MotorSeguridad motorSeguridad;
        private readonly AyudanteCriptografia ayudanteCrypto;
        private readonly ManejadorConfiguracion manejadorConfig;
        private readonly OfuscadorCodigo ofuscador;
        private readonly Registrador registrador;

        // Claves usadas también por ValidadorTemporal
        private const string ClaveUltimaValidacion = "UltimaValidacionUtc";
        private const string ClaveUltimoTickMs = "UltimoTickMs";

        public InterfazVBA()
        {
            motorSeguridad = new MotorSeguridad();
            ayudanteCrypto = new AyudanteCriptografia();
            manejadorConfig = new ManejadorConfiguracion();
            ofuscador = new OfuscadorCodigo();
            registrador = new Registrador();
        }

        // --------------------------------------------------------
        //  LICENCIA / SEGURIDAD GENERAL
        // --------------------------------------------------------

        [ComVisible(true)]
        public bool VerificarSeguridad()
        {
            try
            {
                registrador.RegistrarEvento("Verificación de seguridad iniciada desde VBA.");
                return motorSeguridad.VerificarLicencia();
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("InterfazVBA-VerificarSeguridad", ex);
                return false;
            }
        }

        [ComVisible(true)]
        public string ValidarSistemaCompleto()
        {
            try
            {
                ResultadoValidacion resultado = motorSeguridad.ValidarSistema();
                return resultado.EsValido ? "SISTEMA_VALIDO" : "SISTEMA_INVALIDO";
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("InterfazVBA-ValidarSistemaCompleto", ex);
                return "ERROR: " + ex.Message;
            }
        }

        // --------------------------------------------------------
        //  CRIPTOGRAFÍA / OFUSCACIÓN
        // --------------------------------------------------------

        [ComVisible(true)]
        public string EncriptarTexto(string texto)
        {
            try
            {
                return ayudanteCrypto.Encriptar(texto);
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("InterfazVBA-Encriptar", ex);
                return "ERROR: " + ex.Message;
            }
        }

        [ComVisible(true)]
        public string DesencriptarTexto(string textoEncriptado)
        {
            try
            {
                return ayudanteCrypto.Desencriptar(textoEncriptado);
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("InterfazVBA-Desencriptar", ex);
                return "ERROR: " + ex.Message;
            }
        }

        [ComVisible(true)]
        public string OfuscarTexto(string texto)
        {
            try
            {
                return ofuscador.OfuscarString(texto);
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("InterfazVBA-OfuscarTexto", ex);
                return "ERROR: " + ex.Message;
            }
        }

        [ComVisible(true)]
        public string DesofuscarTexto(string textoOfuscado)
        {
            try
            {
                return ofuscador.DesofuscarString(textoOfuscado);
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("InterfazVBA-DesofuscarTexto", ex);
                return "ERROR: " + ex.Message;
            }
        }

        // --------------------------------------------------------
        //  CONFIGURACIÓN (REGISTRO)
        // --------------------------------------------------------

        [ComVisible(true)]
        public bool GuardarValor(string clave, string valor)
        {
            try
            {
                return manejadorConfig.GuardarConfiguracion(clave, valor);
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("InterfazVBA-Guardar-" + clave, ex);
                return false;
            }
        }

        [ComVisible(true)]
        public string LeerValor(string clave)
        {
            try
            {
                return manejadorConfig.LeerConfiguracion(clave);
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("InterfazVBA-Leer-" + clave, ex);
                return "ERROR: " + ex.Message;
            }
        }

        // --------------------------------------------------------
        //  ESTADO / LOGS
        // --------------------------------------------------------

        [ComVisible(true)]
        public string ObtenerEstadoSistema()
        {
            try
            {
                ResultadoValidacion resultado = motorSeguridad.ValidarSistema();
                return resultado.Mensaje;
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("InterfazVBA-ObtenerEstadoSistema", ex);
                return "ERROR: " + ex.Message;
            }
        }

        [ComVisible(true)]
        public string ObtenerLogEventos()
        {
            try
            {
                return registrador.ObtenerUltimosEventos(20);
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("InterfazVBA-ObtenerLogEventos", ex);
                return "ERROR: " + ex.Message;
            }
        }

        // --------------------------------------------------------
        //  VALIDADOR DE RED
        // --------------------------------------------------------

        [ComVisible(true)]
        public string ValidarRed()
        {
            try
            {
                var validador = new ValidadorRed();
                DetalleValidacion d = validador.Validar();

                string prefijo = d.EsValido ? "OK: " : "ERROR: ";
                return prefijo + d.Mensaje;
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("InterfazVBA-ValidarRed", ex);
                return "ERROR: " + ex.Message;
            }
        }

        // --------------------------------------------------------
        //  VALIDADOR TEMPORAL (FECHA/HORA)
        // --------------------------------------------------------

        [ComVisible(true)]
        public string ValidarTemporal()
        {
            try
            {
                var validador = new ValidadorTemporal();
                DetalleValidacion d = validador.Validar();

                string prefijo = d.EsValido ? "OK: " : "ERROR: ";
                return prefijo + d.Mensaje;
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("InterfazVBA-ValidarTemporal", ex);
                return "ERROR: " + ex.Message;
            }
        }

        // --------------------------------------------------------
        //  VALIDADOR DE HARDWARE / FINGERPRINT
        // --------------------------------------------------------

        [ComVisible(true)]
        public string ValidarHardware()
        {
            try
            {
                var v = new ValidadorHardware();
                DetalleValidacion d = v.Validar();

                string prefijo = d.EsValido ? "OK: " : "ERROR: ";
                string extra = string.IsNullOrEmpty(d.InformacionAdicional)
                               ? string.Empty
                               : " | " + d.InformacionAdicional;

                return prefijo + d.Mensaje + extra;
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("InterfazVBA-ValidarHardware", ex);
                return "ERROR: " + ex.Message;
            }
        }

        // NUEVO: devuelve objeto con CPU, DISK, BIOS, BOARD, GUID, MAC, FINGERPRINT,
        // SistemaOperativo, UsuarioWindows, ZonaHoraria
        [ComVisible(true)]
        public InformacionHardware ObtenerInformacionHardware()
        {
            try
            {
                var v = new ValidadorHardware();
                InformacionHardware info = v.ObtenerInformacionDetallada();
                return info;
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("InterfazVBA-ObtenerInformacionHardware", ex);
                // Devolvemos un objeto vacío para evitar null en VBA
                return new InformacionHardware();
            }
        }

        // Ahora solo devuelve el hash (no la cadena completa)
        [ComVisible(true)]
        public string ObtenerFingerprintHardware()
        {
            try
            {
                var v = new ValidadorHardware();
                InformacionHardware info = v.ObtenerInformacionDetallada();
                return info.FINGERPRINT ?? string.Empty;
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("InterfazVBA-ObtenerFingerprintHardware", ex);
                return string.Empty;
            }
        }

        // --------------------------------------------------------
        //  ADMIN / SOPORTE – RESET SEGURIDAD TEMPORAL
        // --------------------------------------------------------

        /// <summary>
        /// Reinicia la información usada por el ValidadorTemporal para
        /// permitir una nueva "primera validación" con la fecha actual.
        /// Pensado para soporte/desarrollo.
        /// </summary>
        [ComVisible(true)]
        public bool ReiniciarSeguridadTemporal()
        {
            try
            {
                manejadorConfig.GuardarConfiguracion(ClaveUltimaValidacion, string.Empty, true);
                manejadorConfig.GuardarConfiguracion(ClaveUltimoTickMs, string.Empty, true);

                registrador.RegistrarEvento("Seguridad temporal reiniciada desde VBA.");
                return true;
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("InterfazVBA-ReiniciarSeguridadTemporal", ex);
                return false;
            }
        }
    }
}
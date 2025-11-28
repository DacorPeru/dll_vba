using Microsoft.Win32;
using SeguridadCentral.Modelos;
using SeguridadCentral.Nucleo;
using SeguridadCentral.Utilidades;
using System;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace SeguridadCentral.Validaciones
{
    [ComVisible(true)]
    public class ValidadorHardware : IValidador
    {
        private readonly Registrador registrador;
        private readonly AyudanteCriptografia crypto;

        public ValidadorHardware()
        {
            registrador = new Registrador();
            crypto = new AyudanteCriptografia();
        }

        // =====================================================
        //  IMPLEMENTACIÓN DE IValidador
        // =====================================================
        public DetalleValidacion Validar()
        {
            var detalle = new DetalleValidacion
            {
                NombreValidador = "Validador Hardware",
                EsValido = true,
                Mensaje = "Hardware válido",
                FechaValidacion = DateTime.Now
            };

            try
            {
                // Usamos el método que arma toda la info
                InformacionHardware info = ObtenerInformacionDetallada();

                // Al menos 2 identificadores “fuertes” (excluyendo los que tienen "S/N")
                int señalesFuertes = ContarIdentificadoresFuertes(
                    info.CPU, info.DISK, info.BIOS, info.BOARD, info.GUID);

                if (señalesFuertes < 2)
                {
                    detalle.EsValido = false;
                    detalle.Mensaje = "No se pudo identificar de forma fiable este equipo.";
                    return detalle;
                }

                // Cadena clásica para compatibilidad con VBA / logs
                detalle.InformacionAdicional =
                    "CPU=" + info.CPU +
                    "; DISK=" + info.DISK +
                    "; BIOS=" + info.BIOS +
                    "; BOARD=" + info.BOARD +
                    "; GUID=" + info.GUID +
                    "; MAC=" + info.MAC +
                    "; FINGERPRINT=" + info.FINGERPRINT;

                registrador.RegistrarEvento(
                    "Validación hardware exitosa - FP=" + info.FINGERPRINT);
            }
            catch (Exception ex)
            {
                detalle.EsValido = false;
                detalle.Mensaje = "Error en validación de hardware: " + ex.Message;
                registrador.RegistrarError("ValidadorHardware", ex);
            }

            return detalle;
        }

        // =====================================================
        //  NUEVO: OBJETO CON TODA LA INFORMACIÓN
        // =====================================================
        /// <summary>
        /// Devuelve toda la información de hardware en un objeto
        /// (CPU, DISK, BIOS, BOARD, GUID, MAC, FINGERPRINT) más
        /// SistemaOperativo, UsuarioWindows y ZonaHoraria.
        /// </summary>
        internal InformacionHardware ObtenerInformacionDetallada()
        {
            var info = new InformacionHardware();

            // Identificadores crudos normalizados
            string cpu = Normalizar(ObtenerProcessorId());
            string disk = Normalizar(ObtenerDiskSerial());
            string bios = Normalizar(ObtenerBiosSerial());
            string board = Normalizar(ObtenerBaseBoardSerial());
            string guid = Normalizar(ObtenerMachineGuid());
            string mac = Normalizar(ObtenerMacAddress());

            // Aplicar valores por defecto si están vacíos
            info.CPU = string.IsNullOrEmpty(cpu) ? "S/N" : cpu;
            info.DISK = string.IsNullOrEmpty(disk) ? "S/S" : disk;
            info.BIOS = string.IsNullOrEmpty(bios) ? "S/N" : bios;
            info.BOARD = string.IsNullOrEmpty(board) ? "S/S" : board;
            info.GUID = string.IsNullOrEmpty(guid) ? "S/N" : guid;
            info.MAC = string.IsNullOrEmpty(mac) ? "S/MAC" : mac;

            // Fingerprint = hash de todos los identificadores (usando valores reales, no los "S/N")
            string compuesto = string.Join("|", new[] { cpu, disk, bios, board, guid, mac });
            info.FINGERPRINT = crypto.GenerarHash(compuesto);

            // Datos adicionales de entorno
            info.SistemaOperativo = RuntimeInformation.OSDescription;
            info.UsuarioWindows = Environment.UserName;
            info.ZonaHoraria = TimeZoneInfo.Local.DisplayName;

            return info;
        }

        // ================== HELPERS INTERNOS ==================

        private static string Normalizar(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return string.Empty;

            return valor.Trim().ToUpperInvariant();
        }

        private static int ContarIdentificadoresFuertes(params string[] valores)
        {
            if (valores == null) return 0;

            int count = 0;
            foreach (string v in valores)
            {
                // No contar los que tienen "S/N", "S/S", etc.
                if (!string.IsNullOrEmpty(v) &&
                    !v.StartsWith("S/N") &&
                    !v.StartsWith("S/S") &&
                    !v.StartsWith("S/MAC"))
                {
                    count++;
                }
            }
            return count;
        }

        private string ObtenerProcessorId()
        {
            try
            {
                using (var searcher =
                    new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
                {
                    foreach (ManagementBaseObject obj in searcher.Get())
                    {
                        object value = obj["ProcessorId"];
                        if (value != null)
                        {
                            string processorId = value.ToString();
                            if (!string.IsNullOrWhiteSpace(processorId))
                                return processorId;
                        }
                    }
                }
            }
            catch { }
            return string.Empty;
        }

        private string ObtenerDiskSerial()
        {
            try
            {
                using (var searcher =
                    new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_PhysicalMedia"))
                {
                    foreach (ManagementBaseObject obj in searcher.Get())
                    {
                        object value = obj["SerialNumber"];
                        if (value != null)
                        {
                            string serial = value.ToString();
                            if (!string.IsNullOrWhiteSpace(serial))
                                return serial;
                        }
                    }
                }

                // Intentar con Win32_DiskDrive como fallback
                using (var searcher =
                    new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_DiskDrive"))
                {
                    foreach (ManagementBaseObject obj in searcher.Get())
                    {
                        object value = obj["SerialNumber"];
                        if (value != null)
                        {
                            string serial = value.ToString();
                            if (!string.IsNullOrWhiteSpace(serial))
                                return serial;
                        }
                    }
                }
            }
            catch { }
            return string.Empty;
        }

        private string ObtenerBiosSerial()
        {
            try
            {
                using (var searcher =
                    new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS"))
                {
                    foreach (ManagementBaseObject obj in searcher.Get())
                    {
                        object value = obj["SerialNumber"];
                        if (value != null)
                        {
                            string serial = value.ToString();
                            if (!string.IsNullOrWhiteSpace(serial))
                                return serial;
                        }
                    }
                }
            }
            catch { }
            return string.Empty;
        }

        private string ObtenerBaseBoardSerial()
        {
            try
            {
                using (var searcher =
                    new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard"))
                {
                    foreach (ManagementBaseObject obj in searcher.Get())
                    {
                        object value = obj["SerialNumber"];
                        if (value != null)
                        {
                            string serial = value.ToString();
                            if (!string.IsNullOrWhiteSpace(serial))
                                return serial;
                        }
                    }
                }
            }
            catch { }
            return string.Empty;
        }

        private string ObtenerMachineGuid()
        {
            try
            {
                using (RegistryKey key =
                    Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
                {
                    if (key != null)
                    {
                        object value = key.GetValue("MachineGuid");
                        if (value != null)
                        {
                            string guid = value.ToString();
                            if (!string.IsNullOrWhiteSpace(guid))
                                return guid;
                        }
                    }
                }
            }
            catch { }
            return string.Empty;
        }

        private string ObtenerMacAddress()
        {
            try
            {
                NetworkInterface[] interfaces =
                    NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface ni in interfaces)
                {
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                        continue;
                    if (ni.OperationalStatus != OperationalStatus.Up)
                        continue;

                    PhysicalAddress pa = ni.GetPhysicalAddress();
                    if (pa != null)
                    {
                        string mac = pa.ToString();
                        if (!string.IsNullOrWhiteSpace(mac))
                            return mac;
                    }
                }
            }
            catch { }
            return string.Empty;
        }
    }
}
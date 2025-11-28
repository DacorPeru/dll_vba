using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Security.Cryptography;
using System.Text;

namespace SeguridadCentral.Nucleo
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class ManejadorConfiguracion
    {
        private const string CLAVE_REGISTRO = @"SOFTWARE\SeguridadCentralVBA";
        private Registrador registrador;

        public ManejadorConfiguracion()
        {
            registrador = new Registrador();
        }

        [ComVisible(true)]
        public bool GuardarConfiguracion(string clave, string valor, bool encriptar = true)
        {
            try
            {
                string valorGuardar = encriptar ? Encriptar(valor) : valor;

                using (var key = Registry.CurrentUser.CreateSubKey(CLAVE_REGISTRO))
                {
                    key.SetValue(clave, valorGuardar);
                    return true;
                }
            }
            catch (Exception ex)
            {
                registrador.RegistrarError($"GuardarConfiguracion-{clave}", ex);
                return false;
            }
        }

        [ComVisible(true)]
        public string LeerConfiguracion(string clave, bool desencriptar = true)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(CLAVE_REGISTRO))
                {
                    var valor = key?.GetValue(clave)?.ToString() ?? "";
                    return desencriptar ? Desencriptar(valor) : valor;
                }
            }
            catch (Exception ex)
            {
                registrador.RegistrarError($"LeerConfiguracion-{clave}", ex);
                return "";
            }
        }

        [ComVisible(true)]
        public bool EliminarConfiguracion(string clave)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(CLAVE_REGISTRO))
                {
                    key.DeleteValue(clave, false);
                    return true;
                }
            }
            catch (Exception ex)
            {
                registrador.RegistrarError($"EliminarConfiguracion-{clave}", ex);
                return false;
            }
        }

        private string Encriptar(string texto)
        {
            try
            {
                byte[] textoBytes = Encoding.UTF8.GetBytes(texto);
                byte[] protegido = ProtectedData.Protect(textoBytes, null, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(protegido);
            }
            catch
            {
                return texto;
            }
        }

        private string Desencriptar(string textoEncriptado)
        {
            try
            {
                byte[] datosEncriptados = Convert.FromBase64String(textoEncriptado);
                byte[] datosOriginales = ProtectedData.Unprotect(datosEncriptados, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(datosOriginales);
            }
            catch
            {
                return textoEncriptado;
            }
        }
    }
}
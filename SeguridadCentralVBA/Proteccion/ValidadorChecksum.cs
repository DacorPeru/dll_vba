using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using SeguridadCentral.Nucleo;

namespace SeguridadCentral.Proteccion
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class ValidadorChecksum
    {
        private Registrador registrador;
        private string checksumEsperado;

        public ValidadorChecksum()
        {
            registrador = new Registrador();
            // En producción, esto debería venir de una fuente segura
            checksumEsperado = CalcularChecksumAssembly();
        }

        [ComVisible(true)]
        public bool VerificarChecksumAssembly()
        {
            try
            {
                string checksumActual = CalcularChecksumAssembly();
                bool valido = checksumActual == checksumEsperado;

                if (!valido)
                {
                    registrador.RegistrarEvento($"Checksum inválido. Esperado: {checksumEsperado}, Actual: {checksumActual}");
                }

                return valido;
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("ValidadorChecksum-Verificar", ex);
                return false;
            }
        }

        [ComVisible(true)]
        public string CalcularChecksumAssembly()
        {
            try
            {
                string assemblyPath = Assembly.GetExecutingAssembly().Location;
                if (!File.Exists(assemblyPath)) return "ERROR: Assembly no encontrado";

                using (var sha256 = SHA256.Create())
                using (var stream = File.OpenRead(assemblyPath))
                {
                    byte[] hash = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("ValidadorChecksum-Calcular", ex);
                return $"ERROR: {ex.Message}";
            }
        }

        [ComVisible(true)]
        public bool VerificarChecksumArchivo(string rutaArchivo, string checksumEsperado)
        {
            try
            {
                if (!File.Exists(rutaArchivo)) return false;

                using (var sha256 = SHA256.Create())
                using (var stream = File.OpenRead(rutaArchivo))
                {
                    byte[] hash = sha256.ComputeHash(stream);
                    string checksumActual = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    return checksumActual == checksumEsperado.ToLowerInvariant();
                }
            }
            catch (Exception ex)
            {
                registrador.RegistrarError($"ValidadorChecksum-Archivo-{rutaArchivo}", ex);
                return false;
            }
        }

        [ComVisible(true)]
        public void ActualizarChecksumEsperado(string nuevoChecksum)
        {
            checksumEsperado = nuevoChecksum;
            registrador.RegistrarEvento("Checksum esperado actualizado");
        }
    }
}
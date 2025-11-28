using System;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Text;
using SeguridadCentral.Nucleo;

namespace SeguridadCentral.Utilidades
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class AyudanteCriptografia
    {
        private const string CLAVE_BASE = "SeguridadCentralVBA2024!Secure#Key";
        private Registrador registrador;

        public AyudanteCriptografia()
        {
            registrador = new Registrador();
        }

        [ComVisible(true)]
        public string Encriptar(string texto)
        {
            try
            {
                if (string.IsNullOrEmpty(texto)) return texto;

                using (Aes aes = Aes.Create())
                {
                    aes.Key = DeriveKey(CLAVE_BASE);
                    aes.IV = new byte[16]; // IV simple para compatibilidad

                    using (var encryptor = aes.CreateEncryptor())
                    {
                        byte[] textoBytes = Encoding.UTF8.GetBytes(texto);
                        byte[] encriptado = encryptor.TransformFinalBlock(textoBytes, 0, textoBytes.Length);
                        return Convert.ToBase64String(encriptado);
                    }
                }
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("AyudanteCriptografia-Encriptar", ex);
                return texto;
            }
        }

        [ComVisible(true)]
        public string Desencriptar(string textoEncriptado)
        {
            try
            {
                if (string.IsNullOrEmpty(textoEncriptado)) return textoEncriptado;

                using (Aes aes = Aes.Create())
                {
                    aes.Key = DeriveKey(CLAVE_BASE);
                    aes.IV = new byte[16];

                    using (var decryptor = aes.CreateDecryptor())
                    {
                        byte[] textoBytes = Convert.FromBase64String(textoEncriptado);
                        byte[] desencriptado = decryptor.TransformFinalBlock(textoBytes, 0, textoBytes.Length);
                        return Encoding.UTF8.GetString(desencriptado);
                    }
                }
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("AyudanteCriptografia-Desencriptar", ex);
                return textoEncriptado;
            }
        }

        [ComVisible(true)]
        public string GenerarHash(string texto)
        {
            try
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(texto));
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("AyudanteCriptografia-GenerarHash", ex);
                return "";
            }
        }

        [ComVisible(true)]
        public bool VerificarHash(string texto, string hash)
        {
            try
            {
                string hashCalculado = GenerarHash(texto);
                return string.Equals(hashCalculado, hash, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("AyudanteCriptografia-VerificarHash", ex);
                return false;
            }
        }

        [ComVisible(true)]
        public string GenerarSalt(int longitud = 32)
        {
            try
            {
                byte[] salt = new byte[longitud];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }
                return Convert.ToBase64String(salt);
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("AyudanteCriptografia-GenerarSalt", ex);
                return "salt_por_defecto";
            }
        }

        private byte[] DeriveKey(string clave)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(clave, new byte[8], 1000, HashAlgorithmName.SHA256))
            {
                return deriveBytes.GetBytes(32); // 256 bits
            }
        }
    }
}
using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace SeguridadCentral.Utilidades
{
    [ComVisible(true)]
    public static class MetodosExtension
    {
        [ComVisible(true)]
        public static string ToSecureString(this string texto)
        {
            if (string.IsNullOrEmpty(texto)) return texto;

            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(texto);
                Array.Reverse(bytes); // Reversa simple
                return Convert.ToBase64String(bytes);
            }
            catch
            {
                return texto;
            }
        }

        [ComVisible(true)]
        public static string FromSecureString(this string textoSeguro)
        {
            if (string.IsNullOrEmpty(textoSeguro)) return textoSeguro;

            try
            {
                byte[] bytes = Convert.FromBase64String(textoSeguro);
                Array.Reverse(bytes);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return textoSeguro;
            }
        }

        [ComVisible(true)]
        public static bool EsFechaValida(this DateTime fecha)
        {
            return fecha > new DateTime(2000, 1, 1) && fecha < new DateTime(2030, 12, 31);
        }

        [ComVisible(true)]
        public static string ToHexString(this byte[] bytes)
        {
            if (bytes == null) return "";
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }

        [ComVisible(true)]
        public static byte[] FromHexString(this string hex)
        {
            if (string.IsNullOrEmpty(hex) || hex.Length % 2 != 0)
                return new byte[0];

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        [ComVisible(true)]
        public static string ObtenerHashSeguro(this string texto, string salt = "")
        {
            if (string.IsNullOrEmpty(texto)) return "";

            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(texto + salt);
                byte[] hash = sha256.ComputeHash(bytes);
                return hash.ToHexString();
            }
        }
    }
}
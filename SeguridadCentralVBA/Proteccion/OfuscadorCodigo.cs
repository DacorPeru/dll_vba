using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using SeguridadCentral.Nucleo;

namespace SeguridadCentral.Proteccion
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class OfuscadorCodigo
    {
        private Registrador registrador;

        public OfuscadorCodigo()
        {
            registrador = new Registrador();
        }

        [ComVisible(true)]
        public string OfuscarString(string texto)
        {
            try
            {
                if (string.IsNullOrEmpty(texto)) return texto;

                // Convertir a bytes
                byte[] bytes = Encoding.UTF8.GetBytes(texto);

                // Aplicar XOR con clave aleatoria
                byte clave = 0xAB;
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] ^= clave;
                    clave = (byte)((clave * 3 + i) % 256);
                }

                return Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("Ofuscador-String", ex);
                return texto;
            }
        }

        [ComVisible(true)]
        public string DesofuscarString(string textoOfuscado)
        {
            try
            {
                if (string.IsNullOrEmpty(textoOfuscado)) return textoOfuscado;

                byte[] bytes = Convert.FromBase64String(textoOfuscado);

                // Revertir XOR
                byte clave = 0xAB;
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] ^= clave;
                    clave = (byte)((clave * 3 + i) % 256);
                }

                return Encoding.UTF8.GetString(bytes);
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("Ofuscador-Desofuscar", ex);
                return textoOfuscado;
            }
        }

        [ComVisible(true)]
        public byte[] OfuscarBytes(byte[] datos)
        {
            try
            {
                if (datos == null || datos.Length == 0) return datos;

                using (Aes aes = Aes.Create())
                {
                    // Usar clave derivada del assembly
                    byte[] clave = DeriveKeyFromAssembly();
                    byte[] iv = new byte[16];
                    Array.Copy(clave, iv, 16);

                    using (var encryptor = aes.CreateEncryptor(clave, iv))
                    {
                        return encryptor.TransformFinalBlock(datos, 0, datos.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("Ofuscador-Bytes", ex);
                return datos;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private byte[] DeriveKeyFromAssembly()
        {
            // Derivar clave única del assembly actual
            string assemblyInfo = System.Reflection.Assembly.GetExecutingAssembly().FullName;
            using (SHA256 sha = SHA256.Create())
            {
                return sha.ComputeHash(Encoding.UTF8.GetBytes(assemblyInfo));
            }
        }

        [ComVisible(true)]
        public void OfuscarMetodos()
        {
            // Este método aplica técnicas de ofuscación en tiempo de ejecución
            try
            {
                // Forzar recompilación JIT con diferentes opciones
                RuntimeHelpers.PrepareMethod(typeof(OfuscadorCodigo).GetMethod("DeriveKeyFromAssembly").MethodHandle);

                registrador.RegistrarEvento("Ofuscación de métodos aplicada");
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("Ofuscador-Metodos", ex);
            }
        }
    }
}
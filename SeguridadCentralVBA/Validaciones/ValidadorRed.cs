using SeguridadCentral.Modelos;
using SeguridadCentral.Nucleo;
using System;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace SeguridadCentral.Validaciones
{
    [ComVisible(true)]
    public class ValidadorRed : IValidador
    {
        private readonly Registrador registrador;

        // Host de prueba para verificar salida a Internet.
        // Cámbialo por tu dominio/licenciamiento si quieres algo más corporativo.
        private const string HostPruebaInternet = "8.8.8.8"; // Google DNS

        public ValidadorRed()
        {
            registrador = new Registrador();
        }

        public DetalleValidacion Validar()
        {
            var detalle = new DetalleValidacion
            {
                NombreValidador = "Validador Red",
                EsValido = false,                      // ⬅️ por defecto NO es válido
                Mensaje = "Validación de red no iniciada.",
                FechaValidacion = DateTime.Now
            };

            try
            {
                // 1. ¿Hay alguna interfaz de red disponible en general?
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    detalle.Mensaje = "No hay conexión de red disponible. " +
                                      "Se requiere conexión a Internet para usar esta aplicación.";
                    registrador.RegistrarEvento("ValidadorRed: sin red disponible.");
                    return detalle;
                }

                // 2. ¿Existe al menos una interfaz "real" activa?
                bool interfazValida = false;

                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                        ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                        continue;

                    if (ni.OperationalStatus == OperationalStatus.Up)
                    {
                        interfazValida = true;
                        break;
                    }
                }

                if (!interfazValida)
                {
                    detalle.Mensaje = "No se encontró ninguna interfaz de red activa. " +
                                      "Verifique su adaptador de red.";
                    registrador.RegistrarEvento("ValidadorRed: sin interfaces activas.");
                    return detalle;
                }

                // 3. Prueba de salida a Internet (ping a un host conocido)
                using (var ping = new Ping())
                {
                    PingReply reply = ping.Send(HostPruebaInternet, 2000); // 2 s de timeout

                    if (reply == null || reply.Status != IPStatus.Success)
                    {
                        detalle.Mensaje = "No se pudo establecer conexión a Internet. " +
                                          "Verifique su acceso a la red antes de continuar.";
                        registrador.RegistrarEvento($"ValidadorRed: ping fallido a {HostPruebaInternet}. Estado={reply?.Status}");
                        return detalle;
                    }
                }

                // 4. Todo OK → validación de red aprobada
                detalle.EsValido = true;
                detalle.Mensaje = "Conexión de red e Internet verificadas correctamente.";
                registrador.RegistrarEvento("ValidadorRed: validación de red exitosa.");
            }
            catch (Exception ex)
            {
                detalle.EsValido = false;
                detalle.Mensaje = "Error en validación de red: " + ex.Message;
                registrador.RegistrarError("ValidadorRed", ex);
            }

            return detalle;
        }
    }
}

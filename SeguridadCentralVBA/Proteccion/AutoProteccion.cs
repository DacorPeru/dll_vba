// ❌ ELIMINAR ESTE USING:
// using SeguridadCentral.Nucleo;

// ✅ MANTENER SOLO ESTOS:
using SeguridadCentral.Nucleo;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

// ✅ CAMBIAR NAMESPACE:
namespace SeguridadCentralVBA
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class AutoProteccion
    {
        private AntiDepuracion antiDepuracion;
        private Registrador registrador;
        private bool proteccionesActivas;

        public AutoProteccion()
        {
            antiDepuracion = new AntiDepuracion();
            registrador = new Registrador();
            proteccionesActivas = false;
        }

        [ComVisible(true)]
        public void ActivarProtecciones()
        {
            if (proteccionesActivas) return;

            try
            {
                // Activar anti-depuración
                antiDepuracion.ActivarProtecciones();

                // Iniciar monitoreo en segundo plano
                Thread monitoreoThread = new Thread(MonitorearSeguridad)
                {
                    IsBackground = true,
                    Name = "MonitoreoSeguridad"
                };
                monitoreoThread.Start();

                proteccionesActivas = true;
                registrador.RegistrarEvento("Protecciones de seguridad activadas");
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("AutoProteccion-Activar", ex);
            }
        }

        [ComVisible(true)]
        public void DesactivarProtecciones()
        {
            try
            {
                antiDepuracion.DesactivarProtecciones();
                proteccionesActivas = false;
                registrador.RegistrarEvento("Protecciones de seguridad desactivadas");
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("AutoProteccion-Desactivar", ex);
            }
        }

        private void MonitorearSeguridad()
        {
            while (proteccionesActivas)
            {
                try
                {
                    // Verificar depurador cada 5 segundos
                    if (antiDepuracion.DetectarDepurador())
                    {
                        registrador.RegistrarEvento("Depurador detectado - Tomando medidas");
                        ResponderAmenaza();
                    }

                    Thread.Sleep(5000);
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    registrador.RegistrarError("AutoProteccion-Monitoreo", ex);
                }
            }
        }

        private void ResponderAmenaza()
        {
            try
            {
                // Medidas de respuesta ante amenazas detectadas
                registrador.RegistrarEvento("Ejecutando respuesta ante amenaza");

                // Cerrar aplicación de manera segura
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("AutoProteccion-Respuesta", ex);
            }
        }
    }
}
// ❌ ELIMINAR: using SeguridadCentral.Nucleo;
// ❌ ELIMINAR: using SeguridadCentral.Utilidades;

using SeguridadCentral.Nucleo;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

// ✅ CAMBIAR namespace
namespace SeguridadCentralVBA
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class AntiDepuracion
    {
        private MetodosNativos metodosNativos;
        private Registrador registrador;
        private Timer timerVerificacion;
        private bool proteccionesActivas;

        public AntiDepuracion()
        {
            metodosNativos = new MetodosNativos();
            registrador = new Registrador();
            proteccionesActivas = false;
        }

        [ComVisible(true)]
        public void ActivarProtecciones()
        {
            if (proteccionesActivas) return;

            try
            {
                // Configurar timer de verificación periódica
                timerVerificacion = new Timer(VerificarEstadoSeguridad, null, 0, 3000);
                proteccionesActivas = true;

                // Aplicar técnicas anti-depuración inmediatas
                AplicarTecnicasAntiDepuracion();

                registrador.RegistrarEvento("Protecciones anti-depuración activadas");
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("AntiDepuracion-Activar", ex);
            }
        }

        [ComVisible(true)]
        public void DesactivarProtecciones()
        {
            try
            {
                timerVerificacion?.Dispose();
                proteccionesActivas = false;
                registrador.RegistrarEvento("Protecciones anti-depuración desactivadas");
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("AntiDepuracion-Desactivar", ex);
            }
        }

        [ComVisible(true)]
        public bool DetectarDepurador()
        {
            try
            {
                // ✅ ACTUALIZADO: Usar nuevos nombres de métodos
                return Debugger.IsAttached ||
                       metodosNativos.VerificarDepuradorPresente() ||  // ✅ CAMBIADO
                       CheckRemoteDebugger() ||
                       CheckDebuggerViaAPI();
            }
            catch
            {
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AplicarTecnicasAntiDepuracion()
        {
            try
            {
                // ✅ ACTUALIZADO: Usar nuevo nombre
                if (metodosNativos.VerificarBreakpointsHardware())  // ✅ CAMBIADO
                {
                    ResponderBreakpointDetectado();
                }

                // Ofuscar pila de llamadas
                OfuscarPilaLlamadas();

                // Verificar tiempo de ejecución (análisis de performance)
                VerificarTiempoEjecucion();
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("AntiDepuracion-Tecnicas", ex);
            }
        }

        private void VerificarEstadoSeguridad(object state)
        {
            if (!proteccionesActivas) return;

            try
            {
                if (DetectarDepurador())
                {
                    registrador.RegistrarEvento("Depurador detectado en verificación periódica");
                    ResponderDepuradorDetectado();
                }
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("AntiDepuracion-Verificacion", ex);
            }
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private void OfuscarPilaLlamadas()
        {
            // Crear métodos dummy para ofuscar el análisis
            for (int i = 0; i < 10; i++)
            {
                DummyMethod(i);
            }
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        private void DummyMethod(int seed)
        {
            var random = new Random(seed);
            byte[] buffer = new byte[100];
            random.NextBytes(buffer);
        }

        private void VerificarTiempoEjecucion()
        {
            var start = DateTime.Now;
            // Operación que debería tomar tiempo conocido
            Thread.Sleep(10);
            var elapsed = DateTime.Now - start;

            if (elapsed.TotalMilliseconds > 100) // Demasiado tiempo = posible depuración
            {
                registrador.RegistrarEvento("Anomalía de tiempo de ejecución detectada");
            }
        }

        private bool CheckRemoteDebugger()
        {
            try
            {
                // ✅ ACTUALIZADO: Usar nuevo nombre
                return metodosNativos.VerificarDepuradorRemoto(Process.GetCurrentProcess().Handle, out bool isDebugged) && isDebugged;  // ✅ CAMBIADO
            }
            catch
            {
                return false;
            }
        }

        private bool CheckDebuggerViaAPI()
        {
            // ✅ ACTUALIZADO: Usar nuevo nombre
            return metodosNativos.VerificarDepuradorPresente();  // ✅ CAMBIADO
        }

        private void ResponderDepuradorDetectado()
        {
            // Respuesta ante detección de depurador
            try
            {
                Environment.Exit(1);
            }
            catch
            {
                // Fallback si Exit falla
                Process.GetCurrentProcess().Kill();
            }
        }

        private void ResponderBreakpointDetectado()
        {
            registrador.RegistrarEvento("Hardware breakpoint detectado");
            // Podría limpiar breakpoints o terminar ejecución
        }
    }
}
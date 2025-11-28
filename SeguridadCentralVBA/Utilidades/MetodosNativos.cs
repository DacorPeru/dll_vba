using SeguridadCentral.Nucleo;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SeguridadCentralVBA
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class MetodosNativos
    {
        private Registrador registrador;

        // Importaciones de Windows API (MANTENER ORIGINAL)
        [DllImport("kernel32.dll")]
        private static extern bool IsDebuggerPresent();

        [DllImport("kernel32.dll")]
        private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, out bool pbDebuggerPresent);

        [DllImport("kernel32.dll")]
        private static extern void OutputDebugString(string lpOutputString);

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        public MetodosNativos()
        {
            registrador = new Registrador();
        }

        // ✅ CAMBIAR NOMBRE DEL MÉTODO PÚBLICO
        [ComVisible(true)]
        public bool VerificarDepuradorPresente()
        {
            try
            {
                return IsDebuggerPresent(); // Llama a la función nativa
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("MetodosNativos-VerificarDepurador", ex);
                return false;
            }
        }

        // ✅ CAMBIAR NOMBRE DEL MÉTODO PÚBLICO
        [ComVisible(true)]
        public bool VerificarDepuradorRemoto(IntPtr processHandle, out bool isDebugged)
        {
            isDebugged = false;
            try
            {
                return CheckRemoteDebuggerPresent(processHandle, out isDebugged);
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("MetodosNativos-VerificarDepuradorRemoto", ex);
                return false;
            }
        }

        // El resto del código se mantiene igual...
        [ComVisible(true)]
        public bool VerificarBreakpointsHardware()
        {
            try
            {
                // Esta es una técnica avanzada que requiere inline assembly
                // En C# puro es más difícil implementarla completamente
                return false;
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("MetodosNativos-VerificarBreakpointsHardware", ex);
                return false;
            }
        }

        [ComVisible(true)]
        public uint ObtenerTiempoInactividad()
        {
            try
            {
                LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
                lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
                lastInputInfo.dwTime = 0;

                if (GetLastInputInfo(ref lastInputInfo))
                {
                    return (uint)Environment.TickCount - lastInputInfo.dwTime;
                }

                return 0;
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("MetodosNativos-ObtenerTiempoInactividad", ex);
                return 0;
            }
        }

        [ComVisible(true)]
        public bool EstaEstacionTrabajoBloqueada()
        {
            try
            {
                IntPtr hwnd = GetForegroundWindow();
                if (hwnd == IntPtr.Zero)
                    return true; // Posiblemente bloqueado

                // Intentar obtener título de ventana
                System.Text.StringBuilder sb = new System.Text.StringBuilder(256);
                int length = GetWindowText(hwnd, sb, sb.Capacity);
                return length == 0; // Si no hay título, posible pantalla de bloqueo
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("MetodosNativos-EstaEstacionBloqueada", ex);
                return false;
            }
        }

        [ComVisible(true)]
        public void DebugOutput(string mensaje)
        {
            try
            {
                OutputDebugString($"[SeguridadCentral] {mensaje}");
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("MetodosNativos-DebugOutput", ex);
            }
        }
    }
}
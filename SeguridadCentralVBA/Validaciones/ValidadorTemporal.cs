using SeguridadCentral.Modelos;
using SeguridadCentral.Nucleo;
using SeguridadCentral.Utilidades;
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace SeguridadCentral.Validaciones
{
    [ComVisible(true)]
    public class ValidadorTemporal : IValidador
    {
        private readonly Registrador registrador;
        private readonly ManejadorConfiguracion manejadorConfig;

        private const string ClaveUltimaValidacion = "UltimaValidacionUtc";
        private const string ClaveUltimoTickMs = "UltimoTickMs";

        // Margen de deriva “normal” entre reloj del sistema y tiempo real medido por tick.
        private static readonly TimeSpan MargenDerivaNormal = TimeSpan.FromMinutes(10);

        // Deriva máxima aceptable: por encima de esto consideramos manipulación.
        private static readonly TimeSpan MargenDerivaMaxima = TimeSpan.FromHours(6);

        public ValidadorTemporal()
        {
            registrador = new Registrador();
            manejadorConfig = new ManejadorConfiguracion();
        }

        public DetalleValidacion Validar()
        {
            var detalle = new DetalleValidacion
            {
                NombreValidador = "Validador Temporal",
                EsValido = true,
                Mensaje = "OK",
                FechaValidacion = DateTime.UtcNow
            };

            try
            {
                DateTime ahoraUtc = DateTime.UtcNow;
                long tickActual = TiempoSistema.ObtenerTickMs();   // GetTickCount64 (reloj monotónico)

                // Leer última fecha y último tick desde configuración (encriptado)
                string ultimoValorFecha = manejadorConfig.LeerConfiguracion(ClaveUltimaValidacion, true);
                string ultimoValorTick = manejadorConfig.LeerConfiguracion(ClaveUltimoTickMs, true);

                // 0) Primera vez: no hay datos => guardamos y permitimos
                if (string.IsNullOrWhiteSpace(ultimoValorFecha) ||
                    string.IsNullOrWhiteSpace(ultimoValorTick))
                {
                    manejadorConfig.GuardarConfiguracion(
                        ClaveUltimaValidacion,
                        ahoraUtc.ToString("o", CultureInfo.InvariantCulture),
                        true
                    );
                    manejadorConfig.GuardarConfiguracion(
                        ClaveUltimoTickMs,
                        tickActual.ToString(CultureInfo.InvariantCulture),
                        true
                    );

                    registrador.RegistrarEvento("Primera validación temporal registrada.");
                    return detalle;
                }

                // 1) Parse de valores guardados
                if (!DateTime.TryParseExact(
                        ultimoValorFecha,
                        "o",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out DateTime ultimaValidacionUtc))
                {
                    detalle.EsValido = false;
                    detalle.Mensaje = "Información temporal de seguridad dañada (fecha).";
                    registrador.RegistrarError(
                        "ValidadorTemporal-ParseUltimaFecha",
                        new Exception("Formato de fecha inválido en configuración.")
                    );
                    return detalle;
                }

                if (!long.TryParse(ultimoValorTick, NumberStyles.Integer, CultureInfo.InvariantCulture, out long ultimoTickMs))
                {
                    detalle.EsValido = false;
                    detalle.Mensaje = "Información temporal de seguridad dañada (tick).";
                    registrador.RegistrarError(
                        "ValidadorTemporal-ParseUltimoTick",
                        new Exception("Formato de tick inválido en configuración.")
                    );
                    return detalle;
                }

                // 2) Comparación reloj vs tiempo real
                long diffTicks = tickActual - ultimoTickMs;
                TimeSpan tiempoReloj = ahoraUtc - ultimaValidacionUtc;

                // Si el tick no es confiable (reinicio, overflow, etc.), no acusamos manipulación;
                // simplemente registramos y aceptamos, actualizando referencia.
                // (evita falsos positivos después de reiniciar Windows)
                bool tickConfiable = diffTicks > 0 && diffTicks < long.MaxValue / 2;

                if (tickConfiable)
                {
                    TimeSpan tiempoReal = TimeSpan.FromMilliseconds(diffTicks);
                    TimeSpan diferencia = tiempoReloj - tiempoReal; // positivo = reloj adelantado

                    // RETRASO SOSPECHOSO (reloj muy por detrás de lo que permite el tiempo real)
                    if (diferencia < -MargenDerivaMaxima)
                    {
                        DateTime ultimaLocal = ultimaValidacionUtc.ToLocalTime();
                        DateTime ahoraLocal = ahoraUtc.ToLocalTime();

                        detalle.EsValido = false;
                        detalle.Mensaje =
                            "Se ha detectado manipulación de la fecha/hora del sistema (retraso)." +
                            $" Última fecha segura: {ultimaLocal:dd/MM/yyyy HH:mm:ss} (hora local)." +
                            $" Fecha actual del sistema: {ahoraLocal:dd/MM/yyyy HH:mm:ss} (hora local).";

                        registrador.RegistrarEvento(
                            $"Bloqueo por retraso sospechoso. Ultima={ultimaValidacionUtc:o}, Actual={ahoraUtc:o}, " +
                            $"TiempoReloj={tiempoReloj}, TiempoReal={tiempoReal}, Diferencia={diferencia}"
                        );
                        return detalle;
                    }

                    // ADELANTO SOSPECHOSO (reloj muy por delante del tiempo real)
                    if (diferencia > MargenDerivaMaxima)
                    {
                        DateTime ultimaLocal = ultimaValidacionUtc.ToLocalTime();
                        DateTime ahoraLocal = ahoraUtc.ToLocalTime();

                        detalle.EsValido = false;
                        detalle.Mensaje =
                            "Se ha detectado un adelanto anómalo en la fecha/hora del sistema." +
                            $" Última fecha segura: {ultimaLocal:dd/MM/yyyy HH:mm:ss} (hora local)." +
                            $" Fecha actual del sistema: {ahoraLocal:dd/MM/yyyy HH:mm:ss} (hora local).";

                        registrador.RegistrarEvento(
                            $"Bloqueo por adelanto sospechoso. Ultima={ultimaValidacionUtc:o}, Actual={ahoraUtc:o}, " +
                            $"TiempoReloj={tiempoReloj}, TiempoReal={tiempoReal}, Diferencia={diferencia}"
                        );
                        return detalle;
                    }

                    // Deriva moderada pero aceptada (puede ser ajuste manual leve, ntp, etc.)
                    if (diferencia.Duration() > MargenDerivaNormal)
                    {
                        registrador.RegistrarEvento(
                            $"Deriva de reloj fuera de margen normal pero aceptada. Diferencia={diferencia}, " +
                            $"TiempoReloj={tiempoReloj}, TiempoReal={tiempoReal}."
                        );
                    }

                }
                else
                {
                    // Tick no confiable (reinicio del sistema, etc.) → no usamos este ciclo para acusar.
                    registrador.RegistrarEvento(
                        $"Tick no confiable. Ultima={ultimaValidacionUtc:o}, Actual={ahoraUtc:o}, DiffTicks={diffTicks}, TiempoReloj={tiempoReloj}."
                    );
                }

                // 3) Todo correcto → actualizamos referencia temporal y permitimos
                manejadorConfig.GuardarConfiguracion(
                    ClaveUltimaValidacion,
                    ahoraUtc.ToString("o", CultureInfo.InvariantCulture),
                    true
                );
                manejadorConfig.GuardarConfiguracion(
                    ClaveUltimoTickMs,
                    tickActual.ToString(CultureInfo.InvariantCulture),
                    true
                );

                registrador.RegistrarEvento("Validación temporal correcta.");
                return detalle;
            }
            catch (Exception ex)
            {
                detalle.EsValido = false;
                detalle.Mensaje = $"Error interno en validador temporal: {ex.Message}";
                registrador.RegistrarError("ValidadorTemporal", ex);
                return detalle;
            }
        }
    }
}

using SeguridadCentral.Modelos;
using SeguridadCentral.Nucleo;
using SeguridadCentral.Validaciones;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SeguridadCentralVBA
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class MotorSeguridad
    {
        // Campos solo se asignan en el constructor → readonly
        private readonly List<IValidador> validadores;
        private readonly AutoProteccion autoProteccion;
        private readonly Registrador registrador;

        public MotorSeguridad()
        {
            registrador = new Registrador();
            autoProteccion = new AutoProteccion();
            validadores = CrearValidadores();
        }

        // Lista de validadores que se ejecutan en cadena
        private static List<IValidador> CrearValidadores()
        {
            return new List<IValidador>
            {
                new ValidadorTemporal(),
                new ValidadorHardware(),
                new ValidadorRed(),
            };
        }

        [ComVisible(true)]
        public ResultadoValidacion ValidarSistema()
        {
            var resultado = new ResultadoValidacion
            {
                EsValido = true,
                Mensaje = "Sistema seguro",
                FechaValidacion = DateTime.Now
            };

            // Activa protecciones (anti-debug, anti-inyección, etc.)
            autoProteccion.ActivarProtecciones();

            foreach (var validador in validadores)
            {
                var detalle = validador.Validar();
                resultado.Detalles.Add(detalle);

                if (!detalle.EsValido)
                {
                    resultado.EsValido = false;
                    resultado.Mensaje = "Validación fallida";
                    registrador.RegistrarEvento(
                        $"Validación fallida: {detalle.NombreValidador}");
                }
            }

            return resultado;
        }

        [ComVisible(true)]
        public bool VerificarLicencia()
        {
            try
            {
                var resultado = ValidarSistema();
                return resultado.EsValido;
            }
            catch (Exception ex)
            {
                registrador.RegistrarError("VerificarLicencia", ex);
                return false;
            }
        }
    }
}

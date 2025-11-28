using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SeguridadCentral.Modelos
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class ResultadoValidacion
    {
        [ComVisible(true)]
        public bool EsValido { get; set; }

        [ComVisible(true)]
        public string Mensaje { get; set; }

        [ComVisible(true)]
        public DateTime FechaValidacion { get; set; }

        // 👇 OCULTAMOS ESTA PROPIEDAD PARA COM
        [ComVisible(false)]
        public List<DetalleValidacion> Detalles { get; set; }

        public ResultadoValidacion()
        {
            EsValido = true;
            Mensaje = "Validación completada";
            FechaValidacion = DateTime.Now;
            Detalles = new List<DetalleValidacion>();
        }
    }
}

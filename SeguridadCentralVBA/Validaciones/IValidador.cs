using SeguridadCentral.Modelos;
using System.Runtime.InteropServices;

namespace SeguridadCentral.Validaciones
{
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IValidador
    {
        [ComVisible(true)]
        DetalleValidacion Validar();
    }
}
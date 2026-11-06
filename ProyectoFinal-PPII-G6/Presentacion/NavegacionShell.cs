using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.Presentacion
{
    /// <summary>
    /// Navega entre pantallas con el Shell de MAUI. Es la única clase que conoce cómo se navega.
    /// </summary>
    public class NavegacionShell : INavegacion
    {
        /// <summary>
        /// Va a la pestaña que tiene esa ruta.
        /// </summary>
        public Task IrAAsync(string ruta) => Shell.Current.GoToAsync($"//{ruta}");
    }
}
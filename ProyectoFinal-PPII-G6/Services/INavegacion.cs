using System.Threading.Tasks;

namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Permite que un ViewModel pida ir a otra pantalla sin conocer cómo se navega.
    /// </summary>
    public interface INavegacion
    {
        /// <summary>
        /// Va a la pantalla que tiene esa ruta. Las rutas están definidas en AppShell.xaml y, las que usa el código, en Rutas.
        /// </summary>
        Task IrAAsync(string ruta);
    }
}
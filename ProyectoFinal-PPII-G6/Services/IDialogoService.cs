using System.Threading.Tasks;

namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Forma de mostrarle un aviso al usuario. Los ViewModels dependen de esta interfaz y no de MAUI,
    /// así que se pueden probar con un reemplazo que no abra ninguna ventana.
    /// </summary>
    public interface IDialogoService
    {
        /// <summary>
        /// Muestra un aviso con un título y un mensaje, y espera a que el usuario lo cierre.
        /// </summary>
        Task AvisarAsync(string titulo, string mensaje);
    }
}
using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.Presentacion
{
    /// <summary>
    /// Muestra los avisos con los diálogos de MAUI, sobre la página de la ventana actual.
    /// Es el único lugar que conoce la forma de abrir un diálogo.
    /// </summary>
    public class DialogoService : IDialogoService
    {
        /// <summary>
        /// Muestra un aviso con un título y un mensaje. Si todavía no hay ninguna ventana, no hace nada.
        /// </summary>
        public async Task AvisarAsync(string titulo, string mensaje)
        {
            var pagina = Application.Current?.Windows.FirstOrDefault()?.Page;

            if (pagina != null)
                await pagina.DisplayAlertAsync(titulo, mensaje, "OK");
        }
    }
}
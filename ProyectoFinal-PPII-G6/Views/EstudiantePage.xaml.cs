using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.ViewModels;

namespace ProyectoFinal_PPII_G6.Views
{
    /// <summary>
    /// Pantalla de gestión de alumnos: alta, listado y baja.
    /// </summary>
    public partial class EstudiantePage : ContentPage
    {
        /// <summary>
        /// Crea la página y le asigna su ViewModel.
        /// </summary>
        public EstudiantePage(EstudianteViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}

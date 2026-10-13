using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.ViewModels;

namespace ProyectoFinal_PPII_G6.Views
{
    /// <summary>
    /// Pantalla de gestión de alumnos: alta y listado.
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

        /// <summary>
        /// Recarga la lista cada vez que se muestra la pantalla (por ejemplo, después de una carga masiva).
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            (BindingContext as EstudianteViewModel)?.CargarEstudianteCommand.Execute(null);
        }
    }
}

using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.ViewModels;

namespace ProyectoFinal_PPII_G6.Views
{
    /// <summary>
    /// Pantalla de asistencia: registro del día, historial y nivel de riesgo del alumno.
    /// </summary>
    public partial class DetalleEstudiantePage : ContentPage
    {
        /// <summary>
        /// ViewModel de la página.
        /// </summary>
        private readonly DetalleEstudianteViewModel _viewModel;

        /// <summary>
        /// Crea la página y le asigna su ViewModel.
        /// </summary>
        public DetalleEstudiantePage(DetalleEstudianteViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        /// <summary>
        /// Recarga la lista de estudiantes cada vez que se muestra la página.
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel != null)
            {
                await _viewModel.CargarListaEstudiantesAsync();
            }
        }
    }
}

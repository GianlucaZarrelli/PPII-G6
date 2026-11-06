using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.ViewModels;

namespace ProyectoFinal_PPII_G6.Views
{
    /// <summary>
    /// Pantalla del calendario de una comisión: sus clases y las entregas de sus trabajos, ordenadas por fecha.
    /// </summary>
    public partial class CalendarioPage : ContentPage
    {
        private readonly CalendarioViewModel _viewModel;

        /// <summary>
        /// Crea la página y le asigna su ViewModel.
        /// </summary>
        public CalendarioPage(CalendarioViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        /// <summary>
        /// Recarga las comisiones y el calendario cada vez que se muestra la pantalla.
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.CargarComisionesAsync();
        }
    }
}
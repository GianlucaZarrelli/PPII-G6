using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.ViewModels;

namespace ProyectoFinal_PPII_G6.Views
{
    /// <summary>
    /// Pantalla para tomar o corregir la asistencia de una comisión en una fecha.
    /// </summary>
    public partial class AsistenciaFechaPage : ContentPage
    {
        private readonly AsistenciaFechaViewModel _viewModel;

        public AsistenciaFechaPage(AsistenciaFechaViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        /// <summary>
        /// Recarga las comisiones cada vez que se muestra la pantalla.
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.CargarComisionesAsync();
        }
    }
}
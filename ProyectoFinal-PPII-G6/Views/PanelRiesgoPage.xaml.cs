using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.ViewModels;

namespace ProyectoFinal_PPII_G6.Views
{
    /// <summary>
    /// Panel de seguimiento: lista a los alumnos ordenados por riesgo.
    /// </summary>
    public partial class PanelRiesgoPage : ContentPage
    {
        private readonly PanelRiesgoViewModel _viewModel;

        /// <summary>
        /// Crea la página y le asigna su ViewModel.
        /// </summary>
        public PanelRiesgoPage(PanelRiesgoViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        /// <summary>
        /// Recalcula el panel cada vez que se muestra, para reflejar la asistencia recién cargada.
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.CargarAsync();
        }
    }
}
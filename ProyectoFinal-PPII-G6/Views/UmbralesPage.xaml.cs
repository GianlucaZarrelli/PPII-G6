using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.ViewModels;

namespace ProyectoFinal_PPII_G6.Views
{
    /// <summary>
    /// Pantalla donde el tutor configura los umbrales de riesgo sin tocar el código.
    /// </summary>
    public partial class UmbralesPage : ContentPage
    {
        private readonly UmbralesViewModel _viewModel;

        /// <summary>
        /// Crea la página y le asigna su ViewModel.
        /// </summary>
        public UmbralesPage(UmbralesViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        /// <summary>
        /// Recarga los umbrales guardados y el resumen cada vez que se muestra la pantalla.
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.CargarAsync();
        }
    }
}
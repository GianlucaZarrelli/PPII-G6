using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.ViewModels;

namespace ProyectoFinal_PPII_G6.Views
{
    /// <summary>
    /// Menú principal: muestra las métricas, permite cambiar el tema y navegar a los módulos.
    /// </summary>
    public partial class HomePage : ContentPage
    {
        /// <summary>
        /// ViewModel de la página.
        /// </summary>
        private readonly HomeViewModel _viewModel;

        /// <summary>
        /// Crea la página y le asigna su ViewModel.
        /// </summary>
        public HomePage(HomeViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        /// <summary>
        /// Al mostrarse, le avisa al ViewModel para que sincronice el tema y recargue las métricas.
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.CargarMetricasAsync();
        }
    }
}
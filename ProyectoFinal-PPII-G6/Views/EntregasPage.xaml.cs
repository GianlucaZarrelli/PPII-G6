using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.ViewModels;

namespace ProyectoFinal_PPII_G6.Views
{
    /// <summary>
    /// Pantalla para crear trabajos prácticos y registrar quién entregó cada uno.
    /// </summary>
    public partial class EntregasPage : ContentPage
    {
        private readonly EntregasViewModel _viewModel;

        public EntregasPage(EntregasViewModel viewModel)
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
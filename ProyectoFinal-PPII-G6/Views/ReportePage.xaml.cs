using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.ViewModels;

namespace ProyectoFinal_PPII_G6.Views
{
    /// <summary>
    /// Pantalla del reporte de una comisión: cuántos estudiantes están en riesgo de abandonar, con una barra por nivel.
    /// </summary>
    public partial class ReportePage : ContentPage
    {
        private readonly ReporteViewModel _viewModel;

        /// <summary>
        /// Crea la página y le asigna su ViewModel.
        /// </summary>
        public ReportePage(ReporteViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        /// <summary>
        /// Recarga las comisiones y el reporte cada vez que se muestra la pantalla.
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.CargarComisionesAsync();
        }
    }
}
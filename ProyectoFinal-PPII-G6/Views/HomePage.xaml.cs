using System;
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
        /// Evita que el cambio de tema se dispare cuando el switch se actualiza por código.
        /// </summary>
        private bool _isUpdatingSwitch;

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
        /// Al mostrarse, sincroniza el switch con el tema actual y recarga las métricas.
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (Application.Current != null && ThemeSwitch != null)
            {
                // Bloqueamos el evento OnThemeSwitchToggled mientras asignamos el valor inicial
                _isUpdatingSwitch = true;

                // RequestedTheme lee el tema real activo (incluso si UserAppTheme es Unspecified)
                ThemeSwitch.IsToggled = Application.Current.RequestedTheme == AppTheme.Dark;

                _isUpdatingSwitch = false;
            }

            if (_viewModel != null)
            {
                await _viewModel.CargarMetricasAsync();
            }
        }

        /// <summary>
        /// Cambia entre tema claro y oscuro.
        /// </summary>
        private void OnThemeSwitchToggled(object sender, ToggledEventArgs e)
        {
            // Si el cambio lo hizo la app en OnAppearing, ignoramos el evento
            if (_isUpdatingSwitch) return;

            if (Application.Current != null)
            {
                Application.Current.UserAppTheme = e.Value ? AppTheme.Dark : AppTheme.Light;
            }
        }

        /// <summary>
        /// Navega a la pestaña de gestión de alumnos.
        /// </summary>
        private async void OnNavegarEstudiantesClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//GestorEstudiantes");
        }

        /// <summary>
        /// Navega a la pestaña de asistencia.
        /// </summary>
        private async void OnNavegarHistorialClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//HistorialEstudiantes");
        }
    }
}

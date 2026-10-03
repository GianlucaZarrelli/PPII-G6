using System;
using Microsoft.Maui.Controls;

namespace ProyectoFinal_PPII_G6.Views
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();

            // Sincronizar el estado del switch con el tema activo
            if (Application.Current != null)
            {
                ThemeSwitch.IsToggled = Application.Current.UserAppTheme == AppTheme.Dark;
            }
        }

        private void OnThemeSwitchToggled(object sender, ToggledEventArgs e)
        {
            if (Application.Current != null)
            {
                Application.Current.UserAppTheme = e.Value ? AppTheme.Dark : AppTheme.Light;
            }
        }

        private async void OnNavegarEstudiantesClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//GestorEstudiantes");
        }

        private async void OnNavegarHistorialClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//HistorialEstudiantes");
        }
    }
}

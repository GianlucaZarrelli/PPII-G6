using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.ViewModels;

namespace ProyectoFinal_PPII_G6.Views
{
    /// <summary>
    /// Pantalla para registrar los contactos del tutor con un alumno y ver los anteriores.
    /// </summary>
    public partial class ContactosPage : ContentPage
    {
        private readonly ContactosViewModel _viewModel;

        /// <summary>
        /// Crea la página y le asigna su ViewModel.
        /// </summary>
        public ContactosPage(ContactosViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        /// <summary>
        /// Recarga las comisiones y los alumnos cada vez que se muestra la pantalla.
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.CargarComisionesAsync();
        }
    }
}
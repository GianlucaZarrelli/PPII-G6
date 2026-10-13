using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.Services;
using ProyectoFinal_PPII_G6.ViewModels;

namespace ProyectoFinal_PPII_G6.Views
{
    /// <summary>
    /// Panel de seguimiento: lista a los alumnos ordenados por riesgo. Al tocar uno, abre su ficha.
    /// </summary>
    public partial class PanelRiesgoPage : ContentPage
    {
        private readonly PanelRiesgoViewModel _viewModel;
        private readonly SeleccionAlumnoService _seleccion;

        public PanelRiesgoPage(PanelRiesgoViewModel viewModel, SeleccionAlumnoService seleccion)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _seleccion = seleccion;
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

        /// <summary>
        /// Abre la pestaña Historial con el alumno que se tocó.
        /// </summary>
        private async void OnAlumnoTapped(object? sender, TappedEventArgs e)
        {
            if (sender is BindableObject fila && fila.BindingContext is AlumnoRiesgoItem alumno)
            {
                _seleccion.Pedir(alumno.EstudianteId);
                await Shell.Current.GoToAsync("//HistorialEstudiantes");
            }
        }
    }
}
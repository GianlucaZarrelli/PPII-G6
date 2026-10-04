using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.ViewModels
{
    /// <summary>
    /// ViewModel de la pantalla de gestión de alumnos: alta, listado y baja de estudiantes.
    /// </summary>
    public class EstudianteViewModel : BindableObject
    {
        /// <summary>
        /// Servicio de acceso a datos.
        /// </summary>
        private readonly IDataService _dataService;

        private string _dni = string.Empty;

        /// <summary>
        /// DNI ingresado en el formulario.
        /// </summary>
        public string Dni
        {
            get => _dni;
            set { _dni = value; OnPropertyChanged(); }
        }

        private string _nombre = string.Empty;

        /// <summary>
        /// Nombre ingresado en el formulario.
        /// </summary>
        public string Nombre
        {
            get => _nombre;
            set { _nombre = value; OnPropertyChanged(); }
        }

        private string _apellido = string.Empty;

        /// <summary>
        /// Apellido ingresado en el formulario.
        /// </summary>
        public string Apellido
        {
            get => _apellido;
            set { _apellido = value; OnPropertyChanged(); }
        }

        private string _email = string.Empty;

        /// <summary>
        /// Email ingresado en el formulario.
        /// </summary>
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Lista de estudiantes que se muestra en pantalla.
        /// </summary>
        public ObservableCollection<Estudiante> Estudiantes { get; set; }

        /// <summary>
        /// Comando para guardar un estudiante nuevo.
        /// </summary>
        public Command GuardarEstudianteCommand { get; }

        /// <summary>
        /// Comando para recargar la lista de estudiantes.
        /// </summary>
        public Command CargarEstudianteCommand { get; }

        /// <summary>
        /// Comando para eliminar un estudiante.
        /// </summary>
        public Command<Estudiante> EliminarEstudianteCommand { get; }

        /// <summary>
        /// Crea el ViewModel, inicializa los comandos y carga la lista de estudiantes.
        /// </summary>
        public EstudianteViewModel(IDataService dataService)
        {
            _dataService = dataService;
            Estudiantes = new ObservableCollection<Estudiante>();

            GuardarEstudianteCommand = new Command(async () => await GuardarEstudianteAsync());
            CargarEstudianteCommand = new Command(async () => await CargarEstudianteAsync());
            EliminarEstudianteCommand = new Command<Estudiante>(async (est) => await EliminarEstudianteAsync(est));

            CargarEstudianteCommand.Execute(null);
        }

        /// <summary>
        /// Carga los estudiantes desde la base de datos.
        /// </summary>
        private async Task CargarEstudianteAsync()
        {
            var lista = await _dataService.GetEstudiantesAsync();
            Estudiantes.Clear();
            foreach (var est in lista)
            {
                Estudiantes.Add(est);
            }
        }

        /// <summary>
        /// Valida el formulario, guarda el estudiante, limpia los campos y recarga la lista.
        /// </summary>
        private async Task GuardarEstudianteAsync()
        {
            if (string.IsNullOrWhiteSpace(Dni) || string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(Apellido))
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Error", "DNI, Nombre y Apellido son obligatorios.", "OK");
                return;
            }

            var nuevo = new Estudiante(0, Dni, Nombre, Apellido, Email);
            await _dataService.SaveEstudianteAsync(nuevo);

            Dni = string.Empty;
            Nombre = string.Empty;
            Apellido = string.Empty;
            Email = string.Empty;

            await CargarEstudianteAsync();

            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Éxito", "Estudiante registrado en SQL Server.", "OK");
        }

        /// <summary>
        /// Pide confirmación y elimina el estudiante.
        /// </summary>
        private async Task EliminarEstudianteAsync(Estudiante estudiante)
        {
            if (estudiante == null || Application.Current?.MainPage == null) return;

            bool confirmar = await Application.Current.MainPage.DisplayAlert("Confirmar", $"¿Desea eliminar a {estudiante.NombreCompleto}?", "Sí", "No");
            if (confirmar)
            {
                await _dataService.DeleteEstudianteAsync(estudiante.Id);
                await CargarEstudianteAsync();
            }
        }
    }
}

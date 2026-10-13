using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.ViewModels
{
    /// <summary>
    /// ViewModel de la pantalla de gestión de alumnos: se elige una comisión, se ve su nómina y se cargan alumnos en ella.
    /// </summary>
    public class EstudianteViewModel : BindableObject
    {
        /// <summary>
        /// Servicio de acceso a datos.
        /// </summary>
        private readonly IDataService _dataService;

        /// <summary>
        /// Mientras es true, el setter de la comisión no dispara cargas (se usa al repoblar el selector).
        /// </summary>
        private bool _actualizando;

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
        /// Comisiones disponibles para elegir.
        /// </summary>
        public ObservableCollection<Comision> Comisiones { get; } = new();

        /// <summary>
        /// Alumnos de la comisión elegida.
        /// </summary>
        public ObservableCollection<Estudiante> Estudiantes { get; } = new();

        private Comision? _comisionSeleccionada;

        /// <summary>
        /// Comisión elegida: es la que se muestra en la nómina y la que recibe al alumno nuevo.
        /// </summary>
        public Comision? ComisionSeleccionada
        {
            get => _comisionSeleccionada;
            set
            {
                _comisionSeleccionada = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TituloAlta));
                OnPropertyChanged(nameof(TituloNomina));

                if (_actualizando) return;
                _ = CargarAlumnosAsync();
            }
        }

        /// <summary>
        /// Título del formulario de alta, con la comisión elegida.
        /// </summary>
        public string TituloAlta => ComisionSeleccionada == null
            ? "Alta de estudiante"
            : $"Alta de estudiante en la comisión {ComisionSeleccionada.Nombre}";

        /// <summary>
        /// Título de la nómina, con la comisión elegida.
        /// </summary>
        public string TituloNomina => ComisionSeleccionada == null
            ? "Nómina"
            : $"Nómina de la comisión {ComisionSeleccionada.Nombre}";

        private string _estadoLista = "Elegí una comisión para ver su nómina.";

        /// <summary>
        /// Texto que indica qué se está mostrando (o por qué la lista está vacía).
        /// </summary>
        public string EstadoLista
        {
            get => _estadoLista;
            set { _estadoLista = value; OnPropertyChanged(); }
        }

        private string _avisoComisiones = string.Empty;

        /// <summary>
        /// Mensaje que se muestra cuando todavía no hay comisiones creadas.
        /// </summary>
        public string AvisoComisiones
        {
            get => _avisoComisiones;
            set { _avisoComisiones = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Comando para guardar un estudiante nuevo.
        /// </summary>
        public Command GuardarEstudianteCommand { get; }

        /// <summary>
        /// Comando para recargar las comisiones y la nómina.
        /// </summary>
        public Command CargarEstudianteCommand { get; }

        /// <summary>
        /// Crea el ViewModel, inicializa los comandos y carga las comisiones.
        /// </summary>
        public EstudianteViewModel(IDataService dataService)
        {
            _dataService = dataService;

            GuardarEstudianteCommand = new Command(async () => await GuardarEstudianteAsync());
            CargarEstudianteCommand = new Command(async () => await CargarEstudianteAsync());

            CargarEstudianteCommand.Execute(null);
        }

        /// <summary>
        /// Quita puntos y espacios para que "30.123.456" y "30123456" cuenten como el mismo DNI.
        /// </summary>
        private static string NormalizarDni(string? dni) =>
            (dni ?? string.Empty).Replace(".", string.Empty).Replace(" ", string.Empty).Trim();

        private static async Task AvisarAsync(string titulo, string mensaje)
        {
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert(titulo, mensaje, "OK");
        }

        /// <summary>
        /// Recarga las comisiones, conserva la elegida (si no había ninguna, no se muestra nada) y recarga la nómina.
        /// </summary>
        private async Task CargarEstudianteAsync()
        {
            try
            {
                var idPrevio = ComisionSeleccionada?.Id;
                var lista = (await _dataService.GetComisionesAsync())
                    .OrderBy(c => c.Nombre)
                    .ToList();

                _actualizando = true;
                try
                {
                    Comisiones.Clear();
                    foreach (var c in lista)
                    {
                        Comisiones.Add(c);
                    }

                    ComisionSeleccionada = Comisiones.FirstOrDefault(c => c.Id == idPrevio);
                }
                finally
                {
                    _actualizando = false;
                }

                AvisoComisiones = lista.Count == 0
                    ? "No hay comisiones cargadas. Ejecutá scripts/03_comisiones.sql en la base."
                    : string.Empty;

                await CargarAlumnosAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                AvisoComisiones = "No se pudieron cargar las comisiones.";
            }
        }

        /// <summary>
        /// Carga los alumnos de la comisión elegida.
        /// </summary>
        private async Task CargarAlumnosAsync()
        {
            var comision = ComisionSeleccionada;
            Estudiantes.Clear();

            if (comision == null)
            {
                EstadoLista = "Elegí una comisión para ver su nómina.";
                return;
            }

            try
            {
                var lista = await _dataService.GetEstudiantesByComisionIdAsync(comision.Id);

                // Si mientras consultaba cambió la comisión, este resultado ya no sirve
                if (comision != ComisionSeleccionada) return;

                Estudiantes.Clear();
                foreach (var est in lista.OrderBy(e => e.Apellido).ThenBy(e => e.Nombre))
                {
                    Estudiantes.Add(est);
                }

                EstadoLista = Estudiantes.Count == 0
                    ? "Esta comisión todavía no tiene alumnos."
                    : $"{Estudiantes.Count} {(Estudiantes.Count == 1 ? "alumno" : "alumnos")}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                EstadoLista = "No se pudo cargar la nómina.";
            }
        }

        /// <summary>
        /// Valida el formulario, guarda el estudiante en la comisión elegida, limpia los campos y recarga la nómina.
        /// </summary>
        private async Task GuardarEstudianteAsync()
        {
            var dni = NormalizarDni(Dni);
            var nombre = (Nombre ?? string.Empty).Trim();
            var apellido = (Apellido ?? string.Empty).Trim();
            var email = (Email ?? string.Empty).Trim();
            var comision = ComisionSeleccionada;

            if (dni.Length == 0 || nombre.Length == 0 || apellido.Length == 0 || comision == null)
            {
                await AvisarAsync("Error", "DNI, Nombre, Apellido y Comisión son obligatorios.");
                return;
            }

            // La lista de la pantalla es la de la comisión elegida: sirve para avisar antes de ir a la base
            if (Estudiantes.Any(e => NormalizarDni(e.Dni) == dni))
            {
                await AvisarAsync("Error", $"Ya existe un estudiante con ese DNI en la comisión {comision.Nombre}.");
                return;
            }

            try
            {
                var nuevo = new Estudiante(0, dni, nombre, apellido, email, comision.Id);
                await _dataService.SaveEstudianteAsync(nuevo);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                await AvisarAsync("Error", "No se pudo guardar el estudiante. Revisá que el DNI no esté repetido en la comisión.");
                return;
            }

            // La comisión queda elegida para cargar varios alumnos seguidos en la misma
            Dni = string.Empty;
            Nombre = string.Empty;
            Apellido = string.Empty;
            Email = string.Empty;

            await CargarAlumnosAsync();

            await AvisarAsync("Éxito", $"Estudiante registrado en la comisión {comision.Nombre}.");
        }
    }
}
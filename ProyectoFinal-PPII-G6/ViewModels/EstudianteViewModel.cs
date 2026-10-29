using System;
using System.Collections.Generic;
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
        /// Lo que se escribió en el formulario de alta, ya limpio (sin espacios de más y con el DNI normalizado).
        /// </summary>
        private sealed record DatosAlta(string Dni, string Nombre, string Apellido, string Email, Comision? Comision);

        private readonly IDataService _dataService;
        private readonly IDialogoService _dialogo;

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
        public EstudianteViewModel(IDataService dataService, IDialogoService dialogo)
        {
            _dataService = dataService;
            _dialogo = dialogo;

            GuardarEstudianteCommand = new Command(async () => await GuardarEstudianteAsync());
            CargarEstudianteCommand = new Command(async () => await CargarEstudianteAsync());

            CargarEstudianteCommand.Execute(null);
        }

        /// <summary>
        /// Recarga las comisiones, conserva la elegida (si no había ninguna, no se muestra nada) y recarga la nómina.
        /// </summary>
        private async Task CargarEstudianteAsync()
        {
            try
            {
                var idPrevio = ComisionSeleccionada?.Id;
                var lista = (await _dataService.GetComisionesAsync()).OrderBy(c => c.Nombre).ToList();

                RepoblarComisiones(lista, idPrevio);
                AvisoComisiones = lista.Count == 0
                    ? "No hay comisiones cargadas. Ejecutá scripts/comisiones.sql en la base."
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
        /// Vuelve a llenar el selector sin disparar la carga de alumnos, y deja elegida la comisión indicada si sigue estando.
        /// </summary>
        private void RepoblarComisiones(List<Comision> lista, int? idPrevio)
        {
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
        }

        /// <summary>
        /// Carga los alumnos de la comisión elegida, o avisa que hay que elegir una.
        /// </summary>
        private async Task CargarAlumnosAsync()
        {
            var comision = ComisionSeleccionada;
            Estudiantes.Clear();

            if (comision == null)
                EstadoLista = "Elegí una comisión para ver su nómina.";
            else
                await TraerAlumnosAsync(comision);
        }

        /// <summary>
        /// Pide los alumnos de la comisión a la base y los muestra.
        /// Si mientras consultaba cambió la comisión elegida, descarta el resultado porque ya no sirve.
        /// </summary>
        private async Task TraerAlumnosAsync(Comision comision)
        {
            try
            {
                var lista = await _dataService.GetEstudiantesByComisionIdAsync(comision.Id);
                if (comision != ComisionSeleccionada) return;

                MostrarAlumnos(lista);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                EstadoLista = "No se pudo cargar la nómina.";
            }
        }

        /// <summary>
        /// Reemplaza la nómina por los alumnos recibidos, ordenados por apellido, y actualiza el texto de estado.
        /// </summary>
        private void MostrarAlumnos(IEnumerable<Estudiante> lista)
        {
            Estudiantes.Clear();
            foreach (var est in lista.OrderBy(e => e.Apellido).ThenBy(e => e.Nombre))
            {
                Estudiantes.Add(est);
            }

            EstadoLista = Estudiantes.Count == 0
                ? "Esta comisión todavía no tiene alumnos."
                : $"{Estudiantes.Count} {(Estudiantes.Count == 1 ? "alumno" : "alumnos")}";
        }

        /// <summary>
        /// Valida el formulario, guarda el estudiante en la comisión elegida, limpia los campos y recarga la nómina.
        /// </summary>
        private async Task GuardarEstudianteAsync()
        {
            var datos = LeerFormulario();
            var error = ObtenerErrorDelFormulario(datos);

            if (error != null)
            {
                await _dialogo.AvisarAsync("Error", error);
                return;
            }

            if (!await IntentarGuardarAsync(datos)) return;

            LimpiarFormulario();
            await CargarAlumnosAsync();
            await _dialogo.AvisarAsync("Éxito", $"Estudiante registrado en la comisión {datos.Comision!.Nombre}.");
        }

        /// <summary>
        /// Toma lo escrito en el formulario y lo deja limpio: sin espacios de más y con el DNI normalizado.
        /// </summary>
        private DatosAlta LeerFormulario() => new(
            ValidacionDni.Normalizar(Dni),
            (Nombre ?? string.Empty).Trim(),
            (Apellido ?? string.Empty).Trim(),
            (Email ?? string.Empty).Trim(),
            ComisionSeleccionada);

        /// <summary>
        /// Devuelve el primer problema que tenga el formulario, con un mensaje propio para cada uno, o null si está bien.
        /// El orden importa: primero lo que falta, después el formato y al final si el alumno ya existe.
        /// </summary>
        private string? ObtenerErrorDelFormulario(DatosAlta datos) =>
            ErrorDeObligatorios(datos)
            ?? ValidacionDni.ObtenerError(datos.Dni)
            ?? ErrorDeLargo(datos)
            ?? ErrorDeEmail(datos)
            ?? ErrorDeDuplicado(datos);

        /// <summary>
        /// Avisa si falta la comisión o alguno de los datos obligatorios.
        /// </summary>
        private static string? ErrorDeObligatorios(DatosAlta datos)
        {
            if (datos.Comision == null)
                return "Elegí una comisión antes de guardar.";

            return datos.Dni.Length == 0 || datos.Nombre.Length == 0 || datos.Apellido.Length == 0 || datos.Email.Length == 0
                ? "DNI, nombre, apellido y email son obligatorios."
                : null;
        }

        /// <summary>
        /// Avisa si el nombre o el apellido no entran en la base.
        /// </summary>
        private static string? ErrorDeLargo(DatosAlta datos) =>
            datos.Nombre.Length > LimitesAlumno.LargoNombre || datos.Apellido.Length > LimitesAlumno.LargoApellido
                ? $"El nombre y el apellido admiten hasta {LimitesAlumno.LargoNombre} caracteres."
                : null;

        /// <summary>
        /// Avisa si el email no tiene formato de dirección.
        /// </summary>
        private static string? ErrorDeEmail(DatosAlta datos) =>
            ValidacionEmail.EsValido(datos.Email)
                ? null
                : "El email no es válido. Tiene que tener el formato nombre@dominio.com.";

        /// <summary>
        /// Avisa si el DNI ya está en la comisión. La lista de la pantalla es la de la comisión elegida,
        /// así que sirve para avisar antes de ir a la base.
        /// </summary>
        private string? ErrorDeDuplicado(DatosAlta datos) =>
            Estudiantes.Any(e => ValidacionDni.Normalizar(e.Dni) == datos.Dni)
                ? $"Ya existe un estudiante con ese DNI en la comisión {datos.Comision?.Nombre}."
                : null;

        /// <summary>
        /// Guarda el alumno. Si la base falla, avisa y devuelve false.
        /// </summary>
        private async Task<bool> IntentarGuardarAsync(DatosAlta datos)
        {
            try
            {
                var nuevo = new Estudiante(0, datos.Dni, datos.Nombre, datos.Apellido, datos.Email, datos.Comision!.Id);
                await _dataService.SaveEstudianteAsync(nuevo);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                await _dialogo.AvisarAsync("Error", "No se pudo guardar el estudiante. Revisá la conexión con la base de datos.");
                return false;
            }
        }

        /// <summary>
        /// Vacía los campos del formulario. La comisión queda elegida para cargar varios alumnos seguidos en la misma.
        /// </summary>
        private void LimpiarFormulario()
        {
            Dni = string.Empty;
            Nombre = string.Empty;
            Apellido = string.Empty;
            Email = string.Empty;
        }
    }
}
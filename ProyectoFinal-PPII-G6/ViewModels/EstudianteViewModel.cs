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
            set
            {
                _dni = value;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(ErrorDni)) ErrorDni = string.Empty;
            }
        }

        private string _nombre = string.Empty;

        /// <summary>
        /// Nombre ingresado en el formulario.
        /// </summary>
        public string Nombre
        {
            get => _nombre;
            set
            {
                _nombre = value;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(ErrorNombre)) ErrorNombre = string.Empty;
            }
        }

        private string _apellido = string.Empty;

        /// <summary>
        /// Apellido ingresado en el formulario.
        /// </summary>
        public string Apellido
        {
            get => _apellido;
            set
            {
                _apellido = value;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(ErrorApellido)) ErrorApellido = string.Empty;
            }
        }

        private string _email = string.Empty;

        /// <summary>
        /// Email ingresado en el formulario.
        /// </summary>
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(ErrorEmail)) ErrorEmail = string.Empty;
            }
        }

        private string _errorComision = string.Empty;
        private string _errorDni = string.Empty;
        private string _errorNombre = string.Empty;
        private string _errorApellido = string.Empty;
        private string _errorEmail = string.Empty;
        private string _errorNombreComision = string.Empty;

        /// <summary>Texto rojo debajo del selector de comisión.</summary>
        public string ErrorComision
        {
            get => _errorComision;
            set { _errorComision = value; OnPropertyChanged(); }
        }

        /// <summary>Texto rojo debajo del DNI.</summary>
        public string ErrorDni
        {
            get => _errorDni;
            set { _errorDni = value; OnPropertyChanged(); }
        }

        /// <summary>Texto rojo debajo del nombre.</summary>
        public string ErrorNombre
        {
            get => _errorNombre;
            set { _errorNombre = value; OnPropertyChanged(); }
        }

        /// <summary>Texto rojo debajo del apellido.</summary>
        public string ErrorApellido
        {
            get => _errorApellido;
            set { _errorApellido = value; OnPropertyChanged(); }
        }

        /// <summary>Texto rojo debajo del email.</summary>
        public string ErrorEmail
        {
            get => _errorEmail;
            set { _errorEmail = value; OnPropertyChanged(); }
        }

        /// <summary>Texto rojo debajo del nombre de la comisión nueva.</summary>
        public string ErrorNombreComision
        {
            get => _errorNombreComision;
            set { _errorNombreComision = value; OnPropertyChanged(); }
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
                if (!string.IsNullOrEmpty(ErrorComision)) ErrorComision = string.Empty;

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

        private string _nombreComision = string.Empty;

        /// <summary>
        /// Nombre escrito en el campo de la comisión nueva.
        /// </summary>
        public string NombreComision
        {
            get => _nombreComision;
            set
            {
                _nombreComision = value ?? string.Empty;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(ErrorNombreComision)) ErrorNombreComision = string.Empty;
            }
        }

        /// <summary>
        /// Largo máximo del nombre de una comisión, para limitar el cuadro de texto.
        /// </summary>
        public int LargoMaximoNombreComision => ValidacionComision.LargoMaximoNombre;

        /// <summary>
        /// Comando para guardar un estudiante nuevo.
        /// </summary>
        public Command GuardarEstudianteCommand { get; }

        /// <summary>
        /// Comando para crear una comisión nueva.
        /// </summary>
        public Command CrearComisionCommand { get; }

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
            CrearComisionCommand = new Command(async () => await CrearComisionAsync());

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
                    ? "Todavía no hay comisiones. Creá la primera con el campo de abajo."
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
        /// Valida el nombre escrito y, si está bien, crea la comisión. Es pública para poder probarla sin la pantalla.
        /// </summary>
        public async Task CrearComisionAsync()
        {
            var nombre = (NombreComision ?? string.Empty).Trim();
            var error = ValidacionComision.ObtenerError(nombre, Comisiones.Select(c => c.Nombre));

            if (error != null)
                ErrorNombreComision = error;
            else
                await GuardarComisionAsync(nombre);
        }

        /// <summary>
        /// Crea la comisión en la base, la deja elegida y avisa. Si la base falla, avisa y conserva lo escrito.
        /// </summary>
        private async Task GuardarComisionAsync(string nombre)
        {
            try
            {
                var comision = await _dataService.GetOrCreateComisionAsync(nombre);

                NombreComision = string.Empty;
                await MostrarComisionNuevaAsync(comision);
                await _dialogo.AvisarAsync("Éxito", $"Comisión {comision.Nombre} creada.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                await _dialogo.AvisarAsync("Error", "No se pudo crear la comisión. Revisá la conexión con la base de datos.");
            }
        }

        /// <summary>
        /// Vuelve a cargar el selector con la comisión nueva incluida, la deja elegida y muestra su nómina (vacía).
        /// </summary>
        private async Task MostrarComisionNuevaAsync(Comision nueva)
        {
            var lista = (await _dataService.GetComisionesAsync()).OrderBy(c => c.Nombre).ToList();

            RepoblarComisiones(lista, nueva.Id);
            AvisoComisiones = string.Empty;
            await CargarAlumnosAsync();
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
        /// Los errores de validación se muestran en rojo debajo de cada campo (sin ventana emergente).
        /// </summary>
        private async Task GuardarEstudianteAsync()
        {
            var datos = LeerFormulario();
            if (!AplicarErroresDelFormulario(datos))
                return;

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
        /// Completa los textos rojos de cada campo. Devuelve true si el formulario está bien.
        /// Muestra todos los problemas a la vez (no solo el primero).
        /// </summary>
        private bool AplicarErroresDelFormulario(DatosAlta datos)
        {
            LimpiarErroresAlta();

            if (datos.Comision == null)
                ErrorComision = "Elegí una comisión antes de guardar.";

            ErrorDni = ValidacionDni.ObtenerError(datos.Dni) ?? string.Empty;

            if (datos.Nombre.Length == 0)
                ErrorNombre = "Ingresá un nombre.";
            else if (datos.Nombre.Length > LimitesAlumno.LargoNombre)
                ErrorNombre = $"El nombre admite hasta {LimitesAlumno.LargoNombre} caracteres.";

            if (datos.Apellido.Length == 0)
                ErrorApellido = "Ingresá un apellido.";
            else if (datos.Apellido.Length > LimitesAlumno.LargoApellido)
                ErrorApellido = $"El apellido admite hasta {LimitesAlumno.LargoApellido} caracteres.";

            if (datos.Email.Length == 0)
                ErrorEmail = "Ingresá un email.";
            else if (!ValidacionEmail.EsValido(datos.Email))
                ErrorEmail = "El email no es válido. Tiene que tener el formato nombre@dominio.com.";

            if (string.IsNullOrEmpty(ErrorDni)
                && datos.Comision != null
                && Estudiantes.Any(e => ValidacionDni.Normalizar(e.Dni) == datos.Dni))
            {
                ErrorDni = $"Ya existe un estudiante con ese DNI en la comisión {datos.Comision.Nombre}.";
            }

            return string.IsNullOrEmpty(ErrorComision)
                && string.IsNullOrEmpty(ErrorDni)
                && string.IsNullOrEmpty(ErrorNombre)
                && string.IsNullOrEmpty(ErrorApellido)
                && string.IsNullOrEmpty(ErrorEmail);
        }

        /// <summary>
        /// Borra los textos rojos del alta de alumno.
        /// </summary>
        private void LimpiarErroresAlta()
        {
            ErrorComision = string.Empty;
            ErrorDni = string.Empty;
            ErrorNombre = string.Empty;
            ErrorApellido = string.Empty;
            ErrorEmail = string.Empty;
        }

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
            LimpiarErroresAlta();
        }
    }
}
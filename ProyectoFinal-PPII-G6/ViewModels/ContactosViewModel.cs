using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Presentacion;
using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.ViewModels
{
    /// <summary>
    /// Una opción del selector de medio: el valor que se guarda y el texto que se muestra.
    /// </summary>
    public class OpcionMedio
    {
        /// <summary>
        /// Medio de contacto que se guarda.
        /// </summary>
        public MedioContacto Valor { get; }

        /// <summary>
        /// Texto con el que se muestra el medio.
        /// </summary>
        public string Texto { get; }

        /// <summary>
        /// Crea la opción a partir del medio; el texto sale de PresentacionMedio.
        /// </summary>
        public OpcionMedio(MedioContacto valor)
        {
            Valor = valor;
            Texto = PresentacionMedio.Texto(valor);
        }
    }

    /// <summary>
    /// Una fila del historial de contactos de un alumno.
    /// </summary>
    public class ContactoFila
    {
        /// <summary>
        /// Fecha y hora del contacto, en texto.
        /// </summary>
        public string FechaHora { get; set; } = string.Empty;

        /// <summary>
        /// Medio por el que se hizo el contacto.
        /// </summary>
        public string Medio { get; set; } = string.Empty;

        /// <summary>
        /// Resultado u observaciones del contacto.
        /// </summary>
        public string Resultado { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel de la pantalla de contactos: se elige un alumno, se registra el contacto (fecha, hora, medio
    /// y resultado) y se ve el historial de contactos anteriores con ese alumno.
    /// </summary>
    public class ContactosViewModel : BindableObject
    {
        private readonly IDataService _dataService;
        private readonly IDialogoService _dialogo;

        /// <summary>
        /// Evita que un doble toque en "Registrar" guarde dos veces.
        /// </summary>
        private bool _guardando;

        /// <summary>
        /// Comisiones disponibles para elegir.
        /// </summary>
        public ObservableCollection<Comision> Comisiones { get; } = new();

        /// <summary>
        /// Alumnos de la comisión elegida.
        /// </summary>
        public ObservableCollection<Estudiante> ListaEstudiantes { get; } = new();

        /// <summary>
        /// Medios de contacto entre los que se puede elegir.
        /// </summary>
        public List<OpcionMedio> Medios { get; } = new();

        /// <summary>
        /// Contactos anteriores con el alumno elegido, del más reciente al más antiguo.
        /// </summary>
        public ObservableCollection<ContactoFila> Historial { get; } = new();

        /// <summary>
        /// Largo máximo del resultado, para limitar el cuadro de texto.
        /// </summary>
        public int LargoMaximoResultado => ValidacionContacto.LargoMaximoResultado;

        private Comision? _comisionSeleccionada;

        /// <summary>
        /// Comisión elegida; al cambiarla se cargan sus alumnos.
        /// </summary>
        public Comision? ComisionSeleccionada
        {
            get => _comisionSeleccionada;
            set
            {
                _comisionSeleccionada = value;
                OnPropertyChanged();
                _ = CargarEstudiantesAsync();
            }
        }

        private Estudiante? _estudianteSeleccionado;

        /// <summary>
        /// Alumno elegido; al cambiarlo se carga su historial de contactos.
        /// </summary>
        public Estudiante? EstudianteSeleccionado
        {
            get => _estudianteSeleccionado;
            set
            {
                _estudianteSeleccionado = value;
                OnPropertyChanged();
                _ = CargarHistorialAsync();
            }
        }

        private DateTime _fecha;

        /// <summary>
        /// Día en que se hizo el contacto.
        /// </summary>
        public DateTime Fecha
        {
            get => _fecha;
            set { _fecha = value; OnPropertyChanged(); }
        }

        private TimeSpan _hora;

        /// <summary>
        /// Hora a la que se hizo el contacto.
        /// </summary>
        public TimeSpan Hora
        {
            get => _hora;
            set { _hora = value; OnPropertyChanged(); }
        }

        private OpcionMedio? _medioSeleccionado;

        /// <summary>
        /// Medio elegido para el contacto.
        /// </summary>
        public OpcionMedio? MedioSeleccionado
        {
            get => _medioSeleccionado;
            set { _medioSeleccionado = value; OnPropertyChanged(); }
        }

        private string _resultado = string.Empty;

        /// <summary>
        /// Resultado u observaciones del contacto.
        /// </summary>
        public string Resultado
        {
            get => _resultado;
            set
            {
                _resultado = value ?? string.Empty;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ContadorResultado));
            }
        }

        /// <summary>
        /// Cuántos caracteres lleva escritos el resultado, sobre el máximo permitido.
        /// </summary>
        public string ContadorResultado => $"{Resultado.Length} / {ValidacionContacto.LargoMaximoResultado}";

        private string _estado = "Seleccione una comisión.";

        /// <summary>
        /// Texto que indica qué se está mostrando, o por qué la lista está vacía.
        /// </summary>
        public string Estado
        {
            get => _estado;
            set { _estado = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Comando para registrar el contacto.
        /// </summary>
        public Command GuardarCommand { get; }

        /// <summary>
        /// Crea el ViewModel con el acceso a datos y el servicio de avisos, y deja el formulario en este momento.
        /// </summary>
        public ContactosViewModel(IDataService dataService, IDialogoService dialogo)
        {
            _dataService = dataService;
            _dialogo = dialogo;

            foreach (var medio in Enum.GetValues<MedioContacto>())
            {
                Medios.Add(new OpcionMedio(medio));
            }

            PonerFechaYHoraActuales();
            GuardarCommand = new Command(async () => await GuardarAsync());
        }

        /// <summary>
        /// Recarga las comisiones. Si no cambiaron, solo refresca los alumnos y conserva lo elegido
        /// (para no perder el alumno al volver a la pestaña).
        /// </summary>
        public async Task CargarComisionesAsync()
        {
            try
            {
                var lista = await _dataService.GetComisionesAsync();

                if (MismasComisiones(lista))
                    await CargarEstudiantesAsync();
                else
                    RepoblarComisiones(lista);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                Estado = "No se pudieron cargar las comisiones.";
            }
        }

        /// <summary>
        /// Indica si la lista recibida tiene las mismas comisiones que ya están en el selector.
        /// </summary>
        private bool MismasComisiones(IEnumerable<Comision> lista) =>
            Comisiones.Select(c => c.Id).OrderBy(id => id)
                .SequenceEqual(lista.Select(c => c.Id).OrderBy(id => id));

        /// <summary>
        /// Reemplaza el selector de comisiones por la lista recibida y deja elegida la anterior (o la primera).
        /// Al elegir una comisión, el setter carga sus alumnos.
        /// </summary>
        private void RepoblarComisiones(IEnumerable<Comision> lista)
        {
            var idPrevio = ComisionSeleccionada?.Id;
            Comisiones.Clear();

            foreach (var c in lista.OrderBy(c => c.Nombre))
            {
                Comisiones.Add(c);
            }

            ComisionSeleccionada = Comisiones.FirstOrDefault(c => c.Id == idPrevio)
                                   ?? Comisiones.FirstOrDefault();
        }

        /// <summary>
        /// Vacía la lista de alumnos y carga los de la comisión elegida. Si el alumno que estaba elegido
        /// sigue en la lista, lo conserva.
        /// </summary>
        private async Task CargarEstudiantesAsync()
        {
            var comision = ComisionSeleccionada;
            var idPrevio = EstudianteSeleccionado?.Id;

            ListaEstudiantes.Clear();
            EstudianteSeleccionado = null;

            if (comision != null)
                await TraerEstudiantesAsync(comision, idPrevio);
        }

        /// <summary>
        /// Pide los alumnos de la comisión a la base y los muestra.
        /// Si mientras consultaba cambió la comisión, descarta el resultado porque ya no sirve.
        /// </summary>
        private async Task TraerEstudiantesAsync(Comision comision, int? idPrevio)
        {
            try
            {
                var lista = await _dataService.GetEstudiantesByComisionIdAsync(comision.Id);
                if (comision != ComisionSeleccionada) return;

                MostrarEstudiantes(lista, idPrevio);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                Estado = "No se pudo cargar la lista de alumnos.";
            }
        }

        /// <summary>
        /// Llena la lista de alumnos por apellido y deja elegido el indicado, si está. Si no hay alumnos, lo avisa.
        /// </summary>
        private void MostrarEstudiantes(IEnumerable<Estudiante> lista, int? idPrevio)
        {
            foreach (var est in lista.OrderBy(e => e.Apellido).ThenBy(e => e.Nombre))
            {
                ListaEstudiantes.Add(est);
            }

            EstudianteSeleccionado = ListaEstudiantes.FirstOrDefault(e => e.Id == idPrevio);

            if (ListaEstudiantes.Count == 0)
                Estado = "La comisión no tiene alumnos cargados.";
        }

        /// <summary>
        /// Carga los contactos anteriores del alumno elegido, o explica qué falta elegir.
        /// </summary>
        private async Task CargarHistorialAsync()
        {
            var alumno = EstudianteSeleccionado;
            Historial.Clear();

            if (alumno == null)
                Estado = "Elegí un alumno para registrar y ver sus contactos.";
            else
                await TraerHistorialAsync(alumno);
        }

        /// <summary>
        /// Pide los contactos del alumno a la base y los muestra.
        /// Si mientras consultaba el usuario eligió otro alumno, descarta el resultado porque ya no sirve.
        /// </summary>
        private async Task TraerHistorialAsync(Estudiante alumno)
        {
            try
            {
                var contactos = await _dataService.GetContactosPorEstudianteAsync(alumno.Id);
                if (alumno != EstudianteSeleccionado) return;

                MostrarHistorial(alumno, contactos);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                Estado = "No se pudo cargar el historial de contactos.";
            }
        }

        /// <summary>
        /// Llena el historial y resume cuántos contactos tiene el alumno.
        /// </summary>
        private void MostrarHistorial(Estudiante alumno, List<Contacto> contactos)
        {
            Historial.Clear();

            foreach (var contacto in contactos)
            {
                Historial.Add(CrearFila(contacto));
            }

            Estado = contactos.Count == 0
                ? $"{alumno.NombreCompleto} todavía no tiene contactos registrados."
                : $"Contactos con {alumno.NombreCompleto}: {contactos.Count}.";
        }

        /// <summary>
        /// Arma la fila del historial de un contacto.
        /// </summary>
        private static ContactoFila CrearFila(Contacto contacto) => new()
        {
            FechaHora = contacto.FechaHora.ToString("dd/MM/yyyy HH:mm"),
            Medio = PresentacionMedio.Texto(contacto.Medio),
            Resultado = contacto.Resultado
        };

        /// <summary>
        /// Valida el contacto y, si está bien, lo guarda. Si ya se está guardando, no hace nada.
        /// Es pública para poder probarla sin la pantalla.
        /// </summary>
        public async Task GuardarAsync()
        {
            if (_guardando) return;

            var error = ObtenerError();

            if (error != null)
                await _dialogo.AvisarAsync("Atención", error);
            else
                await GuardarConBloqueoAsync(EstudianteSeleccionado!);
        }

        /// <summary>
        /// Fecha y hora del contacto, juntas.
        /// </summary>
        private DateTime FechaHora => Fecha.Date + Hora;

        /// <summary>
        /// Devuelve el primer problema del formulario (alumno, fecha y hora, medio o resultado), o null si está bien.
        /// </summary>
        private string? ObtenerError() =>
            EstudianteSeleccionado == null
                ? "Elegí un alumno antes de registrar el contacto."
                : ValidacionContacto.ObtenerError(FechaHora, MedioSeleccionado?.Valor, Resultado, DateTime.Now);

        /// <summary>
        /// Marca que hay un guardado en curso mientras trabaja, y lo desmarca al terminar, salga como salga.
        /// </summary>
        private async Task GuardarConBloqueoAsync(Estudiante alumno)
        {
            _guardando = true;
            try
            {
                await GuardarYAvisarAsync(alumno);
            }
            finally
            {
                _guardando = false;
            }
        }

        /// <summary>
        /// Guarda el contacto y, si salió bien, limpia el formulario, actualiza el historial y avisa.
        /// </summary>
        private async Task GuardarYAvisarAsync(Estudiante alumno)
        {
            if (!await IntentarGuardarAsync(alumno)) return;

            LimpiarFormulario();
            await CargarHistorialAsync();
            await _dialogo.AvisarAsync("Éxito", "Contacto registrado.");
        }

        /// <summary>
        /// Guarda el contacto. Si la base falla, avisa y devuelve false.
        /// </summary>
        private async Task<bool> IntentarGuardarAsync(Estudiante alumno)
        {
            try
            {
                var contacto = new Contacto(0, alumno.Id, FechaHora, MedioSeleccionado!.Valor, Resultado.Trim());
                await _dataService.SaveContactoAsync(contacto);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                await _dialogo.AvisarAsync("Error", "No se pudo registrar el contacto.");
                return false;
            }
        }

        /// <summary>
        /// Deja el formulario listo para el próximo contacto: sin medio ni resultado, y con este momento.
        /// </summary>
        private void LimpiarFormulario()
        {
            Resultado = string.Empty;
            MedioSeleccionado = null;
            PonerFechaYHoraActuales();
        }

        /// <summary>
        /// Pone la fecha de hoy y la hora actual (sin segundos): lo más común es registrar un contacto recién hecho.
        /// </summary>
        private void PonerFechaYHoraActuales()
        {
            var ahora = DateTime.Now;
            Fecha = ahora.Date;
            Hora = new TimeSpan(ahora.Hour, ahora.Minute, 0);
        }
    }
}
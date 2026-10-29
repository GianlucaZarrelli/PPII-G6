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
    /// Una fila de la lista de entregas: un alumno y si entregó el trabajo elegido.
    /// </summary>
    public class EntregaFila : BindableObject
    {
        /// <summary>
        /// Id del alumno.
        /// </summary>
        public int EstudianteId { get; set; }

        /// <summary>
        /// Apellido y nombre del alumno, para mostrar en la lista.
        /// </summary>
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Id de la entrega ya guardada, o 0 si todavía no hay registro.
        /// </summary>
        public int EntregaId { get; set; }

        private bool _entregado;

        /// <summary>
        /// Indica si el alumno entregó el trabajo.
        /// </summary>
        public bool Entregado
        {
            get => _entregado;
            set { _entregado = value; OnPropertyChanged(); }
        }
    }

    /// <summary>
    /// ViewModel de la pantalla de entregas: se elige una comisión y un trabajo práctico (o se crea uno)
    /// y se marca qué alumnos lo entregaron.
    /// </summary>
    public class EntregasViewModel : BindableObject
    {
        private readonly IDataService _dataService;
        private readonly IDialogoService _dialogo;

        /// <summary>
        /// Evita que un doble toque en "Guardar" guarde dos veces.
        /// </summary>
        private bool _guardando;

        /// <summary>
        /// Comisiones disponibles para elegir.
        /// </summary>
        public ObservableCollection<Comision> Comisiones { get; } = new();

        /// <summary>
        /// Trabajos prácticos de la comisión elegida, por fecha de entrega.
        /// </summary>
        public ObservableCollection<TrabajoPractico> Trabajos { get; } = new();

        /// <summary>
        /// Alumnos de la comisión con su estado de entrega del trabajo elegido.
        /// </summary>
        public ObservableCollection<EntregaFila> Filas { get; } = new();

        private Comision? _comisionSeleccionada;

        /// <summary>
        /// Comisión elegida; al cambiarla se cargan sus trabajos.
        /// </summary>
        public Comision? ComisionSeleccionada
        {
            get => _comisionSeleccionada;
            set
            {
                _comisionSeleccionada = value;
                OnPropertyChanged();
                _ = CargarTrabajosAsync();
            }
        }

        private TrabajoPractico? _trabajoSeleccionado;

        /// <summary>
        /// Trabajo elegido; al cambiarlo se cargan las entregas de los alumnos.
        /// </summary>
        public TrabajoPractico? TrabajoSeleccionado
        {
            get => _trabajoSeleccionado;
            set
            {
                _trabajoSeleccionado = value;
                OnPropertyChanged();
                _ = CargarFilasAsync();
            }
        }

        private string _tituloNuevo = string.Empty;

        /// <summary>
        /// Título del trabajo nuevo, en el formulario de alta.
        /// </summary>
        public string TituloNuevo
        {
            get => _tituloNuevo;
            set { _tituloNuevo = value; OnPropertyChanged(); }
        }

        private DateTime _fechaNueva = DateTime.Today;

        /// <summary>
        /// Fecha de entrega (vencimiento) del trabajo nuevo.
        /// </summary>
        public DateTime FechaNueva
        {
            get => _fechaNueva;
            set { _fechaNueva = value; OnPropertyChanged(); }
        }

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
        /// Comando para crear un trabajo nuevo en la comisión elegida.
        /// </summary>
        public Command CrearTrabajoCommand { get; }

        /// <summary>
        /// Comando para guardar las entregas marcadas.
        /// </summary>
        public Command GuardarCommand { get; }

        /// <summary>
        /// Crea el ViewModel con el acceso a datos y el servicio de avisos.
        /// </summary>
        public EntregasViewModel(IDataService dataService, IDialogoService dialogo)
        {
            _dataService = dataService;
            _dialogo = dialogo;

            CrearTrabajoCommand = new Command(async () => await CrearTrabajoAsync());
            GuardarCommand = new Command(async () => await GuardarAsync());
        }

        /// <summary>
        /// Carga las comisiones y conserva la elegida (o elige la primera).
        /// </summary>
        public async Task CargarComisionesAsync()
        {
            try
            {
                var idPrevio = ComisionSeleccionada?.Id;
                var lista = await _dataService.GetComisionesAsync();

                RepoblarComisiones(lista);
                ComisionSeleccionada = Comisiones.FirstOrDefault(c => c.Id == idPrevio)
                                       ?? Comisiones.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                Estado = "No se pudieron cargar las comisiones.";
            }
        }

        /// <summary>
        /// Reemplaza el selector de comisiones por la lista recibida, ordenada por nombre.
        /// </summary>
        private void RepoblarComisiones(IEnumerable<Comision> lista)
        {
            Comisiones.Clear();
            foreach (var c in lista.OrderBy(c => c.Nombre))
            {
                Comisiones.Add(c);
            }
        }

        /// <summary>
        /// Carga los trabajos de la comisión elegida. Si se pasa un Id, ese trabajo queda seleccionado
        /// (se usa para dejar elegido el que se acaba de crear).
        /// </summary>
        private async Task CargarTrabajosAsync(int? seleccionarTrabajoId = null)
        {
            var idPrevio = seleccionarTrabajoId ?? TrabajoSeleccionado?.Id;
            var comision = ComisionSeleccionada;
            Trabajos.Clear();

            if (comision == null)
                TrabajoSeleccionado = null;
            else
                await TraerTrabajosAsync(comision, idPrevio);
        }

        /// <summary>
        /// Pide los trabajos de la comisión a la base y los muestra.
        /// Si mientras consultaba cambió la comisión elegida, descarta el resultado porque ya no sirve.
        /// </summary>
        private async Task TraerTrabajosAsync(Comision comision, int? idPrevio)
        {
            try
            {
                var lista = await _dataService.GetTrabajosPracticosByComisionIdAsync(comision.Id);
                if (comision != ComisionSeleccionada) return;

                MostrarTrabajos(lista, idPrevio);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                Estado = "No se pudieron cargar los trabajos.";
            }
        }

        /// <summary>
        /// Llena la lista de trabajos por fecha de entrega y deja elegido el indicado (o el primero).
        /// </summary>
        private void MostrarTrabajos(IEnumerable<TrabajoPractico> lista, int? idPrevio)
        {
            Trabajos.Clear();
            foreach (var tp in lista.OrderBy(t => t.FechaEntrega).ThenBy(t => t.Id))
            {
                Trabajos.Add(tp);
            }

            TrabajoSeleccionado = Trabajos.FirstOrDefault(t => t.Id == idPrevio)
                                  ?? Trabajos.FirstOrDefault();
        }

        /// <summary>
        /// Carga la lista de alumnos con su estado de entrega del trabajo elegido, o explica qué falta elegir.
        /// </summary>
        private async Task CargarFilasAsync()
        {
            Filas.Clear();
            var motivo = ObtenerMotivoParaNoCargarFilas();

            if (motivo != null)
                Estado = motivo;
            else
                await TraerFilasAsync(ComisionSeleccionada!, TrabajoSeleccionado!);
        }

        /// <summary>
        /// Devuelve qué falta elegir (la comisión o el trabajo), o null si ya se pueden cargar las entregas.
        /// </summary>
        private string? ObtenerMotivoParaNoCargarFilas()
        {
            if (ComisionSeleccionada == null)
                return "Seleccione una comisión.";

            if (TrabajoSeleccionado != null)
                return null;

            return Trabajos.Count == 0
                ? "La comisión no tiene trabajos. Creá el primero arriba."
                : "Seleccione un trabajo.";
        }

        /// <summary>
        /// Pide los alumnos y sus entregas y los muestra.
        /// Si mientras consultaba cambió la comisión o el trabajo, descarta el resultado porque ya no sirve.
        /// </summary>
        private async Task TraerFilasAsync(Comision comision, TrabajoPractico tp)
        {
            try
            {
                var estudiantes = await _dataService.GetEstudiantesByComisionIdAsync(comision.Id);
                var entregas = await BuscarEntregasAsync(estudiantes, tp);
                if (comision != ComisionSeleccionada || tp != TrabajoSeleccionado) return;

                MostrarFilas(estudiantes, entregas);
                Estado = ResumirEntregas(tp);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                Estado = "No se pudieron cargar las entregas.";
            }
        }

        /// <summary>
        /// Busca la entrega de cada alumno para el trabajo. No hay un método que traiga las entregas
        /// de un trabajo, así que se pregunta alumno por alumno.
        /// </summary>
        private async Task<Dictionary<int, Entrega?>> BuscarEntregasAsync(List<Estudiante> estudiantes, TrabajoPractico tp)
        {
            var entregaPorAlumno = new Dictionary<int, Entrega?>();

            foreach (var est in estudiantes)
            {
                var entregas = await _dataService.GetEntregasPorEstudianteAsync(est.Id);
                entregaPorAlumno[est.Id] = entregas.FirstOrDefault(e => e.TrabajoPracticoId == tp.Id);
            }

            return entregaPorAlumno;
        }

        /// <summary>
        /// Llena la lista con los alumnos ordenados por apellido.
        /// </summary>
        private void MostrarFilas(List<Estudiante> estudiantes, Dictionary<int, Entrega?> entregaPorAlumno)
        {
            Filas.Clear();
            foreach (var est in estudiantes.OrderBy(e => e.Apellido).ThenBy(e => e.Nombre))
            {
                Filas.Add(CrearFila(est, entregaPorAlumno[est.Id]));
            }
        }

        /// <summary>
        /// Arma la fila de un alumno. Sin registro previo, figura como no entregado.
        /// </summary>
        private static EntregaFila CrearFila(Estudiante estudiante, Entrega? entrega) => new()
        {
            EstudianteId = estudiante.Id,
            NombreCompleto = $"{estudiante.Apellido}, {estudiante.Nombre}",
            EntregaId = entrega?.Id ?? 0,
            Entregado = entrega?.Entregado ?? false
        };

        /// <summary>
        /// Texto de resumen: el trabajo, su fecha de entrega y cuántos alumnos lo entregaron.
        /// </summary>
        private string ResumirEntregas(TrabajoPractico tp)
        {
            if (Filas.Count == 0)
                return "La comisión no tiene alumnos cargados.";

            var entregaron = Filas.Count(f => f.Entregado);
            return $"{tp.Titulo} (entrega: {tp.FechaEntrega:dd/MM/yyyy}): {entregaron} de {Filas.Count} entregaron.";
        }

        /// <summary>
        /// Valida el trabajo nuevo y, si está bien, lo crea en la comisión elegida.
        /// </summary>
        private async Task CrearTrabajoAsync()
        {
            var comision = ComisionSeleccionada;
            var error = ObtenerErrorDelTrabajo(comision);

            if (error != null)
            {
                await _dialogo.AvisarAsync("Atención", error);
                return;
            }

            await GuardarTrabajoAsync(comision!, TituloLimpio);
        }

        /// <summary>
        /// Título escrito en el formulario, sin espacios de más.
        /// </summary>
        private string TituloLimpio => (TituloNuevo ?? string.Empty).Trim();

        /// <summary>
        /// Devuelve el primer problema del trabajo nuevo (comisión, título, fecha o título repetido), o null si está bien.
        /// </summary>
        private string? ObtenerErrorDelTrabajo(Comision? comision)
        {
            if (comision == null)
                return "Elegí una comisión antes de crear un trabajo.";

            return ValidacionTrabajo.ObtenerError(TituloLimpio, FechaNueva, DateTime.Today)
                   ?? ErrorDeTituloRepetido();
        }

        /// <summary>
        /// Avisa si la comisión ya tiene un trabajo con ese título (sin distinguir mayúsculas).
        /// </summary>
        private string? ErrorDeTituloRepetido() =>
            Trabajos.Any(t => string.Equals(t.Titulo, TituloLimpio, StringComparison.OrdinalIgnoreCase))
                ? "Ya existe un trabajo con ese título en la comisión."
                : null;

        /// <summary>
        /// Guarda el trabajo nuevo y lo deja seleccionado para marcar entregas enseguida.
        /// </summary>
        private async Task GuardarTrabajoAsync(Comision comision, string titulo)
        {
            try
            {
                var nuevo = new TrabajoPractico(0, comision.Id, titulo, FechaNueva.Date);
                var guardado = await _dataService.SaveTrabajoPracticoAsync(nuevo);

                TituloNuevo = string.Empty;
                await CargarTrabajosAsync(guardado.Id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                await _dialogo.AvisarAsync("Error", "No se pudo crear el trabajo.");
            }
        }

        /// <summary>
        /// Guarda las entregas marcadas. Si ya se está guardando, no hace nada.
        /// </summary>
        private async Task GuardarAsync()
        {
            if (_guardando) return;

            if (ComisionSeleccionada == null || TrabajoSeleccionado == null || Filas.Count == 0)
                await _dialogo.AvisarAsync("Atención", "Elegí una comisión y un trabajo con alumnos antes de guardar.");
            else
                await GuardarConBloqueoAsync(TrabajoSeleccionado);
        }

        /// <summary>
        /// Marca que hay un guardado en curso mientras trabaja, y lo desmarca al terminar, salga como salga.
        /// </summary>
        private async Task GuardarConBloqueoAsync(TrabajoPractico tp)
        {
            _guardando = true;
            try
            {
                await GuardarYAvisarAsync(tp);
            }
            finally
            {
                _guardando = false;
            }
        }

        /// <summary>
        /// Guarda las entregas y, si salió bien, recarga la lista y avisa.
        /// Se recarga para que las filas nuevas tomen su Id y un segundo guardado actualice en vez de duplicar.
        /// </summary>
        private async Task GuardarYAvisarAsync(TrabajoPractico tp)
        {
            if (!await IntentarGuardarAsync(tp)) return;

            await CargarFilasAsync();
            await _dialogo.AvisarAsync("Éxito", "Entregas guardadas.");
        }

        /// <summary>
        /// Guarda todas las entregas de la lista. Si la base falla, avisa y devuelve false.
        /// </summary>
        private async Task<bool> IntentarGuardarAsync(TrabajoPractico tp)
        {
            try
            {
                var entregas = Filas
                    .Select(f => new Entrega(f.EntregaId, f.EstudianteId, tp.Id, f.Entregado))
                    .ToList();

                await _dataService.SaveEntregasAsync(entregas);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                await _dialogo.AvisarAsync("Error", "No se pudieron guardar las entregas.");
                return false;
            }
        }
    }
}
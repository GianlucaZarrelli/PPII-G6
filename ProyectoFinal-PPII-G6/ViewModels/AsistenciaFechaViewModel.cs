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
    /// Una fila de la lista de asistencia: un alumno, si estuvo presente y, si faltó, si la ausencia es justificada.
    /// </summary>
    public class AsistenciaFila : BindableObject
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
        /// Id del registro de asistencia ya guardado, o 0 si todavía no hay.
        /// </summary>
        public int AsistenciaId { get; set; }

        private bool _presente = true;

        /// <summary>
        /// Indica si el alumno estuvo presente. Al marcarlo presente se borra la justificación,
        /// porque una ausencia justificada solo tiene sentido si el alumno faltó.
        /// </summary>
        public bool Presente
        {
            get => _presente;
            set
            {
                if (_presente == value) return;
                _presente = value;

                if (value) _justificada = false;

                OnPropertyChanged();
                OnPropertyChanged(nameof(Ausente));
                OnPropertyChanged(nameof(Justificada));
            }
        }

        private bool _justificada;

        /// <summary>
        /// Indica si la ausencia está justificada (vale la mitad de una presente).
        /// </summary>
        public bool Justificada
        {
            get => _justificada;
            set { _justificada = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Lo contrario de presente: la pantalla lo usa para mostrar el interruptor de la justificación.
        /// </summary>
        public bool Ausente => !Presente;
    }

    /// <summary>
    /// ViewModel del registro de asistencia: se elige una comisión y una fecha, se marca quién vino y se guarda.
    /// Si la clase de esa fecha no existe, se crea al guardar.
    /// </summary>
    public class AsistenciaFechaViewModel : BindableObject
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
        /// Alumnos de la comisión con su asistencia en la fecha elegida.
        /// </summary>
        public ObservableCollection<AsistenciaFila> Filas { get; } = new();

        private Comision? _comisionSeleccionada;

        /// <summary>
        /// Comisión elegida; al cambiarla se recarga la lista de alumnos.
        /// </summary>
        public Comision? ComisionSeleccionada
        {
            get => _comisionSeleccionada;
            set
            {
                _comisionSeleccionada = value;
                OnPropertyChanged();
                _ = CargarFilasAsync();
            }
        }

        private DateTime _fecha = DateTime.Today;

        /// <summary>
        /// Fecha de la clase; al cambiarla se recarga la asistencia de ese día.
        /// </summary>
        public DateTime Fecha
        {
            get => _fecha;
            set
            {
                _fecha = value;
                OnPropertyChanged();
                _ = CargarFilasAsync();
            }
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
        /// Comando para guardar la asistencia de la fecha elegida.
        /// </summary>
        public Command GuardarCommand { get; }

        /// <summary>
        /// Crea el ViewModel con el acceso a datos y el servicio de avisos.
        /// </summary>
        public AsistenciaFechaViewModel(IDataService dataService, IDialogoService dialogo)
        {
            _dataService = dataService;
            _dialogo = dialogo;

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
        /// Carga la lista de alumnos con su asistencia en la fecha elegida, o avisa que hay que elegir una comisión.
        /// </summary>
        private async Task CargarFilasAsync()
        {
            Filas.Clear();
            var comision = ComisionSeleccionada;

            if (comision == null)
                Estado = "Seleccione una comisión.";
            else
                await TraerFilasAsync(comision, Fecha.Date);
        }

        /// <summary>
        /// Pide los alumnos, la clase de ese día y sus asistencias, y los muestra.
        /// Si mientras consultaba cambió la comisión o la fecha, descarta el resultado porque ya no sirve.
        /// </summary>
        private async Task TraerFilasAsync(Comision comision, DateTime fecha)
        {
            try
            {
                var estudiantes = await _dataService.GetEstudiantesByComisionIdAsync(comision.Id);
                var clase = await BuscarClaseAsync(comision.Id, fecha);
                var asistencias = await BuscarAsistenciasAsync(clase);
                if (comision != ComisionSeleccionada || fecha != Fecha.Date) return;

                MostrarFilas(estudiantes, asistencias);
                Estado = ResumirClase(clase, fecha);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                Estado = "No se pudo cargar la asistencia.";
            }
        }

        /// <summary>
        /// Busca la clase de la comisión en esa fecha. Devuelve null si todavía no existe.
        /// </summary>
        private async Task<Clase?> BuscarClaseAsync(int comisionId, DateTime fecha)
        {
            var clases = await _dataService.GetClasesByComisionIdAsync(comisionId);
            return clases.FirstOrDefault(c => c.Fecha.Date == fecha);
        }

        /// <summary>
        /// Trae las asistencias de la clase. Si la clase todavía no existe, no hay ninguna.
        /// </summary>
        private async Task<List<Asistencia>> BuscarAsistenciasAsync(Clase? clase)
        {
            if (clase == null) return new List<Asistencia>();

            var todas = await _dataService.GetTodasLasAsistenciasAsync();
            return todas.Where(a => a.ClaseId == clase.Id).ToList();
        }

        /// <summary>
        /// Llena la lista con los alumnos ordenados por apellido.
        /// </summary>
        private void MostrarFilas(List<Estudiante> estudiantes, List<Asistencia> asistencias)
        {
            Filas.Clear();
            foreach (var est in estudiantes.OrderBy(e => e.Apellido).ThenBy(e => e.Nombre))
            {
                Filas.Add(CrearFila(est, asistencias.FirstOrDefault(a => a.EstudianteId == est.Id)));
            }
        }

        /// <summary>
        /// Arma la fila de un alumno. Sin registro previo, figura como presente.
        /// </summary>
        private static AsistenciaFila CrearFila(Estudiante estudiante, Asistencia? asistencia) => new()
        {
            EstudianteId = estudiante.Id,
            NombreCompleto = $"{estudiante.Apellido}, {estudiante.Nombre}",
            AsistenciaId = asistencia?.Id ?? 0,
            Presente = asistencia?.Presente ?? true,
            Justificada = asistencia?.Justificada ?? false
        };

        /// <summary>
        /// Texto de estado: avisa si no hay alumnos, si la clase se va a crear al guardar, o cuántos alumnos tiene.
        /// </summary>
        private string ResumirClase(Clase? clase, DateTime fecha)
        {
            if (Filas.Count == 0)
                return "La comisión no tiene alumnos cargados.";

            return clase == null
                ? $"No hay clase registrada el {fecha:dd/MM/yyyy}. Al guardar se crea."
                : $"Clase del {fecha:dd/MM/yyyy}: {Filas.Count} alumnos.";
        }

        /// <summary>
        /// Valida la fecha y la lista y, si están bien, guarda la asistencia. Si ya se está guardando, no hace nada.
        /// </summary>
        private async Task GuardarAsync()
        {
            if (_guardando) return;

            var error = ObtenerErrorParaGuardar();

            if (error != null)
                await _dialogo.AvisarAsync("Atención", error);
            else
                await GuardarConBloqueoAsync(ComisionSeleccionada!, Fecha.Date);
        }

        /// <summary>
        /// Devuelve por qué no se puede guardar (falta la comisión o los alumnos, o la fecha es futura), o null si se puede.
        /// </summary>
        private string? ObtenerErrorParaGuardar()
        {
            if (ComisionSeleccionada == null || Filas.Count == 0)
                return "Elegí una comisión con alumnos antes de guardar.";

            return ValidacionAsistencia.ObtenerErrorDeFecha(Fecha, DateTime.Today);
        }

        /// <summary>
        /// Marca que hay un guardado en curso mientras trabaja, y lo desmarca al terminar, salga como salga.
        /// </summary>
        private async Task GuardarConBloqueoAsync(Comision comision, DateTime fecha)
        {
            _guardando = true;
            try
            {
                await GuardarYAvisarAsync(comision, fecha);
            }
            finally
            {
                _guardando = false;
            }
        }

        /// <summary>
        /// Guarda la asistencia y, si salió bien, recarga la lista y avisa.
        /// Se recarga para que las filas nuevas tomen su Id y un segundo guardado actualice en vez de duplicar.
        /// </summary>
        private async Task GuardarYAvisarAsync(Comision comision, DateTime fecha)
        {
            if (!await IntentarGuardarAsync(comision, fecha)) return;

            await CargarFilasAsync();
            await _dialogo.AvisarAsync("Éxito", "Asistencia guardada.");
        }

        /// <summary>
        /// Guarda la asistencia de todos los alumnos de la lista. Si la base falla, avisa y devuelve false.
        /// </summary>
        private async Task<bool> IntentarGuardarAsync(Comision comision, DateTime fecha)
        {
            try
            {
                var claseId = await ObtenerOCrearClaseAsync(comision, fecha);
                var asistencias = Filas.Select(f => CrearAsistencia(f, claseId)).ToList();

                await _dataService.SaveAsistenciasAsync(asistencias);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                await _dialogo.AvisarAsync("Error", "No se pudo guardar la asistencia.");
                return false;
            }
        }

        /// <summary>
        /// Devuelve el Id de la clase de esa fecha, y si todavía no existe la crea.
        /// </summary>
        private async Task<int> ObtenerOCrearClaseAsync(Comision comision, DateTime fecha)
        {
            var clase = await BuscarClaseAsync(comision.Id, fecha);
            if (clase != null) return clase.Id;

            var nueva = new Clase(0, fecha, $"Clase {fecha:dd/MM/yyyy}", comision.Id);
            return await _dataService.SaveClaseAsync(nueva);
        }

        /// <summary>
        /// Convierte una fila en el registro que se guarda. La justificación solo vale si el alumno faltó.
        /// </summary>
        private static Asistencia CrearAsistencia(AsistenciaFila fila, int claseId) =>
            new(fila.AsistenciaId, fila.EstudianteId, claseId, fila.Presente, !fila.Presente && fila.Justificada);
    }
}
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
    /// Una fila de la pantalla: un alumno y si entregó el trabajo elegido.
    /// </summary>
    public class EntregaFila : BindableObject
    {
        /// <summary>
        /// Id del alumno de la fila.
        /// </summary>
        public int EstudianteId { get; set; }
        /// <summary>
        /// Nombre del alumno con formato "Apellido, Nombre".
        /// </summary>
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Id del registro existente; 0 si el alumno todavía no tiene entrega registrada.
        /// </summary>
        public int EntregaId { get; set; }

        private bool _entregado;

        /// <summary>
        /// Indica si el alumno entregó el trabajo; notifica el cambio a la vista.
        /// </summary>
        public bool Entregado
        {
            get => _entregado;
            set { _entregado = value; OnPropertyChanged(); }
        }
    }

    /// <summary>
    /// ViewModel para crear trabajos prácticos y registrar quién entregó cada uno.
    /// </summary>
    public class EntregasViewModel : BindableObject
    {
        private readonly IDataService _dataService;
        private bool _guardando;

        /// <summary>
        /// Comisiones disponibles para elegir.
        /// </summary>
        public ObservableCollection<Comision> Comisiones { get; } = new();
        /// <summary>
        /// Trabajos prácticos de la comisión elegida, ordenados por fecha de entrega.
        /// </summary>
        public ObservableCollection<TrabajoPractico> Trabajos { get; } = new();
        /// <summary>
        /// Alumnos de la comisión con su estado de entrega para el trabajo elegido.
        /// </summary>
        public ObservableCollection<EntregaFila> Filas { get; } = new();

        private Comision? _comisionSeleccionada;

        /// <summary>
        /// Comisión elegida; al cambiar recarga sus trabajos.
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
        /// Trabajo práctico elegido; al cambiar recarga las filas de alumnos.
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
        /// Título ingresado para el trabajo práctico a crear.
        /// </summary>
        public string TituloNuevo
        {
            get => _tituloNuevo;
            set { _tituloNuevo = value; OnPropertyChanged(); }
        }

        private DateTime _fechaNueva = DateTime.Today;

        /// <summary>
        /// Fecha de entrega del trabajo práctico a crear (por defecto, hoy).
        /// </summary>
        public DateTime FechaNueva
        {
            get => _fechaNueva;
            set { _fechaNueva = value; OnPropertyChanged(); }
        }

        private string _estado = "Seleccione una comisión.";

        /// <summary>
        /// Mensaje informativo que se muestra al usuario (resumen de entregas o errores).
        /// </summary>
        public string Estado
        {
            get => _estado;
            set { _estado = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Comando que crea un trabajo práctico nuevo en la comisión elegida.
        /// </summary>
        public Command CrearTrabajoCommand { get; }
        /// <summary>
        /// Comando que guarda el estado de entrega de todos los alumnos para el trabajo elegido.
        /// </summary>
        public Command GuardarCommand { get; }

        /// <summary>
        /// Crea el ViewModel con el servicio de datos e inicializa los comandos.
        /// </summary>
        public EntregasViewModel(IDataService dataService)
        {
            _dataService = dataService;
            CrearTrabajoCommand = new Command(async () => await CrearTrabajoAsync());
            GuardarCommand = new Command(async () => await GuardarAsync());
        }

        /// <summary>
        /// Muestra una alerta con título y mensaje si hay una página principal disponible.
        /// </summary>
        private static async Task AvisarAsync(string titulo, string mensaje)
        {
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert(titulo, mensaje, "OK");
        }

        /// <summary>
        /// Recarga las comisiones y conserva la selección anterior (o elige la primera).
        /// </summary>
        public async Task CargarComisionesAsync()
        {
            try
            {
                var idPrevio = ComisionSeleccionada?.Id;
                var lista = await _dataService.GetComisionesAsync();

                Comisiones.Clear();
                foreach (var c in lista)
                {
                    Comisiones.Add(c);
                }

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
        /// Carga los trabajos de la comisión elegida. Si se pasa un Id, deja ese trabajo seleccionado.
        /// </summary>
        private async Task CargarTrabajosAsync(int? seleccionarTrabajoId = null)
        {
            var idPrevio = seleccionarTrabajoId ?? TrabajoSeleccionado?.Id;
            var comision = ComisionSeleccionada;

            Trabajos.Clear();

            if (comision == null)
            {
                TrabajoSeleccionado = null;
                return;
            }

            try
            {
                var lista = await _dataService.GetTrabajosPracticosByComisionIdAsync(comision.Id);

                // Si mientras consultaba cambió la comisión, este resultado ya no sirve
                if (comision != ComisionSeleccionada) return;

                Trabajos.Clear();
                foreach (var tp in lista.OrderBy(t => t.FechaEntrega).ThenBy(t => t.Id))
                {
                    Trabajos.Add(tp);
                }

                TrabajoSeleccionado = Trabajos.FirstOrDefault(t => t.Id == idPrevio)
                                      ?? Trabajos.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                Estado = "No se pudieron cargar los trabajos.";
            }
        }

        /// <summary>
        /// Arma la lista de alumnos con su estado de entrega para el trabajo elegido.
        /// </summary>
        private async Task CargarFilasAsync()
        {
            Filas.Clear();

            if (ComisionSeleccionada == null)
            {
                Estado = "Seleccione una comisión.";
                return;
            }

            if (TrabajoSeleccionado == null)
            {
                Estado = Trabajos.Count == 0
                    ? "La comisión no tiene trabajos. Creá el primero arriba."
                    : "Seleccione un trabajo.";
                return;
            }

            var comision = ComisionSeleccionada;
            var tp = TrabajoSeleccionado;

            try
            {
                var estudiantes = await _dataService.GetEstudiantesByComisionIdAsync(comision.Id);

                // No hay un método para traer las entregas de un trabajo: se busca alumno por alumno
                var entregaPorAlumno = new Dictionary<int, Entrega?>();
                foreach (var est in estudiantes)
                {
                    var entregas = await _dataService.GetEntregasPorEstudianteAsync(est.Id);
                    entregaPorAlumno[est.Id] = entregas.FirstOrDefault(e => e.TrabajoPracticoId == tp.Id);
                }

                // Si mientras consultaba cambió la comisión o el trabajo, este resultado ya no sirve
                if (comision != ComisionSeleccionada || tp != TrabajoSeleccionado) return;

                Filas.Clear();
                foreach (var est in estudiantes.OrderBy(e => e.Apellido).ThenBy(e => e.Nombre))
                {
                    var entrega = entregaPorAlumno[est.Id];

                    Filas.Add(new EntregaFila
                    {
                        EstudianteId = est.Id,
                        NombreCompleto = $"{est.Apellido}, {est.Nombre}",
                        EntregaId = entrega?.Id ?? 0,
                        Entregado = entrega?.Entregado ?? false
                    });
                }

                if (Filas.Count == 0)
                {
                    Estado = "La comisión no tiene alumnos cargados.";
                }
                else
                {
                    var entregaron = Filas.Count(f => f.Entregado);
                    Estado = $"{tp.Titulo} (entrega: {tp.FechaEntrega:dd/MM/yyyy}): {entregaron} de {Filas.Count} entregaron.";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                Estado = "No se pudieron cargar las entregas.";
            }
        }

        /// <summary>
        /// Valida el título (obligatorio y sin repetir en la comisión), crea el trabajo y lo deja seleccionado.
        /// </summary>
        private async Task CrearTrabajoAsync()
        {
            if (ComisionSeleccionada == null)
            {
                await AvisarAsync("Atención", "Elegí una comisión antes de crear un trabajo.");
                return;
            }

            var titulo = (TituloNuevo ?? string.Empty).Trim();
            if (titulo.Length == 0)
            {
                await AvisarAsync("Atención", "El título del trabajo es obligatorio.");
                return;
            }

            if (Trabajos.Any(t => string.Equals(t.Titulo, titulo, StringComparison.OrdinalIgnoreCase)))
            {
                await AvisarAsync("Atención", "Ya existe un trabajo con ese título en la comisión.");
                return;
            }

            try
            {
                var nuevo = new TrabajoPractico(0, ComisionSeleccionada.Id, titulo, FechaNueva.Date);
                var guardado = await _dataService.SaveTrabajoPracticoAsync(nuevo);

                TituloNuevo = string.Empty;

                // Deja seleccionado el trabajo nuevo para marcar entregas enseguida
                await CargarTrabajosAsync(guardado.Id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                await AvisarAsync("Error", "No se pudo crear el trabajo.");
            }
        }

        /// <summary>
        /// Guarda las entregas de todas las filas para el trabajo elegido y recarga la lista; evita guardados simultáneos.
        /// </summary>
        private async Task GuardarAsync()
        {
            if (ComisionSeleccionada == null || TrabajoSeleccionado == null || Filas.Count == 0)
            {
                await AvisarAsync("Atención", "Elegí una comisión y un trabajo con alumnos antes de guardar.");
                return;
            }

            if (_guardando) return;
            _guardando = true;

            try
            {
                var tp = TrabajoSeleccionado;

                try
                {
                    var entregas = Filas
                        .Select(f => new Entrega(f.EntregaId, f.EstudianteId, tp.Id, f.Entregado))
                        .ToList();

                    await _dataService.SaveEntregasAsync(entregas);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                    await AvisarAsync("Error", "No se pudieron guardar las entregas.");
                    return;
                }

                // Recargo para que las filas nuevas tomen su Id y un segundo guardado actualice en vez de duplicar
                await CargarFilasAsync();
                await AvisarAsync("Éxito", "Entregas guardadas.");
            }
            finally
            {
                _guardando = false;
            }
        }
    }
}
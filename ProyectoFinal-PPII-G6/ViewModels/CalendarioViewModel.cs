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
    /// Una fila del calendario de la comisión: una clase o la entrega de un trabajo.
    /// </summary>
    public class FilaCalendario
    {
        /// <summary>
        /// Fecha del evento, en texto.
        /// </summary>
        public string Fecha { get; set; } = string.Empty;

        /// <summary>
        /// Qué es: "Clase" o "Entrega de trabajo".
        /// </summary>
        public string Tipo { get; set; } = string.Empty;

        /// <summary>
        /// Descripción de la clase o título del trabajo.
        /// </summary>
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Cuánto falta o cuánto pasó, por ejemplo "En 3 días".
        /// </summary>
        public string Etapa { get; set; } = string.Empty;

        /// <summary>
        /// Color de la etapa, en hexadecimal.
        /// </summary>
        public string Color { get; set; } = "#757575";

        /// <summary>
        /// Arma la fila a partir de un evento del calendario; "hoy" se recibe para poder probar la etapa.
        /// </summary>
        public static FilaCalendario Desde(EventoCalendario evento, DateTime hoy) => new()
        {
            Fecha = evento.Fecha.ToString("dd/MM/yyyy"),
            Tipo = PresentacionCalendario.Tipo(evento.Tipo),
            Titulo = evento.Titulo,
            Etapa = PresentacionCalendario.Etapa(evento.Fecha, hoy),
            Color = PresentacionCalendario.Color(evento.Fecha, hoy)
        };
    }

    /// <summary>
    /// ViewModel del calendario: se elige una comisión, se ve el calendario de sus clases y trabajos,
    /// y se pueden cargar las fechas de las clases. Los trabajos se cargan en la pestaña Entregas.
    /// </summary>
    public class CalendarioViewModel : BindableObject
    {
        private readonly IDataService _dataService;
        private readonly IDialogoService _dialogo;

        /// <summary>
        /// Clases de la comisión elegida, tal como se cargaron: sirven para detectar una fecha repetida.
        /// </summary>
        private List<Clase> _clases = new();

        /// <summary>
        /// Evita que un doble toque en "Agregar clase" guarde dos veces.
        /// </summary>
        private bool _guardando;

        /// <summary>
        /// Comisiones disponibles para elegir.
        /// </summary>
        public ObservableCollection<Comision> Comisiones { get; } = new();

        /// <summary>
        /// Clases y entregas de la comisión elegida, por fecha.
        /// </summary>
        public ObservableCollection<FilaCalendario> Filas { get; } = new();

        /// <summary>
        /// Largo máximo de la descripción, para limitar el cuadro de texto.
        /// </summary>
        public int LargoMaximoDescripcion => ValidacionClase.LargoMaximoDescripcion;

        private Comision? _comisionSeleccionada;

        /// <summary>
        /// Comisión elegida; al cambiarla se carga su calendario.
        /// </summary>
        public Comision? ComisionSeleccionada
        {
            get => _comisionSeleccionada;
            set
            {
                _comisionSeleccionada = value;
                OnPropertyChanged();
                _ = CargarCalendarioAsync();
            }
        }

        private DateTime _fechaClase = DateTime.Today;

        /// <summary>
        /// Día de la clase que se quiere agregar.
        /// </summary>
        public DateTime FechaClase
        {
            get => _fechaClase;
            set { _fechaClase = value; OnPropertyChanged(); }
        }

        private string _descripcionClase = string.Empty;

        /// <summary>
        /// Descripción de la clase que se quiere agregar (es opcional).
        /// </summary>
        public string DescripcionClase
        {
            get => _descripcionClase;
            set { _descripcionClase = value ?? string.Empty; OnPropertyChanged(); }
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
        /// Comando para agregar la clase al calendario.
        /// </summary>
        public Command AgregarClaseCommand { get; }

        /// <summary>
        /// Crea el ViewModel con el acceso a datos y el servicio de avisos.
        /// </summary>
        public CalendarioViewModel(IDataService dataService, IDialogoService dialogo)
        {
            _dataService = dataService;
            _dialogo = dialogo;
            AgregarClaseCommand = new Command(async () => await AgregarClaseAsync());
        }

        /// <summary>
        /// Recarga las comisiones. Si no cambiaron, solo refresca el calendario y conserva la comisión elegida.
        /// </summary>
        public async Task CargarComisionesAsync()
        {
            try
            {
                var lista = await _dataService.GetComisionesAsync();

                if (MismasComisiones(lista))
                    await CargarCalendarioAsync();
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
        /// Al elegir una comisión, el setter carga su calendario.
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
        /// Carga el calendario de la comisión elegida, o explica qué falta elegir.
        /// </summary>
        private async Task CargarCalendarioAsync()
        {
            var comision = ComisionSeleccionada;
            Filas.Clear();
            _clases = new List<Clase>();

            if (comision == null)
                Estado = "Seleccione una comisión.";
            else
                await TraerCalendarioAsync(comision);
        }

        /// <summary>
        /// Pide las clases y los trabajos de la comisión a la base y los muestra.
        /// Si mientras consultaba cambió la comisión, descarta el resultado porque ya no sirve.
        /// </summary>
        private async Task TraerCalendarioAsync(Comision comision)
        {
            try
            {
                var clases = await _dataService.GetClasesByComisionIdAsync(comision.Id);
                var trabajos = await _dataService.GetTrabajosPracticosByComisionIdAsync(comision.Id);
                if (comision != ComisionSeleccionada) return;

                MostrarCalendario(comision, clases, trabajos);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                Estado = "No se pudo cargar el calendario.";
            }
        }

        /// <summary>
        /// Llena la lista con el calendario ordenado por fecha y resume cuántas clases y trabajos tiene.
        /// </summary>
        private void MostrarCalendario(Comision comision, List<Clase> clases, List<TrabajoPractico> trabajos)
        {
            _clases = clases;
            Filas.Clear();

            foreach (var evento in CalendarioComision.Armar(clases, trabajos))
            {
                Filas.Add(FilaCalendario.Desde(evento, DateTime.Today));
            }

            Estado = clases.Count + trabajos.Count == 0
                ? $"La comisión {comision.Nombre} todavía no tiene fechas cargadas."
                : $"Comisión {comision.Nombre}  |  Clases: {clases.Count}  |  Trabajos: {trabajos.Count}";
        }

        /// <summary>
        /// Valida la clase y, si está bien, la agrega al calendario. Si ya se está guardando, no hace nada.
        /// Es pública para poder probarla sin la pantalla.
        /// </summary>
        public async Task AgregarClaseAsync()
        {
            if (_guardando) return;

            var error = ObtenerErrorDeClase();

            if (error != null)
                await _dialogo.AvisarAsync("Atención", error);
            else
                await GuardarConBloqueoAsync(ComisionSeleccionada!);
        }

        /// <summary>
        /// Devuelve el primer problema de la clase (comisión, fecha, descripción o fecha repetida), o null si está bien.
        /// </summary>
        private string? ObtenerErrorDeClase() =>
            ComisionSeleccionada == null
                ? "Elegí una comisión antes de agregar una clase."
                : ValidacionClase.ObtenerError(FechaClase, DescripcionClase, DateTime.Today) ?? ErrorDeFechaRepetida();

        /// <summary>
        /// Avisa si la comisión ya tiene una clase ese día: la base no admite dos clases en la misma fecha.
        /// </summary>
        private string? ErrorDeFechaRepetida() =>
            _clases.Any(c => c.Fecha.Date == FechaClase.Date)
                ? "Ya hay una clase cargada ese día en la comisión."
                : null;

        /// <summary>
        /// Marca que hay un guardado en curso mientras trabaja, y lo desmarca al terminar, salga como salga.
        /// </summary>
        private async Task GuardarConBloqueoAsync(Comision comision)
        {
            _guardando = true;
            try
            {
                await IntentarGuardarAsync(comision);
            }
            finally
            {
                _guardando = false;
            }
        }

        /// <summary>
        /// Guarda la clase y, si salió bien, limpia la descripción, actualiza el calendario y avisa.
        /// Si la base falla, avisa y conserva lo escrito.
        /// </summary>
        private async Task IntentarGuardarAsync(Comision comision)
        {
            try
            {
                var clase = new Clase(0, FechaClase.Date, DescripcionClase.Trim(), comision.Id);
                await _dataService.SaveClaseAsync(clase);

                DescripcionClase = string.Empty;
                await CargarCalendarioAsync();
                await _dialogo.AvisarAsync("Éxito", "Clase agregada al calendario.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                await _dialogo.AvisarAsync("Error", "No se pudo agregar la clase.");
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Presentacion;
using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.ViewModels
{
    /// <summary>
    /// Una fila de la vista previa de la importación: un alumno del archivo y lo que se hará con él.
    /// </summary>
    public class ImportacionFila
    {
        /// <summary>
        /// DNI de la fila, ya normalizado.
        /// </summary>
        public string Dni { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del alumno.
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Apellido del alumno.
        /// </summary>
        public string Apellido { get; set; } = string.Empty;

        /// <summary>
        /// Email del alumno.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Nombre y apellido juntos, o "(sin nombre)" si la fila no los trae.
        /// </summary>
        public string NombreCompleto =>
            string.IsNullOrWhiteSpace($"{Nombre} {Apellido}") ? "(sin nombre)" : $"{Nombre} {Apellido}".Trim();

        /// <summary>
        /// DNI y email en una línea, para mostrar debajo del nombre.
        /// </summary>
        public string Detalle =>
            string.IsNullOrWhiteSpace(Email) ? $"DNI: {Dni}" : $"DNI: {Dni}  |  {Email}";

        /// <summary>
        /// Texto que indica qué se hará con la fila.
        /// </summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>
        /// Color del estado, en hexadecimal.
        /// </summary>
        public string Color { get; set; } = "#757575";

        /// <summary>
        /// Indica si la fila se guarda al importar (solo las nuevas y válidas).
        /// </summary>
        public bool SeImporta { get; set; }
    }

    /// <summary>
    /// ViewModel de la carga masiva: se elige una comisión y un CSV, se ve qué se puede importar y se guarda todo junto.
    /// </summary>
    public class CargaMasivaViewModel : BindableObject
    {
        private readonly IDataService _dataService;
        private readonly CsvImporterService _csvImporter;
        private readonly IDialogoService _dialogo;

        /// <summary>
        /// Alumnos leídos del archivo elegido, sin evaluar.
        /// </summary>
        private List<Estudiante> _leidos = new();

        /// <summary>
        /// Evita que un doble toque en "Importar" guarde dos veces.
        /// </summary>
        private bool _importando;

        /// <summary>
        /// Comisiones disponibles para elegir.
        /// </summary>
        public ObservableCollection<Comision> Comisiones { get; } = new();

        /// <summary>
        /// Vista previa de las filas del archivo.
        /// </summary>
        public ObservableCollection<ImportacionFila> Filas { get; } = new();

        private Comision? _comisionSeleccionada;

        /// <summary>
        /// Comisión elegida; al cambiarla se vuelve a evaluar el archivo contra sus alumnos.
        /// </summary>
        public Comision? ComisionSeleccionada
        {
            get => _comisionSeleccionada;
            set
            {
                _comisionSeleccionada = value;
                OnPropertyChanged();
                _ = EvaluarAsync();
            }
        }

        private string _nombreArchivo = string.Empty;

        /// <summary>
        /// Nombre del archivo elegido.
        /// </summary>
        public string NombreArchivo
        {
            get => _nombreArchivo;
            set { _nombreArchivo = value; OnPropertyChanged(); }
        }

        private string _estado = "Elegí una comisión y un archivo CSV.";

        /// <summary>
        /// Resumen de lo que se puede importar, o qué falta para poder hacerlo.
        /// </summary>
        public string Estado
        {
            get => _estado;
            set { _estado = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Comando para elegir el archivo CSV.
        /// </summary>
        public Command ElegirArchivoCommand { get; }

        /// <summary>
        /// Comando para guardar los alumnos nuevos.
        /// </summary>
        public Command ImportarCommand { get; }

        /// <summary>
        /// Crea el ViewModel con el acceso a datos, el lector de CSV y el servicio de avisos.
        /// </summary>
        public CargaMasivaViewModel(IDataService dataService, CsvImporterService csvImporter, IDialogoService dialogo)
        {
            _dataService = dataService;
            _csvImporter = csvImporter;
            _dialogo = dialogo;

            ElegirArchivoCommand = new Command(async () => await ElegirArchivoAsync());
            ImportarCommand = new Command(async () => await ImportarAsync());
        }

        /// <summary>
        /// Carga las comisiones, conserva la elegida (o elige la primera) y evalúa el archivo si ya había uno.
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
        /// Abre el selector de archivos, lee el CSV elegido y lo evalúa contra la comisión.
        /// </summary>
        private async Task ElegirArchivoAsync()
        {
            try
            {
                var archivo = await FilePicker.Default.PickAsync(OpcionesDelSelector());

                // Si el usuario cancela el selector, no pasa nada
                if (archivo == null) return;

                await LeerArchivoAsync(archivo);
                await EvaluarAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                await _dialogo.AvisarAsync("Error", "No se pudo leer el archivo.");
            }
        }

        /// <summary>
        /// Configuración del selector de archivos: solo CSV, con el identificador de tipo de cada plataforma.
        /// </summary>
        private static PickOptions OpcionesDelSelector() => new()
        {
            PickerTitle = "Seleccione el archivo CSV de estudiantes",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".csv" } },
                { DevicePlatform.Android, new[] { "text/csv", "text/comma-separated-values" } },
                { DevicePlatform.iOS, new[] { "public.comma-separated-values-text" } },
                { DevicePlatform.MacCatalyst, new[] { "public.comma-separated-values-text" } }
            })
        };

        /// <summary>
        /// Lee los alumnos del archivo elegido y recuerda su nombre.
        /// </summary>
        private async Task LeerArchivoAsync(FileResult archivo)
        {
            using var stream = await archivo.OpenReadAsync();
            _leidos = await _csvImporter.ParseCsvAsync(stream, ComisionSeleccionada?.Id ?? 0);
            NombreArchivo = archivo.FileName;
        }

        /// <summary>
        /// Compara lo leído con los alumnos de la comisión y muestra la vista previa, o explica qué falta.
        /// </summary>
        private async Task EvaluarAsync()
        {
            Filas.Clear();
            var motivo = ObtenerMotivoParaNoEvaluar();

            if (motivo != null)
                Estado = motivo;
            else
                await ClasificarAsync(ComisionSeleccionada!);
        }

        /// <summary>
        /// Devuelve qué falta para poder evaluar el archivo (el archivo o la comisión), o null si ya se puede.
        /// </summary>
        private string? ObtenerMotivoParaNoEvaluar()
        {
            if (_leidos.Count == 0)
            {
                return string.IsNullOrEmpty(NombreArchivo)
                    ? "Elegí una comisión y un archivo CSV."
                    : "No se encontraron alumnos en el archivo. Formato: Dni;Nombre;Apellido;Email, con encabezado.";
            }

            return ComisionSeleccionada == null ? "Elegí una comisión para ver qué se puede importar." : null;
        }

        /// <summary>
        /// Trae los alumnos de la comisión, clasifica el archivo y muestra el resultado.
        /// Si mientras consultaba cambió la comisión elegida, descarta el resultado porque ya no sirve.
        /// </summary>
        private async Task ClasificarAsync(Comision comision)
        {
            try
            {
                var existentes = await _dataService.GetEstudiantesByComisionIdAsync(comision.Id);
                if (comision != ComisionSeleccionada) return;

                MostrarResultado(ClasificadorImportacion.Clasificar(_leidos, existentes.Select(e => e.Dni)));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                Estado = "No se pudo comparar el archivo con los alumnos existentes.";
            }
        }

        /// <summary>
        /// Muestra las filas de la vista previa y el resumen con los totales.
        /// </summary>
        private void MostrarResultado(ResultadoClasificacion resultado)
        {
            foreach (var fila in resultado.Filas)
            {
                Filas.Add(CrearFila(fila));
            }

            Estado = $"{NombreArchivo}: {resultado.Nuevos} para importar, {resultado.YaExisten} ya existen, " +
                     $"{resultado.Repetidos} repetidos en el archivo, {resultado.ConErrores} con errores.";
        }

        /// <summary>
        /// Traduce una fila clasificada a la fila que muestra la pantalla.
        /// </summary>
        private static ImportacionFila CrearFila(FilaClasificada fila) => new()
        {
            Dni = fila.Dni,
            Nombre = fila.Nombre,
            Apellido = fila.Apellido,
            Email = fila.Email,
            Estado = PresentacionEstadoFila.Texto(fila.Estado),
            Color = PresentacionEstadoFila.Color(fila.Estado),
            SeImporta = fila.SeImporta
        };

        /// <summary>
        /// Guarda los alumnos nuevos todos juntos. Si ya se está importando, no hace nada.
        /// </summary>
        private async Task ImportarAsync()
        {
            if (_importando) return;

            var comision = ComisionSeleccionada;
            var nuevos = Filas.Where(f => f.SeImporta).ToList();
            var motivo = ObtenerMotivoParaNoImportar(comision, nuevos.Count);

            if (motivo == null)
                await GuardarConBloqueoAsync(comision!, nuevos);
            else
                await _dialogo.AvisarAsync("Atención", motivo);
        }

        /// <summary>
        /// Devuelve por qué no se puede importar (sin comisión o sin alumnos nuevos), o null si se puede.
        /// </summary>
        private static string? ObtenerMotivoParaNoImportar(Comision? comision, int cantidadNuevos)
        {
            if (comision == null)
                return "Elegí una comisión antes de importar.";

            return cantidadNuevos == 0 ? "No hay alumnos nuevos para importar." : null;
        }

        /// <summary>
        /// Marca que hay una importación en curso mientras guarda, y la desmarca al terminar, salga como salga.
        /// </summary>
        private async Task GuardarConBloqueoAsync(Comision comision, List<ImportacionFila> nuevos)
        {
            _importando = true;
            try
            {
                await GuardarYAvisarAsync(comision, nuevos);
            }
            finally
            {
                _importando = false;
            }
        }

        /// <summary>
        /// Guarda los alumnos y, si salió bien, limpia la pantalla y avisa cuántos se importaron.
        /// </summary>
        private async Task GuardarYAvisarAsync(Comision comision, List<ImportacionFila> nuevos)
        {
            if (!await IntentarGuardarAsync(comision, nuevos)) return;

            var mensaje = $"Se importaron {nuevos.Count} alumnos a la comisión {comision.Nombre}.";
            ReiniciarImportacion(mensaje);
            await _dialogo.AvisarAsync("Éxito", mensaje);
        }

        /// <summary>
        /// Guarda todo junto: si algo falla, no queda ningún alumno a medias, y devuelve false.
        /// </summary>
        private async Task<bool> IntentarGuardarAsync(Comision comision, List<ImportacionFila> nuevos)
        {
            try
            {
                var alumnos = nuevos
                    .Select(f => new Estudiante(0, f.Dni, f.Nombre, f.Apellido, f.Email, comision.Id))
                    .ToList();

                await _dataService.SaveEstudiantesMassiveAsync(alumnos);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                await _dialogo.AvisarAsync("Error", "No se pudo importar el archivo. No se guardó ningún alumno.");
                return false;
            }
        }

        /// <summary>
        /// Deja la pantalla como al principio, con el mensaje indicado como resumen.
        /// </summary>
        private void ReiniciarImportacion(string mensaje)
        {
            _leidos = new List<Estudiante>();
            NombreArchivo = string.Empty;
            Filas.Clear();
            Estado = mensaje;
        }
    }
}
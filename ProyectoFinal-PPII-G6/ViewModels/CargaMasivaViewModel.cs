using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.ViewModels
{
    /// <summary>
    /// Una fila de la vista previa: un alumno leído del archivo y qué se va a hacer con él.
    /// </summary>
    public class ImportacionFila
    {
        public string Dni { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string NombreCompleto =>
            string.IsNullOrWhiteSpace($"{Nombre} {Apellido}") ? "(sin nombre)" : $"{Nombre} {Apellido}".Trim();

        public string Detalle =>
            string.IsNullOrWhiteSpace(Email) ? $"DNI: {Dni}" : $"DNI: {Dni}  |  {Email}";

        public string Estado { get; set; } = string.Empty;
        public string Color { get; set; } = "#757575";

        /// <summary>
        /// True solo para las filas nuevas y válidas: son las únicas que se guardan.
        /// </summary>
        public bool SeImporta { get; set; }
    }

    /// <summary>
    /// ViewModel de la carga masiva: lee un CSV de alumnos, muestra una vista previa y los importa a una comisión.
    /// </summary>
    public class CargaMasivaViewModel : BindableObject
    {
        private readonly IDataService _dataService;
        private readonly CsvImporterService _csvImporter;

        private List<Estudiante> _leidos = new();
        private bool _importando;

        public ObservableCollection<Comision> Comisiones { get; } = new();
        public ObservableCollection<ImportacionFila> Filas { get; } = new();

        private Comision? _comisionSeleccionada;

        public Comision? ComisionSeleccionada
        {
            get => _comisionSeleccionada;
            set
            {
                _comisionSeleccionada = value;
                OnPropertyChanged();

                // Si ya había un archivo leído, se vuelve a evaluar contra la comisión nueva
                _ = EvaluarAsync();
            }
        }

        private string _nombreArchivo = string.Empty;

        public string NombreArchivo
        {
            get => _nombreArchivo;
            set { _nombreArchivo = value; OnPropertyChanged(); }
        }

        private string _estado = "Elegí una comisión y un archivo CSV.";

        public string Estado
        {
            get => _estado;
            set { _estado = value; OnPropertyChanged(); }
        }

        public Command ElegirArchivoCommand { get; }
        public Command ImportarCommand { get; }

        public CargaMasivaViewModel(IDataService dataService, CsvImporterService csvImporter)
        {
            _dataService = dataService;
            _csvImporter = csvImporter;

            ElegirArchivoCommand = new Command(async () => await ElegirArchivoAsync());
            ImportarCommand = new Command(async () => await ImportarAsync());
        }

        private static async Task AvisarAsync(string titulo, string mensaje)
        {
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert(titulo, mensaje, "OK");
        }

        /// <summary>
        /// Quita puntos y espacios para que "30.123.456" y "30123456" cuenten como el mismo DNI.
        /// </summary>
        private static string NormalizarDni(string? dni) =>
            (dni ?? string.Empty).Replace(".", string.Empty).Replace(" ", string.Empty).Trim();

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
                foreach (var c in lista.OrderBy(c => c.Nombre))
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

        private async Task ElegirArchivoAsync()
        {
            try
            {
                var tipoCsv = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.WinUI, new[] { ".csv" } },
                        { DevicePlatform.Android, new[] { "text/csv", "text/comma-separated-values" } },
                        { DevicePlatform.iOS, new[] { "public.comma-separated-values-text" } },
                        { DevicePlatform.MacCatalyst, new[] { "csv" } }
                    });

                var archivo = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Seleccione el archivo CSV de estudiantes",
                    FileTypes = tipoCsv
                });

                // Si el usuario cancela el selector, no pasa nada
                if (archivo == null) return;

                using var stream = await archivo.OpenReadAsync();
                _leidos = await _csvImporter.ParseCsvAsync(stream, ComisionSeleccionada?.Id ?? 0);
                NombreArchivo = archivo.FileName;

                await EvaluarAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                await AvisarAsync("Error", "No se pudo leer el archivo.");
            }
        }

        /// <summary>
        /// Arma la vista previa: marca cada fila como nueva, ya existente, repetida o con errores.
        /// </summary>
        private async Task EvaluarAsync()
        {
            Filas.Clear();

            if (_leidos.Count == 0)
            {
                Estado = string.IsNullOrEmpty(NombreArchivo)
                    ? "Elegí una comisión y un archivo CSV."
                    : "No se encontraron alumnos en el archivo. Formato: Dni;Nombre;Apellido;Email, con encabezado.";
                return;
            }

            var comision = ComisionSeleccionada;
            if (comision == null)
            {
                Estado = "Elegí una comisión para ver qué se puede importar.";
                return;
            }

            try
            {
                var existentes = await _dataService.GetEstudiantesByComisionIdAsync(comision.Id);

                // Si mientras consultaba cambió la comisión, este resultado ya no sirve
                if (comision != ComisionSeleccionada) return;

                var dnisExistentes = existentes.Select(e => NormalizarDni(e.Dni)).ToHashSet();
                var dnisVistos = new HashSet<string>();

                int nuevos = 0, yaExisten = 0, repetidos = 0, conErrores = 0;
                var filas = new List<ImportacionFila>();

                foreach (var alumno in _leidos)
                {
                    var fila = new ImportacionFila
                    {
                        Dni = NormalizarDni(alumno.Dni),
                        Nombre = (alumno.Nombre ?? string.Empty).Trim(),
                        Apellido = (alumno.Apellido ?? string.Empty).Trim(),
                        Email = (alumno.Email ?? string.Empty).Trim()
                    };

                    if (fila.Dni.Length == 0 || fila.Nombre.Length == 0
                        || fila.Apellido.Length == 0 || fila.Email.Length == 0)
                    {
                        conErrores++;
                        fila.Estado = "Datos incompletos";
                        fila.Color = "#D32F2F";
                    }
                    else if (!ValidacionEmail.EsValido(fila.Email))
                    {
                        conErrores++;
                        fila.Estado = "Email inválido";
                        fila.Color = "#D32F2F";
                    }
                    else if (fila.Dni.Length > 20 || fila.Nombre.Length > 100 || fila.Apellido.Length > 100)
                    {
                        conErrores++;
                        fila.Estado = "Dato demasiado largo";
                        fila.Color = "#D32F2F";
                    }
                    else if (dnisExistentes.Contains(fila.Dni))
                    {
                        yaExisten++;
                        fila.Estado = "Ya existe en la comisión";
                        fila.Color = "#757575";
                    }
                    else if (!dnisVistos.Add(fila.Dni))
                    {
                        repetidos++;
                        fila.Estado = "DNI repetido en el archivo";
                        fila.Color = "#F57C00";
                    }
                    else
                    {
                        nuevos++;
                        fila.SeImporta = true;
                        fila.Estado = "Nuevo";
                        fila.Color = "#388E3C";
                    }

                    filas.Add(fila);
                }

                foreach (var fila in filas)
                {
                    Filas.Add(fila);
                }

                Estado = $"{NombreArchivo}: {nuevos} para importar, {yaExisten} ya existen, " +
                         $"{repetidos} repetidos en el archivo, {conErrores} con errores.";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                Estado = "No se pudo comparar el archivo con los alumnos existentes.";
            }
        }

        private async Task ImportarAsync()
        {
            if (_importando) return;

            var comision = ComisionSeleccionada;
            if (comision == null)
            {
                await AvisarAsync("Atención", "Elegí una comisión antes de importar.");
                return;
            }

            var nuevos = Filas.Where(f => f.SeImporta).ToList();
            if (nuevos.Count == 0)
            {
                await AvisarAsync("Atención", "No hay alumnos nuevos para importar.");
                return;
            }

            _importando = true;
            try
            {
                try
                {
                    var alumnos = nuevos
                        .Select(f => new Estudiante(0, f.Dni, f.Nombre, f.Apellido, f.Email, comision.Id))
                        .ToList();

                    // Se guarda todo junto: si algo falla, no queda ningún alumno a medias
                    await _dataService.SaveEstudiantesMassiveAsync(alumnos);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                    await AvisarAsync("Error", "No se pudo importar el archivo. No se guardó ningún alumno.");
                    return;
                }

                var mensaje = $"Se importaron {nuevos.Count} alumnos a la comisión {comision.Nombre}.";

                _leidos = new List<Estudiante>();
                NombreArchivo = string.Empty;
                Filas.Clear();
                Estado = mensaje;

                await AvisarAsync("Éxito", mensaje);
            }
            finally
            {
                _importando = false;
            }
        }
    }
}
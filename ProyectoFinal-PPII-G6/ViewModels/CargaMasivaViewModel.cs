using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.ViewModels
{
    public class CargaMasivaViewModel : BindableObject
    {
        private readonly CsvImporterService _csvImporterService;
        private readonly IDataService _dataService;

        private string _mensajeEstado = "Seleccione un archivo CSV para importar alumnos.";
        public string MensajeEstado
        {
            get => _mensajeEstado;
            set { _mensajeEstado = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public Command SeleccionarArchivoCommand { get; }

        public CargaMasivaViewModel(CsvImporterService csvImporterService, IDataService dataService)
        {
            _csvImporterService = csvImporterService;
            _dataService = dataService;
            SeleccionarArchivoCommand = new Command(async () => await SeleccionarYProcesarArchivoAsync());
        }

        private async Task SeleccionarYProcesarArchivoAsync()
        {
            try
            {
                var customFileType = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.WinUI, new[] { ".csv" } },
                        { DevicePlatform.Android, new[] { "text/csv", "text/comma-separated-values" } },
                        { DevicePlatform.iOS, new[] { "public.comma-separated-values-text" } },
                        { DevicePlatform.MacCatalyst, new[] { "csv" } }
                    });

                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Seleccione el archivo CSV de estudiantes",
                    FileTypes = customFileType
                });

                if (result != null)
                {
                    IsBusy = true;
                    MensajeEstado = $"Procesando {result.FileName}...";

                    using var stream = await result.OpenReadAsync();
                    var estudiantesImportados = await _csvImporterService.ParseCsvAsync(stream, 1);

                    int guardados = 0;
                    foreach (var est in estudiantesImportados)
                    {
                        await _dataService.SaveEstudianteAsync(est);
                        guardados++;
                    }

                    MensajeEstado = $"¡Éxito! Se importaron {guardados} estudiantes correctamente.";

                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Importación Completa", $"Se registraron {guardados} alumnos desde el CSV.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                MensajeEstado = "Error al procesar el archivo CSV.";
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
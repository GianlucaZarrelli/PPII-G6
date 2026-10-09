using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.ViewModels
{
    /// <summary>
    /// Una fila de la pantalla: un alumno con su estado de asistencia en la fecha elegida.
    /// </summary>
    public class AsistenciaFila : BindableObject
    {
        public int EstudianteId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Id del registro existente; 0 si el alumno todavía no tiene asistencia en esa clase.
        /// </summary>
        public int AsistenciaId { get; set; }

        private bool _presente = true;

        public bool Presente
        {
            get => _presente;
            set
            {
                if (_presente == value) return;
                _presente = value;

                // Una ausencia justificada solo tiene sentido si el alumno está ausente
                if (value) _justificada = false;

                OnPropertyChanged();
                OnPropertyChanged(nameof(Ausente));
                OnPropertyChanged(nameof(Justificada));
            }
        }

        private bool _justificada;

        public bool Justificada
        {
            get => _justificada;
            set { _justificada = value; OnPropertyChanged(); }
        }

        public bool Ausente => !Presente;
    }

    /// <summary>
    /// ViewModel para tomar o corregir la asistencia de una comisión en una fecha.
    /// </summary>
    public class AsistenciaFechaViewModel : BindableObject
    {
        private readonly IDataService _dataService;

        public ObservableCollection<Comision> Comisiones { get; } = new();
        public ObservableCollection<AsistenciaFila> Filas { get; } = new();

        private Comision? _comisionSeleccionada;

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

        public string Estado
        {
            get => _estado;
            set { _estado = value; OnPropertyChanged(); }
        }

        public Command GuardarCommand { get; }

        public AsistenciaFechaViewModel(IDataService dataService)
        {
            _dataService = dataService;
            GuardarCommand = new Command(async () => await GuardarAsync());
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

        private async Task CargarFilasAsync()
        {
            Filas.Clear();

            if (ComisionSeleccionada == null)
            {
                Estado = "Seleccione una comisión.";
                return;
            }

            var comision = ComisionSeleccionada;
            var fecha = Fecha.Date;

            try
            {
                var estudiantes = await _dataService.GetEstudiantesByComisionIdAsync(comision.Id);
                var clases = await _dataService.GetClasesByComisionIdAsync(comision.Id);
                var clase = clases.FirstOrDefault(c => c.Fecha.Date == fecha);

                var asistencias = new System.Collections.Generic.List<Asistencia>();
                if (clase != null)
                {
                    var todas = await _dataService.GetTodasLasAsistenciasAsync();
                    asistencias = todas.Where(a => a.ClaseId == clase.Id).ToList();
                }

                // Si mientras consultaba cambió la comisión o la fecha, este resultado ya no sirve
                if (comision != ComisionSeleccionada || fecha != Fecha.Date) return;

                Filas.Clear();
                foreach (var est in estudiantes.OrderBy(e => e.Apellido).ThenBy(e => e.Nombre))
                {
                    var asis = asistencias.FirstOrDefault(a => a.EstudianteId == est.Id);

                    Filas.Add(new AsistenciaFila
                    {
                        EstudianteId = est.Id,
                        NombreCompleto = $"{est.Apellido}, {est.Nombre}",
                        AsistenciaId = asis?.Id ?? 0,
                        Presente = asis?.Presente ?? true,
                        Justificada = asis?.Justificada ?? false
                    });
                }

                if (Filas.Count == 0)
                    Estado = "La comisión no tiene alumnos cargados.";
                else if (clase == null)
                    Estado = $"No hay clase registrada el {fecha:dd/MM/yyyy}. Al guardar se crea.";
                else
                    Estado = $"Clase del {fecha:dd/MM/yyyy}: {Filas.Count} alumnos.";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                Estado = "No se pudo cargar la asistencia.";
            }
        }

        private async Task GuardarAsync()
        {
            if (ComisionSeleccionada == null || Filas.Count == 0)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Atención", "Elegí una comisión con alumnos antes de guardar.", "OK");
                return;
            }

            var fecha = Fecha.Date;

            // Una clase futura inflaría el total de clases y marcaría ausentes a todos
            if (fecha > DateTime.Today)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Atención", "No se puede registrar asistencia de una fecha futura.", "OK");
                return;
            }

            try
            {
                var clases = await _dataService.GetClasesByComisionIdAsync(ComisionSeleccionada.Id);
                var clase = clases.FirstOrDefault(c => c.Fecha.Date == fecha);

                int claseId;
                if (clase != null)
                {
                    claseId = clase.Id;
                }
                else
                {
                    var nueva = new Clase(0, fecha, $"Clase {fecha:dd/MM/yyyy}", ComisionSeleccionada.Id);
                    claseId = await _dataService.SaveClaseAsync(nueva);
                }

                var asistencias = Filas
                    .Select(f => new Asistencia(f.AsistenciaId, f.EstudianteId, claseId, f.Presente, !f.Presente && f.Justificada))
                    .ToList();

                await _dataService.SaveAsistenciasAsync(asistencias);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Error", "No se pudo guardar la asistencia.", "OK");
                return;
            }

            // Recargo para que las filas nuevas tomen su Id y un segundo guardado actualice en vez de duplicar
            await CargarFilasAsync();

            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Éxito", "Asistencia guardada.", "OK");
        }
    }
}
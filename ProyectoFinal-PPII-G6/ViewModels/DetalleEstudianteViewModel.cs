using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;
using System.Linq;

namespace ProyectoFinal_PPII_G6.ViewModels
{
    /// <summary>
    /// ViewModel de la pantalla de asistencia: registra asistencias y muestra el historial y el nivel de riesgo.
    /// </summary>
    public class DetalleEstudianteViewModel : BindableObject
    {
        /// <summary>
        /// Servicio de acceso a datos.
        /// </summary>
        private readonly IDataService _dataService;

        /// <summary>
        /// Servicio que calcula el nivel de riesgo.
        /// </summary>
        private readonly ICalculadorRiesgoService _calculadorRiesgoService;

        /// <summary>
        /// Estudiantes disponibles para seleccionar.
        /// </summary>
        public ObservableCollection<Estudiante> ListaEstudiantes { get; set; }

        /// <summary>
        /// Historial de asistencias del estudiante seleccionado.
        /// </summary>
        public ObservableCollection<Asistencia> HistorialAsistencias { get; set; }

        private Estudiante? _estudianteSeleccionado;

        /// <summary>
        /// Estudiante seleccionado. Al cambiarlo se carga su historial y su nivel de riesgo.
        /// </summary>
        public Estudiante? EstudianteSeleccionado
        {
            get => _estudianteSeleccionado;
            set
            {
                _estudianteSeleccionado = value;
                OnPropertyChanged();
                if (value != null)
                {
                    _ = CargarDetalleAsync(value.Id);
                }
            }
        }

        private string _nivelRiesgoTexto = "Seleccione un alumno";

        /// <summary>
        /// Texto del nivel de riesgo que se muestra en pantalla.
        /// </summary>
        public string NivelRiesgoTexto
        {
            get => _nivelRiesgoTexto;
            set { _nivelRiesgoTexto = value; OnPropertyChanged(); }
        }

        private string _colorRiesgo = "#757575";

        /// <summary>
        /// Color del indicador de riesgo.
        /// </summary>
        public string ColorRiesgo
        {
            get => _colorRiesgo;
            set { _colorRiesgo = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Comando para registrar al estudiante como presente.
        /// </summary>
        public Command RegistrarAsistenciaCommand { get; }

        /// <summary>
        /// Comando para registrar al estudiante como ausente.
        /// </summary>
        public Command RegistrarInasistenciaCommand { get; }

        /// <summary>
        /// Crea el ViewModel con los servicios que necesita e inicializa los comandos.
        /// </summary>
        public DetalleEstudianteViewModel(IDataService dataService, ICalculadorRiesgoService calculadorRiesgoService)
        {
            _dataService = dataService;
            _calculadorRiesgoService = calculadorRiesgoService;
            ListaEstudiantes = new ObservableCollection<Estudiante>();
            HistorialAsistencias = new ObservableCollection<Asistencia>();

            RegistrarAsistenciaCommand = new Command(async () => await RegistrarAsistenciaAsync(true));
            RegistrarInasistenciaCommand = new Command(async () => await RegistrarAsistenciaAsync(false));
        }

        /// <summary>
        /// Carga la lista de estudiantes para el selector.
        /// </summary>
        public async Task CargarListaEstudiantesAsync()
        {
            var estudiantes = await _dataService.GetEstudiantesAsync();
            ListaEstudiantes.Clear();
            foreach (var est in estudiantes)
            {
                ListaEstudiantes.Add(est);
            }
        }

        /// <summary>
        /// Carga el historial de asistencias de un estudiante y actualiza su nivel de riesgo.
        /// </summary>
        public async Task CargarDetalleAsync(int estudianteId)
        {
            var asistencias = await _dataService.GetAsistenciasPorEstudianteAsync(estudianteId);
            HistorialAsistencias.Clear();

            int inasistencias = 0;
            foreach (var asis in asistencias)
            {
                HistorialAsistencias.Add(asis);
                if (!asis.Presente) inasistencias++;
            }

            CalcularNivelRiesgo(asistencias.Count, inasistencias);
        }

        /// <summary>
        /// Registra la asistencia del día del estudiante seleccionado. Si ya estaba registrada, la actualiza.
        /// </summary>
        private async Task RegistrarAsistenciaAsync(bool presente)
        {
            if (EstudianteSeleccionado == null)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Atención", "Seleccione un estudiante antes de registrar asistencia.", "OK");
                return;
            }

            var fechaHoy = DateTime.Today;
            var clases = await _dataService.GetClasesAsync();
            var claseHoy = clases.FirstOrDefault(c => c.Fecha.Date == fechaHoy);

            int claseId;
            if (claseHoy != null)
            {
                claseId = claseHoy.Id;
            }
            else
            {
                var nuevaClase = new Clase(0, fechaHoy, $"Clase {fechaHoy:dd/MM/yyyy}");
                claseId = await _dataService.SaveClaseAsync(nuevaClase);
            }

            // Buscar si ya existe una asistencia registrada para este alumno en la clase de hoy
            var asistencias = await _dataService.GetAsistenciasPorEstudianteAsync(EstudianteSeleccionado.Id);
            var asistenciaExistente = asistencias.FirstOrDefault(a => a.ClaseId == claseId);

            if (asistenciaExistente != null)
            {
                // Actualizar el registro existente (Evita duplicados)
                asistenciaExistente.Presente = presente;
                await _dataService.SaveAsistenciaAsync(asistenciaExistente);
            }
            else
            {
                // Crear un nuevo registro si es la primera marca del día
                var nuevaAsistencia = new Asistencia(0, EstudianteSeleccionado.Id, claseId, presente);
                await _dataService.SaveAsistenciaAsync(nuevaAsistencia);
            }

            await CargarDetalleAsync(EstudianteSeleccionado.Id);
        }

        /// <summary>
        /// Actualiza el texto y el color del indicador según el nivel de riesgo.
        /// </summary>
        private void CalcularNivelRiesgo(int totalClases, int inasistencias)
        {
            if (totalClases == 0)
            {
                NivelRiesgoTexto = "Sin registros de asistencia";
                ColorRiesgo = "#757575";
                return;
            }

            switch (_calculadorRiesgoService.CalcularNivelRiesgo(inasistencias))
            {
                case NivelRiesgo.Alto:
                    NivelRiesgoTexto = "ALTO RIESGO (3+ Inasistencias)";
                    ColorRiesgo = "#D32F2F";
                    break;
                case NivelRiesgo.Moderado:
                    NivelRiesgoTexto = "RIESGO MODERADO";
                    ColorRiesgo = "#F57C00";
                    break;
                default:
                    NivelRiesgoTexto = "BAJO RIESGO / AL DÍA";
                    ColorRiesgo = "#388E3C";
                    break;
            }
        }
    }
}

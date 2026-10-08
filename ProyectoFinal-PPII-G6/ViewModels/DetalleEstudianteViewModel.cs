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
    /// ViewModel de la pantalla de asistencia: registra asistencias y muestra el historial y el nivel de riesgo.
    /// </summary>
    public class DetalleEstudianteViewModel : BindableObject
    {
        private readonly IDataService _dataService;
        private readonly RiesgoService _calculadorRiesgoService;

        public ObservableCollection<Estudiante> ListaEstudiantes { get; set; }
        public ObservableCollection<Asistencia> HistorialAsistencias { get; set; }

        private Estudiante? _estudianteSeleccionado;

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

        public string NivelRiesgoTexto
        {
            get => _nivelRiesgoTexto;
            set { _nivelRiesgoTexto = value; OnPropertyChanged(); }
        }

        private string _colorRiesgo = "#757575";

        public string ColorRiesgo
        {
            get => _colorRiesgo;
            set { _colorRiesgo = value; OnPropertyChanged(); }
        }

        public Command RegistrarAsistenciaCommand { get; }
        public Command RegistrarInasistenciaCommand { get; }

        public DetalleEstudianteViewModel(IDataService dataService, RiesgoService calculadorRiesgoService)
        {
            _dataService = dataService;
            _calculadorRiesgoService = calculadorRiesgoService;
            ListaEstudiantes = new ObservableCollection<Estudiante>();
            HistorialAsistencias = new ObservableCollection<Asistencia>();

            RegistrarAsistenciaCommand = new Command(async () => await RegistrarAsistenciaAsync(true));
            RegistrarInasistenciaCommand = new Command(async () => await RegistrarAsistenciaAsync(false));
        }

        public async Task CargarListaEstudiantesAsync()
        {
            var estudiantes = await _dataService.GetEstudiantesAsync();
            ListaEstudiantes.Clear();
            foreach (var est in estudiantes)
            {
                ListaEstudiantes.Add(est);
            }
        }

        public async Task CargarDetalleAsync(int estudianteId)
        {
            var asistencias = await _dataService.GetAsistenciasPorEstudianteAsync(estudianteId);
            var clases = await _dataService.GetClasesAsync();

            HistorialAsistencias.Clear();
            foreach (var asis in asistencias)
            {
                HistorialAsistencias.Add(asis);
            }

            CalcularNivelRiesgo(asistencias, clases.Count);
        }

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

            var asistencias = await _dataService.GetAsistenciasPorEstudianteAsync(EstudianteSeleccionado.Id);
            var asistenciaExistente = asistencias.FirstOrDefault(a => a.ClaseId == claseId);

            if (asistenciaExistente != null)
            {
                asistenciaExistente.Presente = presente;
                await _dataService.SaveAsistenciaAsync(asistenciaExistente);
            }
            else
            {
                var nuevaAsistencia = new Asistencia(0, EstudianteSeleccionado.Id, claseId, presente, false);
                await _dataService.SaveAsistenciaAsync(nuevaAsistencia);
            }

            await CargarDetalleAsync(EstudianteSeleccionado.Id);
        }

        private void CalcularNivelRiesgo(List<Asistencia> asistencias, int totalClases)
        {
            if (totalClases == 0)
            {
                NivelRiesgoTexto = "Sin registros de asistencia";
                ColorRiesgo = "#757575";
                return;
            }

            var resultado = _calculadorRiesgoService.CalcularRiesgo(
                asistencias,
                totalClases,
                new List<Entrega>(),
                0
            );

            switch (resultado.Nivel)
            {
                case NivelRiesgoEnum.Rojo:
                    NivelRiesgoTexto = $"ALTO RIESGO ({resultado.PorcentajeAsistencia:F0}% Asist.)";
                    ColorRiesgo = "#D32F2F";
                    break;
                case NivelRiesgoEnum.Amarillo:
                    NivelRiesgoTexto = $"RIESGO MODERADO ({resultado.PorcentajeAsistencia:F0}% Asist.)";
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
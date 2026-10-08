using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.ViewModels
{
    /// <summary>
    /// ViewModel del menú principal. Muestra el total de alumnos y cuántos están en riesgo alto.
    /// </summary>
    public class HomeViewModel : BindableObject
    {
        private readonly IDataService _dataService;
        private readonly RiesgoService _calculadorRiesgoService;

        private int _totalEstudiantes;

        public int TotalEstudiantes
        {
            get => _totalEstudiantes;
            set { _totalEstudiantes = value; OnPropertyChanged(); }
        }

        private int _estudiantesEnRiesgo;

        public int EstudiantesEnRiesgo
        {
            get => _estudiantesEnRiesgo;
            set { _estudiantesEnRiesgo = value; OnPropertyChanged(); }
        }

        public HomeViewModel(IDataService dataService, RiesgoService calculadorRiesgoService)
        {
            _dataService = dataService;
            _calculadorRiesgoService = calculadorRiesgoService;
        }

        /// <summary>
        /// Calcula el total de estudiantes y cuántos están en riesgo alto.
        /// </summary>
        public async Task CargarMetricasAsync()
        {
            var estudiantes = await _dataService.GetEstudiantesAsync();
            var todasLasAsistencias = await _dataService.GetTodasLasAsistenciasAsync();
            var clases = await _dataService.GetClasesAsync();

            TotalEstudiantes = estudiantes.Count;

            int enRiesgoCount = 0;
            int totalClases = clases.Count;

            foreach (var est in estudiantes)
            {
                var asistenciasEstudiante = todasLasAsistencias
                    .Where(a => a.EstudianteId == est.Id)
                    .ToList();

                // Evaluamos el riesgo pasando la lista de asistencias y el total de clases
                var resultado = _calculadorRiesgoService.CalcularRiesgo(
                    asistenciasEstudiante,
                    totalClases,
                    new List<Entrega>(),
                    0
                );

                if (resultado.Nivel == NivelRiesgoEnum.Rojo)
                {
                    enRiesgoCount++;
                }
            }

            EstudiantesEnRiesgo = enRiesgoCount;
        }
    }
}
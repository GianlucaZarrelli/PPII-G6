using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.ViewModels
{
    /// <summary>
    /// ViewModel del menú principal. Muestra el total de alumnos y cuántos están en riesgo alto.
    /// </summary>
    public class HomeViewModel : BindableObject
    {
        /// <summary>
        /// Servicio de acceso a datos.
        /// </summary>
        private readonly IDataService _dataService;

        /// <summary>
        /// Servicio que calcula el nivel de riesgo.
        /// </summary>
        private readonly ICalculadorRiesgoService _calculadorRiesgoService;

        private int _totalEstudiantes;

        /// <summary>
        /// Cantidad total de estudiantes.
        /// </summary>
        public int TotalEstudiantes
        {
            get => _totalEstudiantes;
            set { _totalEstudiantes = value; OnPropertyChanged(); }
        }

        private int _estudiantesEnRiesgo;

        /// <summary>
        /// Cantidad de estudiantes en riesgo alto.
        /// </summary>
        public int EstudiantesEnRiesgo
        {
            get => _estudiantesEnRiesgo;
            set { _estudiantesEnRiesgo = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Crea el ViewModel con los servicios que necesita.
        /// </summary>
        public HomeViewModel(IDataService dataService, ICalculadorRiesgoService calculadorRiesgoService)
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

            TotalEstudiantes = estudiantes.Count;

            int enRiesgoCount = 0;
            foreach (var est in estudiantes)
            {
                int ausencias = todasLasAsistencias
                    .Where(a => a.EstudianteId == est.Id && !a.Presente)
                    .Count();

                if (_calculadorRiesgoService.CalcularNivelRiesgo(ausencias) == NivelRiesgo.Alto)
                {
                    enRiesgoCount++;
                }
            }

            EstudiantesEnRiesgo = enRiesgoCount;
        }
    }
}

using System;
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
        private readonly ConsultaSeguimiento _consulta;

        private int _totalEstudiantes;

        /// <summary>
        /// Total de alumnos de todas las comisiones.
        /// </summary>
        public int TotalEstudiantes
        {
            get => _totalEstudiantes;
            set { _totalEstudiantes = value; OnPropertyChanged(); }
        }

        private int _estudiantesEnRiesgo;

        /// <summary>
        /// Cantidad de alumnos en riesgo alto (nivel rojo).
        /// </summary>
        public int EstudiantesEnRiesgo
        {
            get => _estudiantesEnRiesgo;
            set { _estudiantesEnRiesgo = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Crea el ViewModel con la consulta que evalúa a los alumnos.
        /// </summary>
        public HomeViewModel(ConsultaSeguimiento consulta)
        {
            _consulta = consulta;
        }

        /// <summary>
        /// Evalúa a todos los alumnos con el mismo criterio que el panel y la ficha, y cuenta los que están en rojo.
        /// </summary>
        public async Task CargarMetricasAsync()
        {
            try
            {
                var resumenes = await _consulta.ObtenerTodasAsync();

                TotalEstudiantes = resumenes.Count;
                EstudiantesEnRiesgo = resumenes.Count(r => r.Nivel == NivelSeguimiento.Rojo);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
            }
        }
    }
}
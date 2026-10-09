using System;
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
        private readonly RiesgoService _riesgoService;

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

        public HomeViewModel(IDataService dataService, RiesgoService riesgoService)
        {
            _dataService = dataService;
            _riesgoService = riesgoService;
        }

        /// <summary>
        /// Calcula el total de estudiantes y cuántos están en riesgo alto,
        /// con el mismo criterio que la ficha del alumno (asistencia + entregas, por comisión).
        /// </summary>
        public async Task CargarMetricasAsync()
        {
            try
            {
                var estudiantes = await _dataService.GetEstudiantesAsync();
                var todasLasAsistencias = await _dataService.GetTodasLasAsistenciasAsync();
                var clases = await _dataService.GetClasesAsync();
                var comisiones = await _dataService.GetComisionesAsync();

                var tpsPorComision = new Dictionary<int, List<TrabajoPractico>>();
                foreach (var c in comisiones)
                {
                    tpsPorComision[c.Id] = await _dataService.GetTrabajosPracticosByComisionIdAsync(c.Id);
                }

                int enRiesgoCount = 0;

                foreach (var est in estudiantes)
                {
                    var idsClases = clases.Where(c => c.ComisionId == est.ComisionId).Select(c => c.Id).ToHashSet();
                    var tps = tpsPorComision.GetValueOrDefault(est.ComisionId) ?? new List<TrabajoPractico>();
                    var idsTps = tps.Select(t => t.Id).ToHashSet();

                    var asistencias = todasLasAsistencias
                        .Where(a => a.EstudianteId == est.Id && idsClases.Contains(a.ClaseId))
                        .ToList();

                    var entregasAlumno = await _dataService.GetEntregasPorEstudianteAsync(est.Id);
                    var entregas = entregasAlumno.Where(e => idsTps.Contains(e.TrabajoPracticoId)).ToList();

                    var resultado = _riesgoService.CalcularRiesgo(asistencias, idsClases.Count, entregas, tps.Count);

                    if (resultado.Nivel == NivelRiesgoEnum.Rojo)
                    {
                        enRiesgoCount++;
                    }
                }

                TotalEstudiantes = estudiantes.Count;
                EstudiantesEnRiesgo = enRiesgoCount;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
            }
        }
    }
}
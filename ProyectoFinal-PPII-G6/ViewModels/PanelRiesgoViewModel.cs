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
    /// Una fila del panel: un alumno con su nivel de riesgo y el dato que lo justifica.
    /// </summary>
    public class AlumnoRiesgoItem
    {
        /// <summary>
        /// Identificador del estudiante.
        /// </summary>
        public int EstudianteId { get; set; }
        /// <summary>
        /// Nombre y apellido del estudiante.
        /// </summary>
        public string NombreCompleto { get; set; } = string.Empty;
        /// <summary>
        /// Nombre de la comisión del estudiante.
        /// </summary>
        public string Comision { get; set; } = string.Empty;
        /// <summary>
        /// Texto del nivel de riesgo que se muestra (por ejemplo "ALTO RIESGO").
        /// </summary>
        public string Nivel { get; set; } = string.Empty;
        /// <summary>
        /// Color hexadecimal asociado al nivel de riesgo.
        /// </summary>
        public string ColorNivel { get; set; } = "#757575";
        /// <summary>
        /// Resumen de asistencia y entregas que justifica el nivel.
        /// </summary>
        public string Detalle { get; set; } = string.Empty;

        /// <summary>
        /// Orden de prioridad: 0 = rojo, 1 = amarillo, 2 = verde, 3 = sin datos.
        /// </summary>
        public int Orden { get; set; }

        /// <summary>
        /// El menor de los dos porcentajes; desempata dentro del mismo nivel (el peor primero).
        /// </summary>
        public double PeorPorcentaje { get; set; }
    }

    /// <summary>
    /// ViewModel del panel de seguimiento: lista a los alumnos de una comisión ordenados por riesgo.
    /// </summary>
    public class PanelRiesgoViewModel : BindableObject
    {
        private readonly IDataService _dataService;
        private readonly RiesgoService _riesgoService;

        /// <summary>
        /// Comisiones disponibles en el selector, ordenadas por nombre.
        /// </summary>
        public ObservableCollection<Comision> Comisiones { get; } = new();
        /// <summary>
        /// Alumnos de la comisión elegida, ordenados por riesgo.
        /// </summary>
        public ObservableCollection<AlumnoRiesgoItem> Alumnos { get; } = new();

        private Comision? _comisionSeleccionada;

        /// <summary>
        /// Comisión elegida; al cambiarla se recalcula el panel.
        /// </summary>
        public Comision? ComisionSeleccionada
        {
            get => _comisionSeleccionada;
            set
            {
                _comisionSeleccionada = value;
                OnPropertyChanged();
                _ = CargarAlumnosAsync();
            }
        }

        /// <summary>
        /// Crea el ViewModel con sus servicios.
        /// </summary>
        public PanelRiesgoViewModel(IDataService dataService, RiesgoService riesgoService)
        {
            _dataService = dataService;
            _riesgoService = riesgoService;
        }

        /// <summary>
        /// Recarga las comisiones, conserva la selección anterior (o elige la primera) y recalcula el panel.
        /// </summary>
        public async Task CargarAsync()
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
            }
        }

        /// <summary>
        /// Calcula el riesgo de cada alumno de la comisión elegida (asistencia + entregas) y arma el panel ordenado.
        /// </summary>
        private async Task CargarAlumnosAsync()
        {
            var comision = ComisionSeleccionada;

            Alumnos.Clear();
            if (comision == null) return;

            try
            {
                var estudiantes = await _dataService.GetEstudiantesByComisionIdAsync(comision.Id);
                var clases = await _dataService.GetClasesByComisionIdAsync(comision.Id);
                var tps = await _dataService.GetTrabajosPracticosByComisionIdAsync(comision.Id);
                var todasLasAsistencias = await _dataService.GetTodasLasAsistenciasAsync();

                var idsClases = clases.Select(c => c.Id).ToHashSet();
                var idsTps = tps.Select(t => t.Id).ToHashSet();

                var items = new List<AlumnoRiesgoItem>();

                foreach (var est in estudiantes)
                {
                    var asistencias = todasLasAsistencias
                        .Where(a => a.EstudianteId == est.Id && idsClases.Contains(a.ClaseId))
                        .ToList();

                    var entregasAlumno = await _dataService.GetEntregasPorEstudianteAsync(est.Id);
                    var entregas = entregasAlumno.Where(e => idsTps.Contains(e.TrabajoPracticoId)).ToList();

                    var resultado = _riesgoService.CalcularRiesgo(asistencias, idsClases.Count, entregas, tps.Count);

                    items.Add(CrearItem(
                        est,
                        comision.Nombre,
                        resultado,
                        asistencias.Count(a => a.Presente),
                        asistencias.Count(a => !a.Presente && a.Justificada),
                        idsClases.Count,
                        entregas.Count(e => e.Entregado), tps.Count));
                }

                // Si mientras consultaba el usuario eligió otra comisión, este resultado ya no sirve
                if (comision != ComisionSeleccionada) return;

                var ordenados = items
                    .OrderBy(i => i.Orden)
                    .ThenBy(i => i.PeorPorcentaje)
                    .ThenBy(i => i.NombreCompleto)
                    .ToList();

                Alumnos.Clear();
                foreach (var item in ordenados)
                {
                    Alumnos.Add(item);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
            }
        }

        /// <summary>
        /// Arma la fila del panel con el texto de detalle, el nivel, su color y el orden de prioridad.
        /// </summary>
        private static AlumnoRiesgoItem CrearItem(
            Estudiante est, string comision, ResultadoRiesgo resultado,
            int presentes, int justificadas, int totalClases, int entregados, int totalTps)
        {
            var asistenciaTexto = totalClases > 0
                ? (justificadas > 0
                    ? $"Asistencia {presentes} + {justificadas} just. de {totalClases} ({resultado.PorcentajeAsistencia:F0}%)"
                    : $"Asistencia {presentes}/{totalClases} ({resultado.PorcentajeAsistencia:F0}%)")
                : "Asistencia: sin clases";

            var entregasTexto = totalTps > 0
                ? $"Entregas {entregados}/{totalTps} ({resultado.PorcentajeEntregas:F0}%)"
                : "Entregas: sin trabajos";

            var item = new AlumnoRiesgoItem
            {
                EstudianteId = est.Id,
                NombreCompleto = est.NombreCompleto,
                Comision = comision,
                Detalle = $"{asistenciaTexto}  |  {entregasTexto}",
                PeorPorcentaje = Math.Min(resultado.PorcentajeAsistencia, resultado.PorcentajeEntregas)
            };

            // Sin clases ni trabajos no hay nada que evaluar: no es lo mismo que "al día"
            if (totalClases == 0 && totalTps == 0)
            {
                item.Orden = 3;
                item.Nivel = "SIN DATOS";
                item.ColorNivel = "#757575";
                return item;
            }

            switch (resultado.Nivel)
            {
                case NivelRiesgoEnum.Rojo:
                    item.Orden = 0;
                    item.Nivel = "ALTO RIESGO";
                    item.ColorNivel = "#D32F2F";
                    break;
                case NivelRiesgoEnum.Amarillo:
                    item.Orden = 1;
                    item.Nivel = "RIESGO MODERADO";
                    item.ColorNivel = "#F57C00";
                    break;
                default:
                    item.Orden = 2;
                    item.Nivel = "AL DÍA";
                    item.ColorNivel = "#388E3C";
                    break;
            }

            return item;
        }
    }
}
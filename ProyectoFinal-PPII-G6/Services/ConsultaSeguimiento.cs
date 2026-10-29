using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProyectoFinal_PPII_G6.Models;

namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Todo lo que la ficha de un alumno necesita mostrar: su resumen y los datos de su comisión sin filtrar.
    /// </summary>
    public class FichaAlumno
    {
        /// <summary>
        /// Resumen con el nivel y los números del alumno.
        /// </summary>
        public ResumenAlumno Resumen { get; init; } = null!;

        /// <summary>
        /// Nombre de la comisión del alumno.
        /// </summary>
        public string ComisionNombre { get; init; } = "-";

        /// <summary>
        /// Todas las clases de la comisión, de la más reciente a la más antigua.
        /// </summary>
        public IReadOnlyList<Clase> Clases { get; init; } = Array.Empty<Clase>();

        /// <summary>
        /// Todos los trabajos de la comisión.
        /// </summary>
        public IReadOnlyList<TrabajoPractico> Trabajos { get; init; } = Array.Empty<TrabajoPractico>();

        /// <summary>
        /// Asistencias registradas del alumno.
        /// </summary>
        public IReadOnlyList<Asistencia> Asistencias { get; init; } = Array.Empty<Asistencia>();

        /// <summary>
        /// Entregas registradas del alumno.
        /// </summary>
        public IReadOnlyList<Entrega> Entregas { get; init; } = Array.Empty<Entrega>();
    }

    /// <summary>
    /// Reúne los datos de la base que hacen falta para evaluar a los alumnos y le pide el cálculo a SeguimientoService.
    /// Las pantallas solo piden "la comisión" o "el alumno": no saben qué se consulta ni cómo se calcula.
    /// </summary>
    public class ConsultaSeguimiento
    {
        private readonly IDataService _dataService;
        private readonly SeguimientoService _seguimiento;

        /// <summary>
        /// Crea la consulta con el acceso a datos y el servicio que evalúa a los alumnos.
        /// </summary>
        public ConsultaSeguimiento(IDataService dataService, SeguimientoService seguimiento)
        {
            _dataService = dataService;
            _seguimiento = seguimiento;
        }

        /// <summary>
        /// Evalúa a todos los alumnos de una comisión y los devuelve ordenados para el panel.
        /// </summary>
        public async Task<IReadOnlyList<ResumenAlumno>> ObtenerComisionAsync(int comisionId)
        {
            var asistencias = await _dataService.GetTodasLasAsistenciasAsync();
            return await EvaluarComisionAsync(comisionId, asistencias);
        }

        /// <summary>
        /// Evalúa a todos los alumnos de todas las comisiones (lo usa el resumen de Inicio).
        /// </summary>
        public async Task<IReadOnlyList<ResumenAlumno>> ObtenerTodasAsync()
        {
            var comisiones = await _dataService.GetComisionesAsync();
            var asistencias = await _dataService.GetTodasLasAsistenciasAsync();

            var resumenes = new List<ResumenAlumno>();
            foreach (var comision in comisiones)
            {
                resumenes.AddRange(await EvaluarComisionAsync(comision.Id, asistencias));
            }

            return resumenes;
        }

        /// <summary>
        /// Arma la ficha de un alumno: su resumen más las clases, trabajos, asistencias y entregas para mostrar el historial.
        /// </summary>
        public async Task<FichaAlumno> ObtenerFichaAsync(Estudiante alumno)
        {
            var comision = await _dataService.GetComisionByIdAsync(alumno.ComisionId);
            var clases = await _dataService.GetClasesByComisionIdAsync(alumno.ComisionId);
            var trabajos = await _dataService.GetTrabajosPracticosByComisionIdAsync(alumno.ComisionId);
            var asistencias = await _dataService.GetAsistenciasPorEstudianteAsync(alumno.Id);
            var entregas = await _dataService.GetEntregasPorEstudianteAsync(alumno.Id);

            return new FichaAlumno
            {
                Resumen = _seguimiento.Evaluar(alumno, clases, trabajos, asistencias, entregas, DateTime.Today),
                ComisionNombre = comision?.Nombre ?? "-",
                Clases = clases,
                Trabajos = trabajos,
                Asistencias = asistencias,
                Entregas = entregas
            };
        }

        /// <summary>
        /// Evalúa a los alumnos de una comisión con las asistencias que ya se cargaron, y los ordena.
        /// </summary>
        private async Task<IReadOnlyList<ResumenAlumno>> EvaluarComisionAsync(int comisionId, List<Asistencia> asistencias)
        {
            var alumnos = await _dataService.GetEstudiantesByComisionIdAsync(comisionId);
            var clases = await _dataService.GetClasesByComisionIdAsync(comisionId);
            var trabajos = await _dataService.GetTrabajosPracticosByComisionIdAsync(comisionId);

            var resumenes = new List<ResumenAlumno>();
            foreach (var alumno in alumnos)
            {
                var entregas = await _dataService.GetEntregasPorEstudianteAsync(alumno.Id);
                var asistenciasAlumno = asistencias.Where(a => a.EstudianteId == alumno.Id);
                resumenes.Add(_seguimiento.Evaluar(alumno, clases, trabajos, asistenciasAlumno, entregas, DateTime.Today));
            }

            return _seguimiento.Ordenar(resumenes);
        }
    }
}
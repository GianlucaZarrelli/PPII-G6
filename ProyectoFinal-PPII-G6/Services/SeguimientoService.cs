using System;
using System.Collections.Generic;
using System.Linq;
using ProyectoFinal_PPII_G6.Models;

namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Nivel que se muestra para un alumno. SinDatos significa que todavía no hay clases ni trabajos que evaluar.
    /// El orden de los valores es el orden de prioridad del panel, por eso no conviene cambiarlo.
    /// </summary>
    public enum NivelSeguimiento
    {
        Rojo,
        Amarillo,
        Verde,
        SinDatos
    }

    /// <summary>
    /// Resultado de evaluar a un alumno: los números en los que se basa y el nivel que resulta.
    /// </summary>
    public class ResumenAlumno
    {
        /// <summary>
        /// Alumno evaluado.
        /// </summary>
        public Estudiante Estudiante { get; init; } = null!;

        /// <summary>
        /// Nivel resultante, incluido "sin datos".
        /// </summary>
        public NivelSeguimiento Nivel { get; init; }

        /// <summary>
        /// Cantidad de clases que le cuentan al alumno.
        /// </summary>
        public int ClasesContadas { get; init; }

        /// <summary>
        /// Clases en las que estuvo presente.
        /// </summary>
        public int Presentes { get; init; }

        /// <summary>
        /// Ausencias justificadas (valen la mitad de una presente).
        /// </summary>
        public int Justificadas { get; init; }

        /// <summary>
        /// Cantidad de trabajos que le cuentan al alumno (los que ya vencieron).
        /// </summary>
        public int TrabajosContados { get; init; }

        /// <summary>
        /// Trabajos vencidos que entregó.
        /// </summary>
        public int Entregados { get; init; }

        /// <summary>
        /// Porcentaje de asistencia, con las justificadas contadas a la mitad.
        /// </summary>
        public double PorcentajeAsistencia { get; init; }

        /// <summary>
        /// Porcentaje de trabajos vencidos que entregó.
        /// </summary>
        public double PorcentajeEntregas { get; init; }

        /// <summary>
        /// Ids de las clases que cuentan para este alumno. Las demás no se tienen en cuenta.
        /// </summary>
        public IReadOnlySet<int> IdsClasesContadas { get; init; } = new HashSet<int>();

        /// <summary>
        /// Ids de los trabajos que cuentan para este alumno (los que ya vencieron).
        /// </summary>
        public IReadOnlySet<int> IdsTrabajosContados { get; init; } = new HashSet<int>();

        /// <summary>
        /// El menor de los dos porcentajes: desempata dentro de un mismo nivel.
        /// </summary>
        public double PeorPorcentaje => Math.Min(PorcentajeAsistencia, PorcentajeEntregas);

        /// <summary>
        /// Prioridad en el panel: 0 rojo, 1 amarillo, 2 verde, 3 sin datos.
        /// </summary>
        public int Orden => (int)Nivel;
    }

    /// <summary>
    /// Decide qué clases y trabajos le cuentan a cada alumno, calcula su nivel y ordena la lista para el panel.
    /// No accede a la base de datos: recibe los datos ya cargados, y por eso se puede probar sola.
    /// </summary>
    public class SeguimientoService
    {
        private readonly RiesgoService _riesgoService;

        /// <summary>
        /// Crea el servicio con el motor que calcula los porcentajes y el nivel.
        /// </summary>
        public SeguimientoService(RiesgoService riesgoService)
        {
            _riesgoService = riesgoService;
        }

        /// <summary>
        /// Evalúa a un alumno. Las clases y los trabajos son los de su comisión; "hoy" se recibe
        /// como parámetro para poder probar qué pasa con los trabajos vencidos y los que no.
        /// </summary>
        public ResumenAlumno Evaluar(
            Estudiante alumno,
            IEnumerable<Clase> clasesComision,
            IEnumerable<TrabajoPractico> trabajosComision,
            IEnumerable<Asistencia> asistenciasAlumno,
            IEnumerable<Entrega> entregasAlumno,
            DateTime hoy)
        {
            var asistenciasTodas = asistenciasAlumno.ToList();
            var clases = SeleccionarClasesDelAlumno(alumno, clasesComision, asistenciasTodas);
            var trabajos = SeleccionarTrabajosVencidos(trabajosComision, hoy);

            var asistencias = FiltrarAsistencias(asistenciasTodas, clases);
            var entregas = FiltrarEntregas(entregasAlumno, trabajos);

            var resultado = _riesgoService.CalcularRiesgo(asistencias, clases.Count, entregas, trabajos.Count);
            return ArmarResumen(alumno, clases, trabajos, asistencias, entregas, resultado);
        }

        /// <summary>
        /// Ordena para el panel: primero el nivel más grave y, dentro de cada nivel, el peor porcentaje;
        /// si empatan, por nombre.
        /// </summary>
        public IReadOnlyList<ResumenAlumno> Ordenar(IEnumerable<ResumenAlumno> resumenes)
        {
            return resumenes
                .OrderBy(r => r.Orden)
                .ThenBy(r => r.PeorPorcentaje)
                .ThenBy(r => r.Estudiante.NombreCompleto)
                .ToList();
        }

        /// <summary>
        /// Cuentan las clases desde el alta del alumno, y también las anteriores en las que ya hay un registro
        /// (por ejemplo, si se cargó a mano la asistencia de una clase previa a su incorporación).
        /// </summary>
        private static List<Clase> SeleccionarClasesDelAlumno(
            Estudiante alumno, IEnumerable<Clase> clasesComision, List<Asistencia> asistencias)
        {
            var conRegistro = asistencias.Select(a => a.ClaseId).ToHashSet();

            return clasesComision
                .Where(c => c.Fecha.Date >= alumno.FechaAlta.Date || conRegistro.Contains(c.Id))
                .ToList();
        }

        /// <summary>
        /// Un trabajo cuenta recién desde el día siguiente a su vencimiento: antes de eso todavía se puede entregar.
        /// </summary>
        private static List<TrabajoPractico> SeleccionarTrabajosVencidos(
            IEnumerable<TrabajoPractico> trabajosComision, DateTime hoy)
        {
            return trabajosComision.Where(t => t.FechaEntrega.Date < hoy.Date).ToList();
        }

        /// <summary>
        /// Se queda con las asistencias de las clases que cuentan.
        /// </summary>
        private static List<Asistencia> FiltrarAsistencias(List<Asistencia> asistencias, List<Clase> clases)
        {
            var idsClases = clases.Select(c => c.Id).ToHashSet();
            return asistencias.Where(a => idsClases.Contains(a.ClaseId)).ToList();
        }

        /// <summary>
        /// Se queda con las entregas de los trabajos que cuentan.
        /// </summary>
        private static List<Entrega> FiltrarEntregas(IEnumerable<Entrega> entregas, List<TrabajoPractico> trabajos)
        {
            var idsTrabajos = trabajos.Select(t => t.Id).ToHashSet();
            return entregas.Where(e => idsTrabajos.Contains(e.TrabajoPracticoId)).ToList();
        }

        /// <summary>
        /// Junta el nivel y los números base en el resumen que se devuelve.
        /// </summary>
        private static ResumenAlumno ArmarResumen(
            Estudiante alumno, List<Clase> clases, List<TrabajoPractico> trabajos,
            List<Asistencia> asistencias, List<Entrega> entregas, ResultadoRiesgo resultado)
        {
            return new ResumenAlumno
            {
                Estudiante = alumno,
                Nivel = ElegirNivel(resultado, clases.Count, trabajos.Count),
                ClasesContadas = clases.Count,
                Presentes = asistencias.Count(a => a.Presente),
                Justificadas = asistencias.Count(a => !a.Presente && a.Justificada),
                TrabajosContados = trabajos.Count,
                Entregados = entregas.Count(e => e.Entregado),
                PorcentajeAsistencia = resultado.PorcentajeAsistencia,
                PorcentajeEntregas = resultado.PorcentajeEntregas,
                IdsClasesContadas = clases.Select(c => c.Id).ToHashSet(),
                IdsTrabajosContados = trabajos.Select(t => t.Id).ToHashSet()
            };
        }

        /// <summary>
        /// Sin clases ni trabajos que contar no hay nada que evaluar, y eso no es lo mismo que estar al día.
        /// </summary>
        private static NivelSeguimiento ElegirNivel(ResultadoRiesgo resultado, int totalClases, int totalTrabajos)
        {
            if (totalClases == 0 && totalTrabajos == 0)
                return NivelSeguimiento.SinDatos;

            return resultado.Nivel switch
            {
                NivelRiesgoEnum.Rojo => NivelSeguimiento.Rojo,
                NivelRiesgoEnum.Amarillo => NivelSeguimiento.Amarillo,
                _ => NivelSeguimiento.Verde
            };
        }
    }
}
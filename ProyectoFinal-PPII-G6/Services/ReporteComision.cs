using System.Collections.Generic;
using System.Linq;

namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Qué se puede decir de una comisión: que no tiene estudiantes, que todavía no hay datos para calcular su riesgo,
    /// o cuántos estudiantes hay en cada nivel.
    /// </summary>
    public enum EstadoReporte
    {
        SinEstudiantes,
        SinDatos,
        ConDatos
    }

    /// <summary>
    /// Reporte del riesgo de una comisión (H12): cuántos de sus estudiantes están en cada nivel.
    /// No calcula el riesgo: cuenta los niveles que ya calculó el seguimiento, los mismos que muestra el panel,
    /// así que el reporte y el panel no pueden discrepar.
    /// </summary>
    public class ReporteComision
    {
        /// <summary>
        /// Si la comisión tiene estudiantes y datos para calcular su riesgo.
        /// </summary>
        public EstadoReporte Estado { get; init; }

        /// <summary>
        /// Cantidad de estudiantes de la comisión.
        /// </summary>
        public int Total { get; init; }

        /// <summary>
        /// Estudiantes en alto riesgo de abandonar.
        /// </summary>
        public int AltoRiesgo { get; init; }

        /// <summary>
        /// Estudiantes en riesgo moderado.
        /// </summary>
        public int RiesgoModerado { get; init; }

        /// <summary>
        /// Estudiantes al día.
        /// </summary>
        public int AlDia { get; init; }

        /// <summary>
        /// Estudiantes a los que todavía no se les puede calcular el riesgo (por ejemplo, los que se incorporaron hoy).
        /// </summary>
        public int EstudiantesSinDatos { get; init; }

        /// <summary>
        /// Arma el reporte a partir de los resúmenes de los estudiantes de una comisión.
        /// Dice "sin datos" cuando ningún estudiante tiene una sola asistencia, ausencia justificada o entrega registrada:
        /// en ese caso contar "0 en riesgo" (o "todos en riesgo", si hay clases cargadas) sería engañoso.
        /// </summary>
        public static ReporteComision Armar(IReadOnlyCollection<ResumenAlumno> resumenes) => new()
        {
            Estado = ElegirEstado(resumenes),
            Total = resumenes.Count,
            AltoRiesgo = Contar(resumenes, NivelSeguimiento.Rojo),
            RiesgoModerado = Contar(resumenes, NivelSeguimiento.Amarillo),
            AlDia = Contar(resumenes, NivelSeguimiento.Verde),
            EstudiantesSinDatos = Contar(resumenes, NivelSeguimiento.SinDatos)
        };

        /// <summary>
        /// Sin estudiantes, sin ningún registro de ningún estudiante, o con datos (alcanza con que uno tenga registros).
        /// </summary>
        private static EstadoReporte ElegirEstado(IReadOnlyCollection<ResumenAlumno> resumenes)
        {
            if (resumenes.Count == 0)
                return EstadoReporte.SinEstudiantes;

            return resumenes.Any(TieneRegistros) ? EstadoReporte.ConDatos : EstadoReporte.SinDatos;
        }

        /// <summary>
        /// Indica si el estudiante tiene algún registro: una asistencia, una ausencia justificada o una entrega.
        /// </summary>
        private static bool TieneRegistros(ResumenAlumno resumen) =>
            resumen.Presentes > 0 || resumen.Justificadas > 0 || resumen.Entregados > 0;

        /// <summary>
        /// Cuenta los estudiantes que están en un nivel.
        /// </summary>
        private static int Contar(IEnumerable<ResumenAlumno> resumenes, NivelSeguimiento nivel) =>
            resumenes.Count(r => r.Nivel == nivel);
    }
}
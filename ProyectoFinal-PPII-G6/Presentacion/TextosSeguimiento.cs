using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.Presentacion
{
    /// <summary>
    /// Los textos que explican de dónde sale cada porcentaje. Los usan el panel (versión corta) y la ficha (versión larga).
    /// </summary>
    public static class TextosSeguimiento
    {
        /// <summary>
        /// Asistencia en una línea, para el panel.
        /// </summary>
        public static string AsistenciaCorta(ResumenAlumno r)
        {
            if (r.ClasesContadas == 0) return "Asistencia: sin clases";

            return r.Justificadas > 0
                ? $"Asistencia {r.Presentes} + {r.Justificadas} just. de {r.ClasesContadas} ({r.PorcentajeAsistencia:F0} %)"
                : $"Asistencia {r.Presentes}/{r.ClasesContadas} ({r.PorcentajeAsistencia:F0} %)";
        }

        /// <summary>
        /// Entregas en una línea, para el panel.
        /// </summary>
        public static string EntregasCorta(ResumenAlumno r)
        {
            if (r.TrabajosContados == 0) return "Entregas: sin trabajos vencidos";

            return $"Entregas {r.Entregados}/{r.TrabajosContados} ({r.PorcentajeEntregas:F0} %)";
        }

        /// <summary>
        /// Asistencia explicada con más detalle, para la ficha.
        /// </summary>
        public static string AsistenciaLarga(ResumenAlumno r)
        {
            if (r.ClasesContadas == 0) return "Asistencia: sin clases desde el alta";

            return r.Justificadas > 0
                ? $"Asistencia: {r.Presentes} presentes + {r.Justificadas} justificadas (valen la mitad) de {r.ClasesContadas} clases ({r.PorcentajeAsistencia:F0} %)"
                : $"Asistencia: {r.Presentes} de {r.ClasesContadas} clases ({r.PorcentajeAsistencia:F0} %)";
        }

        /// <summary>
        /// Entregas explicadas con más detalle, para la ficha.
        /// </summary>
        public static string EntregasLarga(ResumenAlumno r)
        {
            if (r.TrabajosContados == 0) return "Entregas: ningún trabajo vencido todavía";

            return $"Entregas: {r.Entregados} de {r.TrabajosContados} trabajos vencidos ({r.PorcentajeEntregas:F0} %)";
        }
    }
}
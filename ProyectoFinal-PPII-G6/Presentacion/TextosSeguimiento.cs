using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.Presentacion
{
    /// <summary>
    /// Los textos que explican de dónde sale cada porcentaje. Los usan el panel (versión corta) y la ficha (versión larga).
    /// </summary>
    public static class TextosSeguimiento
    {
        /// <summary>
        /// Asistencia en una línea, para el panel. Muestra la base del porcentaje ("1 de 4 clases").
        /// </summary>
        public static string AsistenciaCorta(ResumenAlumno r)
        {
            if (r.ClasesContadas == 0) return "Asistencia: sin clases";

            var faltas = r.ClasesContadas - r.Presentes;
            var pctInasistencias = 100 - r.PorcentajeAsistencia;

            return r.Justificadas > 0
                ? $"Inasistencias {faltas} de {r.ClasesContadas} clases ({pctInasistencias:F0} %), {r.Justificadas} just."
                : $"Inasistencias {faltas} de {r.ClasesContadas} clases ({pctInasistencias:F0} %)";
        }

        /// <summary>
        /// Entregas en una línea, para el panel.
        /// </summary>
        public static string EntregasCorta(ResumenAlumno r)
        {
            if (r.TrabajosContados == 0) return "Entregas: sin trabajos vencidos";

            var noEntregados = r.TrabajosContados - r.Entregados;
            return $"No entregados {noEntregados} de {r.TrabajosContados} ({100 - r.PorcentajeEntregas:F0} %)";
        }

        /// <summary>
        /// Asistencia explicada con más detalle, para la ficha.
        /// </summary>
        public static string AsistenciaLarga(ResumenAlumno r)
        {
            if (r.ClasesContadas == 0) return "Asistencia: sin clases desde la incorporación";

            var faltas = r.ClasesContadas - r.Presentes;
            var pctInasistencias = 100 - r.PorcentajeAsistencia;

            return r.Justificadas > 0
                ? $"Inasistencias: {faltas} de {r.ClasesContadas} clases ({pctInasistencias:F0} %), de las cuales {r.Justificadas} justificadas (valen la mitad)"
                : $"Inasistencias: {faltas} de {r.ClasesContadas} clases ({pctInasistencias:F0} %)";
        }

        /// <summary>
        /// Entregas explicadas con más detalle, para la ficha.
        /// </summary>
        public static string EntregasLarga(ResumenAlumno r)
        {
            if (r.TrabajosContados == 0) return "Entregas: ningún trabajo vencido todavía";

            var noEntregados = r.TrabajosContados - r.Entregados;
            return $"No entregados: {noEntregados} de {r.TrabajosContados} trabajos vencidos ({100 - r.PorcentajeEntregas:F0} %)";
        }

        /// <summary>
        /// Texto de la racha para el panel. Distingue "sin datos" de "sin ausencias seguidas".
        /// </summary>
        public static string RachaCorta(ResumenAlumno r) => r.Racha.Estado switch
        {
            EstadoRacha.SinDatos => "Ausencias seguidas: sin datos",
            EstadoRacha.SinRacha => "Sin ausencias seguidas",
            EstadoRacha.ConRacha => r.Racha.Cantidad == 1
                ? "1 ausencia seguida"
                : $"{r.Racha.Cantidad} ausencias seguidas",
            _ => "Ausencias seguidas: sin datos"
        };

        /// <summary>
        /// Inasistencias compactas para la grilla del panel ("1 de 4 · 25 %").
        /// </summary>
        public static string AsistenciaGrilla(ResumenAlumno r)
        {
            if (r.ClasesContadas == 0) return "Sin clases";
            var faltas = r.ClasesContadas - r.Presentes;
            return $"{faltas} de {r.ClasesContadas} · {100 - r.PorcentajeAsistencia:F0} %";
        }

        /// <summary>
        /// Entregas compactas para la grilla del panel ("2 de 3 · 67 %").
        /// </summary>
        public static string EntregasGrilla(ResumenAlumno r)
        {
            if (r.TrabajosContados == 0) return "Sin TPs";
            var noEntregados = r.TrabajosContados - r.Entregados;
            return $"{noEntregados} de {r.TrabajosContados} · {100 - r.PorcentajeEntregas:F0} %";
        }

        /// <summary>
        /// Texto de la racha para la ficha.
        /// </summary>
        public static string RachaLarga(ResumenAlumno r) => r.Racha.Estado switch
        {
            EstadoRacha.SinDatos => "Ausencias seguidas: sin datos de asistencia tomada",
            EstadoRacha.SinRacha => "Ausencias seguidas: ninguna (la última clase con lista lo tuvo presente)",
            EstadoRacha.ConRacha => r.Racha.Cantidad == 1
                ? "Ausencias seguidas: 1 clase hasta la última con asistencia tomada"
                : $"Ausencias seguidas: {r.Racha.Cantidad} clases hasta la última con asistencia tomada",
            _ => "Ausencias seguidas: sin datos de asistencia tomada"
        };
    }
}

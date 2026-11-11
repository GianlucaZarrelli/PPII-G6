using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.Presentacion
{
    /// <summary>
    /// Textos del reporte de una comisión y tamaño de las barras del gráfico. Es el único lugar donde están escritos.
    /// </summary>
    public static class PresentacionReporte
    {
        /// <summary>
        /// Mensaje cuando la comisión no tiene estudiantes cargados.
        /// </summary>
        public const string MensajeSinEstudiantes = "La comisión todavía no tiene estudiantes.";

        /// <summary>
        /// Mensaje cuando no hay ninguna asistencia ni entrega registrada: no se muestran números que engañen.
        /// </summary>
        public const string MensajeSinDatos = "Todavía no hay datos para calcular el riesgo de esta comisión.";

        /// <summary>
        /// Frase principal del reporte, por ejemplo "7 de 30 estudiantes en alto riesgo de abandonar",
        /// o el mensaje que explica por qué no hay números.
        /// </summary>
        public static string Titular(ReporteComision reporte) => reporte.Estado switch
        {
            EstadoReporte.SinEstudiantes => MensajeSinEstudiantes,
            EstadoReporte.SinDatos => MensajeSinDatos,
            _ => $"{reporte.AltoRiesgo} de {reporte.Total} {(reporte.Total == 1 ? "estudiante" : "estudiantes")} en alto riesgo de abandonar"
        };

        /// <summary>
        /// Los otros niveles, en una línea. Vacío si no hay números que mostrar.
        /// </summary>
        public static string Detalle(ReporteComision reporte) => reporte.Estado != EstadoReporte.ConDatos
            ? string.Empty
            : $"{PresentacionNivel.Texto(NivelSeguimiento.Amarillo)}: {reporte.RiesgoModerado}  |  "
              + $"{PresentacionNivel.Texto(NivelSeguimiento.Verde)}: {reporte.AlDia}  |  "
              + $"{PresentacionNivel.Texto(NivelSeguimiento.SinDatos)}: {reporte.EstudiantesSinDatos}";

        /// <summary>
        /// Porcentaje de estudiantes que hay en un nivel, sin decimales y con el mismo formato que el panel ("23 %").
        /// </summary>
        public static string Porcentaje(int cantidad, int total) =>
            total == 0 ? "0 %" : $"{100.0 * cantidad / total:F0} %";

        /// <summary>
        /// Ancho de la barra de un nivel: proporcional a la cantidad de estudiantes, con "anchoMaximo" para el total.
        /// </summary>
        public static double Ancho(int cantidad, int total, double anchoMaximo) =>
            total == 0 ? 0 : anchoMaximo * cantidad / total;
    }
}
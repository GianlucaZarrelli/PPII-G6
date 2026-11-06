using System;
using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.Presentacion
{
    /// <summary>
    /// Textos y colores con los que se muestra el calendario de una comisión. Es el único lugar donde están escritos.
    /// </summary>
    public static class PresentacionCalendario
    {
        /// <summary>
        /// Texto del tipo de evento.
        /// </summary>
        public static string Tipo(TipoEventoCalendario tipo) =>
            tipo == TipoEventoCalendario.Clase ? "Clase" : "Entrega de trabajo";

        /// <summary>
        /// Cuánto falta o cuánto pasó desde el evento: "Hoy", "Mañana", "En 3 días", "Ayer", "Hace 5 días".
        /// </summary>
        public static string Etapa(DateTime fecha, DateTime hoy)
        {
            var dias = (fecha.Date - hoy.Date).Days;

            return dias switch
            {
                0 => "Hoy",
                1 => "Mañana",
                -1 => "Ayer",
                > 1 => $"En {dias} días",
                _ => $"Hace {-dias} días"
            };
        }

        /// <summary>
        /// Color de la etapa: gris si ya pasó, violeta si es hoy, verde si todavía falta.
        /// </summary>
        public static string Color(DateTime fecha, DateTime hoy) => fecha.Date.CompareTo(hoy.Date) switch
        {
            < 0 => "#757575",
            0 => "#512BD4",
            _ => "#388E3C"
        };
    }
}
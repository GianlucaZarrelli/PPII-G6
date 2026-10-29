using System;

namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Reglas del registro de asistencia de una clase.
    /// </summary>
    public static class ValidacionAsistencia
    {
        /// <summary>
        /// Devuelve el motivo por el que no se puede registrar asistencia en esa fecha, o null si se puede.
        /// Una clase futura inflaría el total de clases y marcaría ausentes a todos los alumnos.
        /// Se compara solo el día, sin la hora, y la fecha de hoy se recibe como parámetro para poder probarla.
        /// </summary>
        public static string? ObtenerErrorDeFecha(DateTime fecha, DateTime hoy) =>
            fecha.Date > hoy.Date
                ? "No se puede registrar asistencia de una fecha futura."
                : null;
    }
}
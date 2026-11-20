using System;

namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Reglas de la fecha de incorporación de un alumno a la comisión.
    /// </summary>
    public static class ValidacionIncorporacion
    {
        /// <summary>
        /// Devuelve el motivo por el que la fecha no sirve, o null si está bien.
        /// La fecha es opcional: null significa que el alumno cuenta desde el inicio de la comisión.
        /// No se acepta una fecha posterior a hoy: todavía no se incorporó.
        /// </summary>
        public static string? ObtenerError(DateTime? fechaIncorporacion, DateTime hoy) =>
            fechaIncorporacion.HasValue && fechaIncorporacion.Value.Date > hoy.Date
                ? "La fecha de incorporación no puede ser posterior a hoy."
                : null;
    }
}

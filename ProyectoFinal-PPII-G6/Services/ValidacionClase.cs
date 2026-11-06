using System;

namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Reglas de los datos de una clase del calendario: la fecha y la descripción.
    /// A diferencia de la asistencia, una clase se puede cargar por adelantado: la fecha puede ser futura.
    /// </summary>
    public static class ValidacionClase
    {
        /// <summary>
        /// Largo máximo de la descripción, igual al de la columna de la base de datos.
        /// </summary>
        public const int LargoMaximoDescripcion = 200;

        /// <summary>
        /// Devuelve el primer problema que tenga la clase, o null si está bien. La fecha tiene que caer dentro
        /// del año de "hoy" y la descripción (que es opcional) tiene que entrar en la base.
        /// La fecha de hoy se recibe como parámetro para poder probar el cambio de año.
        /// </summary>
        public static string? ObtenerError(DateTime fecha, string? descripcion, DateTime hoy) =>
            AnioEnCurso.ErrorFueraDelAnio(fecha, hoy, "de la clase") ?? ErrorDeDescripcion(descripcion);

        /// <summary>
        /// Avisa si la descripción no entra en la base. Los espacios de los extremos no cuentan.
        /// </summary>
        private static string? ErrorDeDescripcion(string? descripcion) =>
            (descripcion ?? string.Empty).Trim().Length > LargoMaximoDescripcion
                ? $"La descripción admite hasta {LargoMaximoDescripcion} caracteres."
                : null;
    }
}
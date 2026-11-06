using System;
using System.Globalization;

namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Regla compartida de las fechas del calendario: tienen que caer dentro del año en curso.
    /// La usan los trabajos prácticos y las clases, para que el límite y el mensaje estén en un solo lugar.
    /// </summary>
    public static class AnioEnCurso
    {
        /// <summary>
        /// Avisa si la fecha cae fuera del año de "hoy" (del 1 de enero al 31 de diciembre, ambos incluidos),
        /// o devuelve null si está bien. Se compara solo el día, sin la hora.
        /// "nombreDeLaFecha" completa el mensaje: "La fecha {nombreDeLaFecha} tiene que estar entre...".
        /// </summary>
        public static string? ErrorFueraDelAnio(DateTime fecha, DateTime hoy, string nombreDeLaFecha)
        {
            var primero = new DateTime(hoy.Year, 1, 1);
            var ultimo = new DateTime(hoy.Year, 12, 31);

            if (fecha.Date >= primero && fecha.Date <= ultimo)
                return null;

            return $"La fecha {nombreDeLaFecha} tiene que estar entre el {Formatear(primero)} y el {Formatear(ultimo)}.";
        }

        /// <summary>
        /// Escribe la fecha como día/mes/año, igual en cualquier idioma del equipo.
        /// </summary>
        private static string Formatear(DateTime fecha) =>
            fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
    }
}
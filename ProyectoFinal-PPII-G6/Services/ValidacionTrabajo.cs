using System;
using System.Globalization;

namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Reglas de los datos de un trabajo práctico: el título y la fecha de entrega (el vencimiento).
    /// </summary>
    public static class ValidacionTrabajo
    {
        /// <summary>
        /// Largo máximo del título, igual al de la columna de la base de datos.
        /// </summary>
        public const int LargoMaximoTitulo = 150;

        /// <summary>
        /// Devuelve el primer problema que tenga el trabajo, o null si está bien.
        /// El título tiene que existir y entrar en la base, y la fecha de entrega tiene que caer
        /// dentro del año de "hoy" (del 1 de enero al 31 de diciembre, ambos incluidos).
        /// La fecha de hoy se recibe como parámetro para poder probar el cambio de año.
        /// </summary>
        public static string? ObtenerError(string? titulo, DateTime fechaEntrega, DateTime hoy) =>
            ErrorDeTitulo(titulo) ?? ErrorDeFecha(fechaEntrega, hoy);

        /// <summary>
        /// Avisa si el título está vacío o no entra en la base.
        /// </summary>
        private static string? ErrorDeTitulo(string? titulo)
        {
            var texto = (titulo ?? string.Empty).Trim();

            if (texto.Length == 0)
                return "El título del trabajo es obligatorio.";

            return texto.Length > LargoMaximoTitulo
                ? $"El título admite hasta {LargoMaximoTitulo} caracteres."
                : null;
        }

        /// <summary>
        /// Avisa si la fecha de entrega cae fuera del año en curso. Se compara solo el día, sin la hora.
        /// </summary>
        private static string? ErrorDeFecha(DateTime fechaEntrega, DateTime hoy)
        {
            var primero = new DateTime(hoy.Year, 1, 1);
            var ultimo = new DateTime(hoy.Year, 12, 31);

            if (fechaEntrega.Date >= primero && fechaEntrega.Date <= ultimo)
                return null;

            return $"La fecha de entrega tiene que estar entre el {Formatear(primero)} y el {Formatear(ultimo)}.";
        }

        /// <summary>
        /// Escribe la fecha como día/mes/año, igual en cualquier idioma del equipo.
        /// </summary>
        private static string Formatear(DateTime fecha) =>
            fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
    }
}
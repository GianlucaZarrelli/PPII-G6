using System;
using ProyectoFinal_PPII_G6.Models;

namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Reglas del registro de un contacto con un estudiante.
    /// </summary>
    public static class ValidacionContacto
    {
        /// <summary>
        /// Largo máximo del resultado, igual al de la columna de la base de datos.
        /// </summary>
        public const int LargoMaximoResultado = 500;

        /// <summary>
        /// Devuelve el primer problema que tenga el contacto, o null si está bien. Se revisa en el mismo orden
        /// en que se completa el formulario: la fecha y la hora (que no sean futuras), el medio y el resultado.
        /// El momento actual se recibe como parámetro para poder probar el límite.
        /// </summary>
        public static string? ObtenerError(DateTime fechaHora, MedioContacto? medio, string? resultado, DateTime ahora) =>
            ErrorDeFechaHora(fechaHora, ahora) ?? ErrorDeMedio(medio) ?? ErrorDeResultado(resultado);

        /// <summary>
        /// Un contacto ya ocurrió: no puede tener una fecha y hora posteriores a este momento.
        /// </summary>
        private static string? ErrorDeFechaHora(DateTime fechaHora, DateTime ahora) =>
            fechaHora > ahora
                ? "La fecha y la hora del contacto no pueden ser futuras."
                : null;

        /// <summary>
        /// Avisa si no se eligió el medio de contacto.
        /// </summary>
        private static string? ErrorDeMedio(MedioContacto? medio) =>
            medio == null
                ? "Elegí el medio de contacto."
                : null;

        /// <summary>
        /// Avisa si el resultado está vacío o no entra en la base. Los espacios de los extremos no cuentan.
        /// </summary>
        private static string? ErrorDeResultado(string? resultado)
        {
            var texto = (resultado ?? string.Empty).Trim();

            if (texto.Length == 0)
                return "El resultado del contacto es obligatorio.";

            return texto.Length > LargoMaximoResultado
                ? $"El resultado admite hasta {LargoMaximoResultado} caracteres."
                : null;
        }
    }
}
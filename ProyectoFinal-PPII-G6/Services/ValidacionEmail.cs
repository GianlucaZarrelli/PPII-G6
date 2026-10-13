using System;
using System.Net.Mail;

namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Regla de validación del email de un alumno, compartida por el alta manual y la carga masiva.
    /// </summary>
    public static class ValidacionEmail
    {
        /// <summary>
        /// Largo máximo de la columna Email en la base de datos.
        /// </summary>
        private const int LargoMaximo = 100;

        /// <summary>
        /// Indica si el texto es una dirección de email válida: no vacía, con formato nombre@dominio.ext
        /// y que entre en la columna de la base.
        /// </summary>
        public static bool EsValido(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            var texto = email.Trim();
            if (texto.Length > LargoMaximo) return false;

            if (!MailAddress.TryCreate(texto, out var direccion)) return false;

            // Rechaza formas como "Ana <ana@dominio.com>": tiene que ser solo la dirección
            if (!string.Equals(direccion.Address, texto, StringComparison.OrdinalIgnoreCase)) return false;

            // El dominio tiene que tener al menos un punto ("demo.test"), así "ana@demo" no pasa
            return direccion.Host.Contains('.');
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    ///  Reglas de nombre de una comisión nueva: tiene que existir, entrar en la base y no estar repetido.
    /// </summary>
    public static class ValidacionComision
    {
        /// <summary>
        /// Largo maximo de caracteres para el nombre de una comisión.
        /// </summary>
        public const int LargoMaximoNombre = 100;

        /// <summary>
        /// Devuelve el primer problema que tenga el nombre, o null si está bien. Los espacios de los extremos no cuentan.
        /// Se revisa en este orden: que exista, que entre en la base y que no esté repetido.
        /// </summary>
        
        public static string? ObtenerError(string? nombre, IEnumerable<string> nombresExistentes)
        {
            var texto = (nombre ?? string.Empty).Trim();

            if (texto.Length == 0)
                return "El nombre de la comisión es obligatorio.";

            if (texto.Length > LargoMaximoNombre)
                return $"El nombre de la comisión admite hasta {LargoMaximoNombre} caracteres.";

            return ErrorDeRepetida(texto, nombresExistentes);
        }

        /// <summary>
        /// Avisa si ya hay una comisión con ese nombre. No distingue mayúsculas de minúsculas,
        /// igual que la base de datos: "a" y "A" serían la misma comisión.
        /// </summary>
        private static string? ErrorDeRepetida(string texto, IEnumerable<string> nombresExistentes) =>
            nombresExistentes.Any(e => string.Equals(e.Trim(), texto, StringComparison.OrdinalIgnoreCase))
                ? "Ya existe una comisión con ese nombre."
                : null;

    }
}

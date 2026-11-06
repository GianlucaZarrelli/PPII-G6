using System.Linq;
using ProyectoFinal_PPII_G6.Models;

namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Reglas de los umbrales de riesgo: son porcentajes y, en cada uno, el de alto riesgo tiene que ser menor
    /// que el de riesgo moderado (si no, el nivel moderado no tendría ningún valor posible).
    /// </summary>
    public static class ValidacionUmbrales
    {
        /// <summary>
        /// Valor mínimo que puede tener un umbral.
        /// </summary>
        public const int Minimo = 0;

        /// <summary>
        /// Valor máximo que puede tener un umbral.
        /// </summary>
        public const int Maximo = 100;

        /// <summary>
        /// Devuelve el primer problema que tengan los umbrales, o null si están bien. Se revisa primero que sean
        /// porcentajes válidos, después la asistencia y por último las entregas.
        /// </summary>
        public static string? ObtenerError(UmbralesRiesgo umbrales) =>
            ErrorDeRango(umbrales)
            ?? ErrorDeOrden("la asistencia", umbrales.AsistenciaRojo, umbrales.AsistenciaAmarillo)
            ?? ErrorDeOrden("las entregas", umbrales.EntregasRojo, umbrales.EntregasAmarillo);

        /// <summary>
        /// Avisa si alguno de los cuatro valores no es un porcentaje.
        /// </summary>
        private static string? ErrorDeRango(UmbralesRiesgo umbrales)
        {
            var valores = new[] { umbrales.AsistenciaRojo, umbrales.EntregasRojo, umbrales.AsistenciaAmarillo, umbrales.EntregasAmarillo };

            return valores.Any(v => v < Minimo || v > Maximo)
                ? $"Los umbrales son porcentajes: tienen que estar entre {Minimo} y {Maximo}."
                : null;
        }

        /// <summary>
        /// Avisa si el umbral de alto riesgo no es menor que el de riesgo moderado.
        /// </summary>
        private static string? ErrorDeOrden(string queSeMide, int rojo, int amarillo) =>
            rojo >= amarillo
                ? $"En {queSeMide}, el umbral de alto riesgo tiene que ser menor que el de riesgo moderado."
                : null;
    }
}
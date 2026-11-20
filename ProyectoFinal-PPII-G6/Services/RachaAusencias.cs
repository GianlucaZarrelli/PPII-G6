using System;
using System.Collections.Generic;
using System.Linq;
using ProyectoFinal_PPII_G6.Models;

namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Cómo terminó el cálculo de ausencias seguidas: sin datos, sin racha vigente, o con una cantidad.
    /// </summary>
    public enum EstadoRacha
    {
        /// <summary>
        /// El alumno no tiene ningún registro de asistencia en clases con lista tomada (que le cuenten).
        /// </summary>
        SinDatos,

        /// <summary>
        /// Tiene datos y la última clase con lista lo tuvo presente: no hay ausencias seguidas.
        /// </summary>
        SinRacha,

        /// <summary>
        /// Hay una o más ausencias seguidas hasta la última clase con asistencia tomada.
        /// </summary>
        ConRacha
    }

    /// <summary>
    /// Resultado de calcular la racha de ausencias de un alumno.
    /// </summary>
    public sealed class ResultadoRacha
    {
        /// <summary>
        /// Estado de la racha.
        /// </summary>
        public EstadoRacha Estado { get; init; }

        /// <summary>
        /// Cantidad de ausencias seguidas. Solo tiene sentido cuando el estado es ConRacha.
        /// </summary>
        public int Cantidad { get; init; }

        /// <summary>
        /// Sin registros del alumno en clases con lista tomada.
        /// </summary>
        public static ResultadoRacha SinDatos() => new() { Estado = EstadoRacha.SinDatos };

        /// <summary>
        /// Tiene datos y no hay ausencias seguidas vigentes.
        /// </summary>
        public static ResultadoRacha SinRacha() => new() { Estado = EstadoRacha.SinRacha };

        /// <summary>
        /// Racha vigente con la cantidad indicada.
        /// </summary>
        public static ResultadoRacha Con(int cantidad) => new()
        {
            Estado = EstadoRacha.ConRacha,
            Cantidad = cantidad
        };
    }

    /// <summary>
    /// Calcula cuántas clases seguidas lleva faltando un alumno, mirando solo las clases
    /// en las que ya se tomó asistencia en la comisión. No usa las mismas reglas que el riesgo:
    /// las clases futuras y las pasadas sin lista no suman ni cortan la racha.
    /// </summary>
    public static class RachaAusencias
    {
        /// <summary>
        /// Calcula la racha vigente hasta la última clase con asistencia tomada.
        /// Si el alumno tiene fecha de incorporación, solo mira clases desde esa fecha.
        /// Una presencia corta la racha; una ausencia (incluida la justificada) la sigue.
        /// Sin registro del alumno en ninguna clase con lista → sin datos (no "0 ausencias").
        /// </summary>
        public static ResultadoRacha Calcular(
            Estudiante alumno,
            IEnumerable<Clase> clasesComision,
            IEnumerable<Asistencia> asistenciasComision,
            IEnumerable<Asistencia> asistenciasAlumno,
            DateTime hoy)
        {
            var idsConLista = asistenciasComision.Select(a => a.ClaseId).ToHashSet();
            var clasesConLista = clasesComision
                .Where(c => c.Fecha.Date <= hoy.Date)
                .Where(c => idsConLista.Contains(c.Id))
                .Where(c => !alumno.FechaAlta.HasValue || c.Fecha.Date >= alumno.FechaAlta.Value.Date)
                .OrderByDescending(c => c.Fecha.Date)
                .ThenByDescending(c => c.Id)
                .ToList();

            if (clasesConLista.Count == 0)
                return ResultadoRacha.SinDatos();

            var porClase = asistenciasAlumno
                .GroupBy(a => a.ClaseId)
                .ToDictionary(g => g.Key, g => g.First());

            // Sin ningún registro propio no se puede hablar de racha: es "sin datos", no cero.
            if (!clasesConLista.Any(c => porClase.ContainsKey(c.Id)))
                return ResultadoRacha.SinDatos();

            var cantidad = 0;
            foreach (var clase in clasesConLista)
            {
                if (porClase.TryGetValue(clase.Id, out var registro) && registro.Presente)
                    break;

                cantidad++;
            }

            return cantidad == 0
                ? ResultadoRacha.SinRacha()
                : ResultadoRacha.Con(cantidad);
        }
    }
}

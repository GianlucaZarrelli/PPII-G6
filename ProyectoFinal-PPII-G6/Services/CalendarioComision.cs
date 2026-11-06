using System;
using System.Collections.Generic;
using System.Linq;
using ProyectoFinal_PPII_G6.Models;

namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Qué es un evento del calendario de una comisión.
    /// </summary>
    public enum TipoEventoCalendario
    {
        Clase,
        Trabajo
    }

    /// <summary>
    /// Un día del calendario de una comisión: una clase o la fecha de entrega de un trabajo.
    /// </summary>
    public class EventoCalendario
    {
        /// <summary>
        /// Día en que ocurre el evento.
        /// </summary>
        public DateTime Fecha { get; init; }

        /// <summary>
        /// Si es una clase o la entrega de un trabajo.
        /// </summary>
        public TipoEventoCalendario Tipo { get; init; }

        /// <summary>
        /// Descripción de la clase o título del trabajo.
        /// </summary>
        public string Titulo { get; init; } = string.Empty;
    }

    /// <summary>
    /// Arma el calendario de una comisión juntando sus clases y los vencimientos de sus trabajos.
    /// No accede a la base de datos: recibe los datos ya cargados, y por eso se puede probar sola.
    /// </summary>
    public static class CalendarioComision
    {
        /// <summary>
        /// Texto que se muestra para una clase sin descripción.
        /// </summary>
        public const string TituloClaseSinDescripcion = "Clase";

        /// <summary>
        /// Junta las clases y los trabajos en una sola lista ordenada por fecha. Si coinciden en el mismo día,
        /// va primero la clase y después la entrega; si también coinciden en el tipo, por título.
        /// </summary>
        public static IReadOnlyList<EventoCalendario> Armar(
            IEnumerable<Clase> clases, IEnumerable<TrabajoPractico> trabajos)
        {
            var eventosDeClases = clases.Select(c => Crear(c.Fecha, TipoEventoCalendario.Clase, TituloDeClase(c)));
            var eventosDeTrabajos = trabajos.Select(t => Crear(t.FechaEntrega, TipoEventoCalendario.Trabajo, t.Titulo));

            return eventosDeClases.Concat(eventosDeTrabajos)
                .OrderBy(e => e.Fecha)
                .ThenBy(e => e.Tipo)
                .ThenBy(e => e.Titulo)
                .ToList();
        }

        /// <summary>
        /// Descripción de la clase, o el texto por defecto si no tiene.
        /// </summary>
        private static string TituloDeClase(Clase clase) =>
            string.IsNullOrWhiteSpace(clase.Descripcion) ? TituloClaseSinDescripcion : clase.Descripcion.Trim();

        /// <summary>
        /// Arma un evento. Se guarda solo el día: la hora no importa en el calendario.
        /// </summary>
        private static EventoCalendario Crear(DateTime fecha, TipoEventoCalendario tipo, string titulo) => new()
        {
            Fecha = fecha.Date,
            Tipo = tipo,
            Titulo = titulo
        };
    }
}
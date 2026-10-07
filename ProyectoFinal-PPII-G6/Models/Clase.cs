using System;

namespace ProyectoFinal_PPII_G6.Models
{
    /// <summary>
    /// Clase (día de cursada) sobre la que se registran asistencias.
    /// </summary>
    public class Clase
    {
        /// <summary>
        /// Identificador de la clase.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Fecha en la que se dictó la clase.
        /// </summary>
        public DateTime Fecha { get; set; }

        /// <summary>
        /// Descripción de la clase.
        /// </summary>
        public string? Descripcion { get; set; } = string.Empty;

        public int ComisionId { get; set; }

        /// <summary>
        /// Constructor vacío requerido por Entity Framework.
        /// </summary>
        public Clase() { }

        /// <summary>
        /// Crea una clase con su fecha y una descripción opcional.
        /// </summary>
        public Clase(int id, DateTime fecha, string? descripcion = null, int comisionId = 0)
        {
            Id = id;
            Fecha = fecha;
            Descripcion = descripcion ?? string.Empty;
            ComisionId = comisionId;
        }
    }
}

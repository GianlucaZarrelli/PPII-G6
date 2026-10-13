using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoFinal_PPII_G6.Models
{
    /// <summary>
    /// Trabajo práctico asignado a una comisión.
    /// </summary>
    public class TrabajoPractico
    {
        /// <summary>
        /// Identificador del trabajo práctico.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identificador de la comisión a la que pertenece.
        /// </summary>
        public int ComisionId { get; set; }

        /// <summary>
        /// Título del trabajo práctico.
        /// </summary>
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Fecha límite de entrega.
        /// </summary>
        public DateTime FechaEntrega { get; set; }

        /// <summary>
        /// Constructor vacío requerido por Entity Framework.
        /// </summary>
        public TrabajoPractico() { }

        /// <summary>
        /// Crea un trabajo práctico con todos sus datos.
        /// </summary>
        public TrabajoPractico(int id, int comisionId, string titulo, DateTime fechaEntrega)
        {
            Id = id;
            ComisionId = comisionId;
            Titulo = titulo;
            FechaEntrega = fechaEntrega;
        }
    }
}

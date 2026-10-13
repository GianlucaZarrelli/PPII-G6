using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoFinal_PPII_G6.Models
{
    /// <summary>
    /// Registro de entrega de un trabajo práctico por parte de un estudiante.
    /// </summary>
    public class Entrega
    {
        /// <summary>
        /// Identificador del registro.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Identificador del estudiante.
        /// </summary>
        public int EstudianteId { get; set; }
        /// <summary>
        /// Identificador del trabajo práctico.
        /// </summary>
        public int TrabajoPracticoId { get; set; }
        /// <summary>
        /// Indica si el estudiante entregó el trabajo práctico.
        /// </summary>
        public bool Entregado { get; set; }
        
        /// <summary>
        /// Constructor vacío requerido por Entity Framework.
        /// </summary>
        public Entrega() { }

        /// <summary>
        /// Crea un registro de entrega con todos sus datos.
        /// </summary>
        public Entrega(int id, int estudianteId, int trabajoPracticoId, bool entregado)
        {
            Id = id;
            EstudianteId = estudianteId;
            TrabajoPracticoId = trabajoPracticoId;
            Entregado = entregado;
        }
    }
}

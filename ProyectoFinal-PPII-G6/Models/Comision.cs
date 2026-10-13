using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoFinal_PPII_G6.Models
{
    /// <summary>
    /// Comisión (curso) que agrupa estudiantes, clases y trabajos prácticos.
    /// </summary>
    public class Comision
    {
        /// <summary>
        /// Identificador de la comisión.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre de la comisión.
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Constructor vacío requerido por Entity Framework.
        /// </summary>
        public Comision() { }

        /// <summary>
        /// Crea una comisión con su Id y nombre.
        /// </summary>
        public Comision(int id, string nombre)
        {
            Id = id;
            Nombre = nombre;
        }
    }
}

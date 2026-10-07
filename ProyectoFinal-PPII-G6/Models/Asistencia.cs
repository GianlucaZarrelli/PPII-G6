using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoFinal_PPII_G6.Models
{
    /// <summary>
    /// Registro de asistencia de un estudiante a una clase.
    /// </summary>
    public class Asistencia
    {
        /// <summary>
        /// Constructor vacío requerido por Entity Framework.
        /// </summary>
        public Asistencia()
        {
        }

        /// <summary>
        /// Crea un registro de asistencia con todos sus datos.
        /// </summary>
        public Asistencia(int id, int estudianteId, int claseId, bool presente, bool justificada)
        {
            this.Id = id;
            this.EstudianteId = estudianteId;
            this.ClaseId = claseId;
            this.Presente = presente;
            this.Justificada = justificada;
        }

        /// <summary>
        /// Identificador del registro.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identificador del estudiante.
        /// </summary>
        public int EstudianteId { get; set; }

        /// <summary>
        /// Identificador de la clase.
        /// </summary>
        public int ClaseId { get; set; }

        /// <summary>
        /// Indica si el estudiante estuvo presente.
        /// </summary>
        public bool Presente { get; set; }

        public bool Justificada { get; set; }
    }
}
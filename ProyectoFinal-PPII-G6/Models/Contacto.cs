using System;

namespace ProyectoFinal_PPII_G6.Models
{
    /// <summary>
    /// Contacto (intervención) del tutor con un estudiante, con su medio y su resultado.
    /// </summary>
    public class Contacto
    {
        /// <summary>
        /// Identificador del contacto.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identificador del estudiante con el que se tuvo el contacto.
        /// </summary>
        public int EstudianteId { get; set; }

        /// <summary>
        /// Fecha y hora en que se realizó el contacto.
        /// </summary>
        public DateTime FechaHora { get; set; }

        /// <summary>
        /// Medio por el que se realizó el contacto.
        /// </summary>
        public MedioContacto Medio { get; set; }

        /// <summary>
        /// Resultado u observaciones del contacto.
        /// </summary>
        public string Resultado { get; set; } = string.Empty;

        /// <summary>
        /// Constructor vacío requerido por Entity Framework.
        /// </summary>
        public Contacto() { }

        /// <summary>
        /// Crea un contacto con todos sus datos.
        /// </summary>
        public Contacto(int id, int estudianteId, DateTime fechaHora, MedioContacto medio, string resultado)
        {
            Id = id;
            EstudianteId = estudianteId;
            FechaHora = fechaHora;
            Medio = medio;
            Resultado = resultado;
        }
    }
}
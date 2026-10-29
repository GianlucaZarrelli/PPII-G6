using System;

namespace ProyectoFinal_PPII_G6.Models
{
    /// <summary>
    /// Estudiante registrado en el sistema.
    /// </summary>
    public class Estudiante
    {
        /// <summary>
        /// Constructor vacío requerido por Entity Framework.
        /// </summary>
        public Estudiante()
        {
        }

        /// <summary>
        /// Crea un estudiante con sus datos personales.
        /// </summary>
        public Estudiante(int id, string dni, string nombre, string apellido, string email = "", int comisionId = 0)
        {
            this.Id = id;
            this.Dni = dni;
            this.Nombre = nombre;
            this.Apellido = apellido;
            this.ComisionId = comisionId;
            this.Email = email;
        }

        /// <summary>
        /// Identificador del estudiante.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// DNI del estudiante.
        /// </summary>
        public string Dni { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del estudiante.
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Apellido del estudiante.
        /// </summary>
        public string Apellido { get; set; } = string.Empty;

        /// <summary>
        /// Email del estudiante.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Identificador de la comisión en la que está inscripto el estudiante.
        /// </summary>
        public int ComisionId { get; set; }

        /// <summary>
        /// Fecha desde la que el estudiante forma parte de la comisión. Las clases anteriores no se le cuentan.
        /// </summary>
        public DateTime FechaAlta { get; set; } = DateTime.Today;

        /// <summary>
        /// Nombre y apellido juntos, para mostrar en pantalla.
        /// </summary>
        public string NombreCompleto => $"{Nombre} {Apellido}";
    }
}
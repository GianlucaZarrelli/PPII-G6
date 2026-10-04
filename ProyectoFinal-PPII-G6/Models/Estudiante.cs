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
        public Estudiante(int id, string dni, string nombre, string apellido, string email = "")
        {
            this.Id = id;
            this.Dni = dni;
            this.Nombre = nombre;
            this.Apellido = apellido;
            this.Email = email;
        }

        /// <summary>
        /// Identificador del estudiante.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// DNI del estudiante.
        /// </summary>
        public string Dni { get; set; }

        /// <summary>
        /// Nombre del estudiante.
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Apellido del estudiante.
        /// </summary>
        public string Apellido { get; set; }

        /// <summary>
        /// Email del estudiante.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Nombre y apellido juntos, para mostrar en pantalla.
        /// </summary>
        public string NombreCompleto => $"{Nombre} {Apellido}";
    }
}

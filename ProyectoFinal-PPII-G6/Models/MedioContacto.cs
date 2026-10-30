namespace ProyectoFinal_PPII_G6.Models
{
    /// <summary>
    /// Forma en que el tutor se comunicó con un estudiante.
    /// El nombre de cada valor es el que se guarda en la base de datos: si se agrega uno,
    /// hay que sumarlo también a la restricción CK_Contactos_Medio de crear_base.sql.
    /// </summary>
    public enum MedioContacto
    {
        /// <summary>
        /// Llamada telefónica.
        /// </summary>
        Llamada,

        /// <summary>
        /// Correo electrónico.
        /// </summary>
        Mail,

        /// <summary>
        /// En persona, por ejemplo al terminar la clase.
        /// </summary>
        Presencial,

        /// <summary>
        /// Mensaje de texto o por WhatsApp.
        /// </summary>
        Mensaje
    }
}
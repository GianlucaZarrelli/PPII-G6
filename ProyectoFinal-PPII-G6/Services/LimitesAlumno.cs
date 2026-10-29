namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Largos permitidos para los datos de un alumno. Coinciden con las columnas de la base de datos
    /// y son el único lugar donde están escritos: el alta, la carga masiva y las validaciones los comparten.
    /// </summary>
    public static class LimitesAlumno
    {
        /// <summary>
        /// Cantidad mínima de dígitos de un DNI.
        /// </summary>
        public const int DniMinimo = 6;

        /// <summary>
        /// Cantidad máxima de dígitos de un DNI.
        /// </summary>
        public const int DniMaximo = 9;

        /// <summary>
        /// Largo máximo del nombre.
        /// </summary>
        public const int LargoNombre = 100;

        /// <summary>
        /// Largo máximo del apellido.
        /// </summary>
        public const int LargoApellido = 100;

        /// <summary>
        /// Largo máximo del email.
        /// </summary>
        public const int LargoEmail = 100;
    }
}
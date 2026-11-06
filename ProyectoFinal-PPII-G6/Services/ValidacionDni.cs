namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Reglas del DNI de un alumno, compartidas por el alta manual y la carga masiva.
    /// </summary>
    public static class ValidacionDni
    {
        /// <summary>
        /// Quita puntos y espacios para que "30.123.456" y "30123456" cuenten como el mismo DNI.
        /// </summary>
        public static string Normalizar(string? dni) =>
            (dni ?? string.Empty).Replace(".", string.Empty).Replace(" ", string.Empty).Trim();

        /// <summary>
        /// Devuelve el motivo por el que el DNI no sirve, o null si es válido.
        /// Un DNI válido tiene solo dígitos y entre <see cref="LimitesAlumno.DniMinimo"/> y
        /// <see cref="LimitesAlumno.DniMaximo"/> de ellos, una vez quitados los puntos y los espacios.
        /// </summary>
        public static string? ObtenerError(string? dni)
        {
            var numero = Normalizar(dni);

            if (numero.Length == 0)
                return "El DNI es obligatorio.";

            if (!numero.All(c => c is >= '0' and <= '9'))
                return "El DNI solo puede tener números.";

            if (numero.Length < LimitesAlumno.DniMinimo || numero.Length > LimitesAlumno.DniMaximo)
                return $"El DNI tiene que tener entre {LimitesAlumno.DniMinimo} y {LimitesAlumno.DniMaximo} dígitos.";

            return null;
        }
    }
}
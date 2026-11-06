namespace ProyectoFinal_PPII_G6.Models
{
    /// <summary>
    /// Umbrales con los que se decide el nivel de riesgo de un alumno, como porcentajes de 0 a 100.
    /// Hay una sola configuración para todo el sistema.
    /// </summary>
    public class UmbralesRiesgo
    {
        /// <summary>
        /// Id de la única fila de configuración que existe en la base de datos.
        /// </summary>
        public const int IdUnico = 1;

        /// <summary>
        /// Valor inicial: por debajo de este porcentaje de asistencia, el riesgo es alto.
        /// </summary>
        public const int AsistenciaRojoPorDefecto = 75;

        /// <summary>
        /// Valor inicial: por debajo de este porcentaje de entregas, el riesgo es alto.
        /// </summary>
        public const int EntregasRojoPorDefecto = 60;

        /// <summary>
        /// Valor inicial: por debajo de este porcentaje de asistencia (y desde el umbral rojo), el riesgo es moderado.
        /// </summary>
        public const int AsistenciaAmarilloPorDefecto = 85;

        /// <summary>
        /// Valor inicial: por debajo de este porcentaje de entregas (y desde el umbral rojo), el riesgo es moderado.
        /// </summary>
        public const int EntregasAmarilloPorDefecto = 80;

        /// <summary>
        /// Identificador de la configuración: siempre es <see cref="IdUnico"/>.
        /// </summary>
        public int Id { get; set; } = IdUnico;

        /// <summary>
        /// Por debajo de este porcentaje de asistencia, el riesgo es alto.
        /// </summary>
        public int AsistenciaRojo { get; set; }

        /// <summary>
        /// Por debajo de este porcentaje de entregas, el riesgo es alto.
        /// </summary>
        public int EntregasRojo { get; set; }

        /// <summary>
        /// Por debajo de este porcentaje de asistencia (y desde el umbral rojo), el riesgo es moderado.
        /// </summary>
        public int AsistenciaAmarillo { get; set; }

        /// <summary>
        /// Por debajo de este porcentaje de entregas (y desde el umbral rojo), el riesgo es moderado.
        /// </summary>
        public int EntregasAmarillo { get; set; }

        /// <summary>
        /// Constructor vacío requerido por Entity Framework.
        /// </summary>
        public UmbralesRiesgo() { }

        /// <summary>
        /// Crea los umbrales con sus cuatro valores.
        /// </summary>
        public UmbralesRiesgo(int asistenciaRojo, int entregasRojo, int asistenciaAmarillo, int entregasAmarillo)
        {
            AsistenciaRojo = asistenciaRojo;
            EntregasRojo = entregasRojo;
            AsistenciaAmarillo = asistenciaAmarillo;
            EntregasAmarillo = entregasAmarillo;
        }

        /// <summary>
        /// Los valores con los que arranca el sistema, y a los que se vuelve al restablecer.
        /// </summary>
        public static UmbralesRiesgo PorDefecto() => new(
            AsistenciaRojoPorDefecto, EntregasRojoPorDefecto, AsistenciaAmarilloPorDefecto, EntregasAmarilloPorDefecto);
    }
}
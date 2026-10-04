namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Niveles de riesgo de un estudiante según sus inasistencias.
    /// </summary>
    public enum NivelRiesgo
    {
        /// <summary>Sin inasistencias.</summary>
        Bajo,

        /// <summary>1 o 2 inasistencias.</summary>
        Moderado,

        /// <summary>3 o más inasistencias.</summary>
        Alto
    }

    /// <summary>
    /// Define el cálculo del nivel de riesgo de un estudiante.
    /// </summary>
    public interface ICalculadorRiesgoService
    {
        /// <summary>
        /// Devuelve el nivel de riesgo según la cantidad de inasistencias.
        /// </summary>
        NivelRiesgo CalcularNivelRiesgo(int inasistencias);
    }

    /// <summary>
    /// Calcula el nivel de riesgo: 3 o más inasistencias es Alto, 1 o 2 es Moderado y 0 es Bajo.
    /// </summary>
    public class CalculadorRiesgoService : ICalculadorRiesgoService
    {
        /// <summary>
        /// Devuelve el nivel de riesgo según la cantidad de inasistencias.
        /// </summary>
        public NivelRiesgo CalcularNivelRiesgo(int inasistencias)
        {
            if (inasistencias >= 3)
                return NivelRiesgo.Alto;
            if (inasistencias >= 1)
                return NivelRiesgo.Moderado;

            return NivelRiesgo.Bajo;
        }
    }
}

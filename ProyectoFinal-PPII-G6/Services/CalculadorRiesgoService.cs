using ProyectoFinal_PPII_G6.Models;
using System.Collections.Generic;
using System.Linq;

namespace ProyectoFinal_PPII_G6.Services
{
    public enum NivelRiesgo
    {
        Bajo,
        Medio,
        Alto
    }

    public class CalculadorRiesgoService
    {
        // Regla de negocio para la alerta temprana (H6):
        // - 25% o más de inasistencias = Riesgo Alto
        // - Entre 15% y 24.9% = Riesgo Medio
        // - Menos de 15% = Riesgo Bajo
        public NivelRiesgo CalcularRiesgo(List<Asistencia> asistenciasEstudiante)
        {
            if (asistenciasEstudiante == null || asistenciasEstudiante.Count == 0)
            {
                return NivelRiesgo.Bajo;
            }

            int totalClases = asistenciasEstudiante.Count;
            int ausencias = 0;

            foreach (var asistencia in asistenciasEstudiante)
            {
                if (!asistencia.Presente)
                {
                    ausencias++;
                }
            }

            double porcentaje = ((double)ausencias / totalClases) * 100;

            if (porcentaje >= 25)
            {
                return NivelRiesgo.Alto;
            }
            if (porcentaje >= 15)
            {
                return NivelRiesgo.Medio;
            }

            return NivelRiesgo.Bajo;
        }

        public double CalcularPorcentajeAusencias(List<Asistencia> asistenciasEstudiante)
        {
            if (asistenciasEstudiante == null || asistenciasEstudiante.Count == 0)
            {
                return 0;
            }

            int ausencias = 0;
            foreach (var asistencia in asistenciasEstudiante)
            {
                if (!asistencia.Presente)
                {
                    ausencias++;
                }
            }

            return ((double)ausencias / asistenciasEstudiante.Count) * 100;
        }
    }
}

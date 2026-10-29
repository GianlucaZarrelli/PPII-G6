using System;
using System.Collections.Generic;
using System.Linq;
using ProyectoFinal_PPII_G6.Models;

namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Nivel de riesgo académico de un estudiante.
    /// </summary>
    public enum NivelRiesgoEnum
    {
        /// <summary>
        /// Riesgo alto.
        /// </summary>
        Rojo,
        /// <summary>
        /// Riesgo medio.
        /// </summary>
        Amarillo,
        /// <summary>
        /// Sin riesgo.
        /// </summary>
        Verde
    }

    /// <summary>
    /// Resultado del cálculo de riesgo de un estudiante.
    /// </summary>
    public class ResultadoRiesgo
    {
        /// <summary>
        /// Porcentaje de asistencia efectiva (0 a 100).
        /// </summary>
        public double PorcentajeAsistencia;
        /// <summary>
        /// Porcentaje de trabajos prácticos entregados (0 a 100).
        /// </summary>
        public double PorcentajeEntregas;
        /// <summary>
        /// Nivel de riesgo resultante.
        /// </summary>
        public NivelRiesgoEnum Nivel { get; set; }
    }

    /// <summary>
    /// Calcula el nivel de riesgo de un estudiante según su asistencia y sus entregas.
    /// </summary>
    public class RiesgoService
    {
        /// <summary>
        /// Peso de una ausencia justificada en el cálculo de asistencia.
        /// </summary>
        private const double ValorAusenciaJustificada = 0.5;

        /// <summary>
        /// Por debajo de este porcentaje de asistencia, el riesgo es alto.
        /// </summary>
        private const double UmbralAsistenciaRojo = 75;

        /// <summary>
        /// Por debajo de este porcentaje de entregas, el riesgo es alto.
        /// </summary>
        private const double UmbralEntregasRojo = 60;

        /// <summary>
        /// Por debajo de este porcentaje de asistencia (y desde el umbral rojo), el riesgo es medio.
        /// </summary>
        private const double UmbralAsistenciaAmarillo = 85;

        /// <summary>
        /// Por debajo de este porcentaje de entregas (y desde el umbral rojo), el riesgo es medio.
        /// </summary>
        private const double UmbralEntregasAmarillo = 80;

        /// <summary>
        /// Calcula los porcentajes de asistencia y entregas y el nivel de riesgo: Rojo si la asistencia es menor a 75% o las entregas a 60%;
        /// Amarillo si la asistencia es menor a 85% o las entregas a 80%; Verde en otro caso. Sin clases o TPs, el porcentaje es 100%.
        /// </summary>
        public ResultadoRiesgo CalcularRiesgo(
            List<Asistencia> asistenciasEstudiante,
            int totalClasesComision,
            List<Entrega> entregasEstudiante,
            int totalTpsComision)
        {
            var asistencia = CalcularPorcentajeAsistencia(asistenciasEstudiante, totalClasesComision);
            var entregas = CalcularPorcentajeEntregas(entregasEstudiante, totalTpsComision);

            return new ResultadoRiesgo
            {
                PorcentajeAsistencia = asistencia,
                PorcentajeEntregas = entregas,
                Nivel = ElegirNivel(asistencia, entregas)
            };
        }

        /// <summary>
        /// Porcentaje de asistencia efectiva: una presente vale 1 y una ausencia justificada vale la mitad.
        /// Sin clases, no hay nada que penalizar y el resultado es 100.
        /// </summary>
        private static double CalcularPorcentajeAsistencia(List<Asistencia> asistencias, int totalClases)
        {
            if (totalClases <= 0) return 100;

            int presentes = asistencias.Count(a => a.Presente);
            int justificadas = asistencias.Count(a => !a.Presente && a.Justificada);
            double asistenciaEfectiva = presentes + justificadas * ValorAusenciaJustificada;

            return (asistenciaEfectiva / totalClases) * 100;
        }

        /// <summary>
        /// Porcentaje de trabajos entregados. Sin trabajos, no hay nada que penalizar y el resultado es 100.
        /// </summary>
        private static double CalcularPorcentajeEntregas(List<Entrega> entregas, int totalTps)
        {
            if (totalTps <= 0) return 100;

            int entregados = entregas.Count(e => e.Entregado);

            return ((double)entregados / totalTps) * 100;
        }

        /// <summary>
        /// Rojo si cualquiera de los dos porcentajes cae bajo su umbral rojo; si no, amarillo si cae bajo el amarillo; si no, verde.
        /// </summary>
        private static NivelRiesgoEnum ElegirNivel(double asistencia, double entregas)
        {
            if (asistencia < UmbralAsistenciaRojo || entregas < UmbralEntregasRojo)
                return NivelRiesgoEnum.Rojo;

            if (asistencia < UmbralAsistenciaAmarillo || entregas < UmbralEntregasAmarillo)
                return NivelRiesgoEnum.Amarillo;

            return NivelRiesgoEnum.Verde;
        }
    }

}
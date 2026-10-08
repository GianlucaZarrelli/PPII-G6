using System;
using System.Collections.Generic;
using System.Linq;
using ProyectoFinal_PPII_G6.Models;

namespace ProyectoFinal_PPII_G6.Services
{
    public enum NivelRiesgoEnum
    {
        Rojo,
        Amarillo,
        Verde
    }

    public class ResultadoRiesgo
    {
        public double PorcentajeAsistencia;
        public double PorcentajeEntregas;
        public NivelRiesgoEnum Nivel { get; set; }
    }

    public class RiesgoService
    {
        public ResultadoRiesgo CalcularRiesgo(
            List<Asistencia> asistenciasEstudiante,
            int totalClasesComision,
            List<Entrega> entregasEstudiante,
            int totalTpsComision)
        {
            var resultado = new ResultadoRiesgo();

            if (totalClasesComision > 0)
            {
                int presentes = asistenciasEstudiante.Count(a => a.Presente == true);
                resultado.PorcentajeAsistencia = ((double)presentes / totalClasesComision) * 100;
            }
            else
            {
                resultado.PorcentajeAsistencia = 100;
            }

            if (totalTpsComision > 0)
            {
                int entregados = entregasEstudiante.Count(e => e.Entregado);
                resultado.PorcentajeEntregas = ((double)entregados / totalTpsComision) * 100;
            }
            else
            {
                resultado.PorcentajeEntregas = 100;
            }

            if (resultado.PorcentajeAsistencia < 75 || resultado.PorcentajeEntregas < 60)
            {
                resultado.Nivel = NivelRiesgoEnum.Rojo;
            } else if (resultado.PorcentajeAsistencia < 85 || resultado.PorcentajeEntregas < 80)
            {
                resultado.Nivel = NivelRiesgoEnum.Amarillo;
            } else
            {
                resultado.Nivel = NivelRiesgoEnum.Verde;
            }

            return resultado;
        }
    }

}

using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;
using System.Collections.Generic;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    public class CalculadorRiesgoServiceTests
    {
        [Fact]
        // Se crea una lista de ausencias con un porcentaje mayor o igual al 25%.
        public void CalcularRiesgo_AusenciasMayorOIgualAl25PorCiento_DevuelveRiesgoAlto()
        {
            var servicio = new CalculadorRiesgoService();
            var asistencias = new List<Asistencia>
            {
                new Asistencia(1, 1, 1, false), // Ausencia
                new Asistencia(2, 1, 2, true), // Presente
                new Asistencia(3, 1, 3, true), // Presente
                new Asistencia(4, 1, 4, true) // Presente
            };

            // Ejecucion del método a probar

            NivelRiesgo resultado = servicio.CalcularRiesgo(asistencias);


            // Se verifica que el resultado sea Riesgo Alto
            Assert.Equal(NivelRiesgo.Alto, resultado);
        }

        [Fact]
        public void CalcularRiesgo_AusenciasMenoresAl15PorCiento_DevuelveRiesgoBajo()
        {
            // 100% asistencia (0% ausencias)
            var servicio = new CalculadorRiesgoService();
            var asistencias = new List<Asistencia>
            {
                new Asistencia(1, 1, 1, true),
                new Asistencia(2, 1, 2, true),
                new Asistencia(3, 1, 3, true),
                new Asistencia(4, 1, 4, true),
                new Asistencia(5, 1, 5, true)
            };

            // Ejecución
            NivelRiesgo resultado = servicio.CalcularRiesgo(asistencias);

            // Verificación
            Assert.Equal(NivelRiesgo.Bajo, resultado);
        }
    }
}

using Xunit;
using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas unitarias del cálculo del nivel de riesgo.
    /// </summary>
    public class CalculadorRiesgoServiceTests
    {
        /// <summary>
        /// Servicio que se prueba.
        /// </summary>
        private readonly CalculadorRiesgoService _service;

        /// <summary>
        /// Crea una instancia nueva del servicio para cada prueba.
        /// </summary>
        public CalculadorRiesgoServiceTests()
        {
            _service = new CalculadorRiesgoService();
        }

        /// <summary>
        /// Verifica que cada cantidad de inasistencias devuelva el nivel de riesgo correcto.
        /// </summary>
        [Theory]
        [InlineData(0, NivelRiesgo.Bajo)]
        [InlineData(1, NivelRiesgo.Moderado)]
        [InlineData(2, NivelRiesgo.Moderado)]
        [InlineData(3, NivelRiesgo.Alto)]
        [InlineData(5, NivelRiesgo.Alto)]
        public void CalcularNivelRiesgo_RetornaNivelCorrecto(int inasistencias, NivelRiesgo nivelEsperado)
        {
            // Act
            var resultado = _service.CalcularNivelRiesgo(inasistencias);

            // Assert
            Assert.Equal(nivelEsperado, resultado);
        }
    }
}

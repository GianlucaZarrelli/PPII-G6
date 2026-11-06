using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas de las reglas de los umbrales de riesgo: son porcentajes y el de alto riesgo es menor que el moderado.
    /// </summary>
    public class ValidacionUmbralesTests
    {
        [Fact]
        public void ObtenerError_ValoresPorDefecto_RetornaNull()
        {
            // Act
            var error = ValidacionUmbrales.ObtenerError(UmbralesRiesgo.PorDefecto());

            // Assert
            Assert.Null(error);
        }

        [Theory]
        [InlineData(50, 40, 90, 70)]
        [InlineData(0, 0, 1, 1)]
        [InlineData(99, 99, 100, 100)]
        public void ObtenerError_PorcentajesCoherentes_RetornaNull(int asistenciaRojo, int entregasRojo, int asistenciaAmarillo, int entregasAmarillo)
        {
            // Act
            var error = ValidacionUmbrales.ObtenerError(new UmbralesRiesgo(asistenciaRojo, entregasRojo, asistenciaAmarillo, entregasAmarillo));

            // Assert
            Assert.Null(error);
        }

        [Theory]
        [InlineData(-1, 60, 85, 80)]
        [InlineData(75, -5, 85, 80)]
        [InlineData(75, 60, 101, 80)]
        [InlineData(75, 60, 85, 150)]
        public void ObtenerError_ValorFueraDeCeroACien_IndicaElRango(int asistenciaRojo, int entregasRojo, int asistenciaAmarillo, int entregasAmarillo)
        {
            // Act
            var error = ValidacionUmbrales.ObtenerError(new UmbralesRiesgo(asistenciaRojo, entregasRojo, asistenciaAmarillo, entregasAmarillo));

            // Assert
            Assert.Equal("Los umbrales son porcentajes: tienen que estar entre 0 y 100.", error);
        }

        [Theory]
        [InlineData(85, 85)]
        [InlineData(90, 85)]
        public void ObtenerError_AsistenciaConAltoRiesgoNoMenorQueModerado_AvisaDeLaAsistencia(int rojo, int amarillo)
        {
            // Act
            var error = ValidacionUmbrales.ObtenerError(new UmbralesRiesgo(rojo, 60, amarillo, 80));

            // Assert
            Assert.Equal("En la asistencia, el umbral de alto riesgo tiene que ser menor que el de riesgo moderado.", error);
        }

        [Theory]
        [InlineData(80, 80)]
        [InlineData(90, 80)]
        public void ObtenerError_EntregasConAltoRiesgoNoMenorQueModerado_AvisaDeLasEntregas(int rojo, int amarillo)
        {
            // Act
            var error = ValidacionUmbrales.ObtenerError(new UmbralesRiesgo(75, rojo, 85, amarillo));

            // Assert
            Assert.Equal("En las entregas, el umbral de alto riesgo tiene que ser menor que el de riesgo moderado.", error);
        }

        [Fact]
        public void ObtenerError_TodoIncorrecto_ReportaPrimeroElRango()
        {
            // Act: se informa un problema por vez, empezando por el rango
            var error = ValidacionUmbrales.ObtenerError(new UmbralesRiesgo(200, 90, 85, 80));

            // Assert
            Assert.StartsWith("Los umbrales son porcentajes", error);
        }

        [Fact]
        public void ObtenerError_AsistenciaYEntregasIncoherentes_ReportaPrimeroLaAsistencia()
        {
            // Act
            var error = ValidacionUmbrales.ObtenerError(new UmbralesRiesgo(90, 90, 85, 80));

            // Assert
            Assert.StartsWith("En la asistencia", error);
        }

        [Fact]
        public void PorDefecto_SonLosValoresDeSiempre()
        {
            // Act
            var umbrales = UmbralesRiesgo.PorDefecto();

            // Assert
            Assert.Equal(75, umbrales.AsistenciaRojo);
            Assert.Equal(60, umbrales.EntregasRojo);
            Assert.Equal(85, umbrales.AsistenciaAmarillo);
            Assert.Equal(80, umbrales.EntregasAmarillo);
        }
    }
}
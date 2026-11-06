using ProyectoFinal_PPII_G6.Services;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas de las reglas de una clase del calendario: la fecha (dentro del año, puede ser futura) y la descripción.
    /// </summary>
    public class ValidacionClaseTests
    {
        private static readonly DateTime Hoy = new(2026, 10, 30);

        [Theory]
        [InlineData(2026, 1, 1)]
        [InlineData(2026, 10, 30)]
        [InlineData(2026, 11, 20)]
        [InlineData(2026, 12, 31)]
        public void ObtenerError_FechaDentroDelAnio_EsValidaAunqueSeaFutura(int anio, int mes, int dia)
        {
            // Act
            var error = ValidacionClase.ObtenerError(new DateTime(anio, mes, dia), "Introducción", Hoy);

            // Assert
            Assert.Null(error);
        }

        [Theory]
        [InlineData(2025, 12, 31)]
        [InlineData(2027, 1, 1)]
        public void ObtenerError_FechaFueraDelAnio_IndicaElRangoPermitido(int anio, int mes, int dia)
        {
            // Act
            var error = ValidacionClase.ObtenerError(new DateTime(anio, mes, dia), "Introducción", Hoy);

            // Assert
            Assert.Equal("La fecha de la clase tiene que estar entre el 01/01/2026 y el 31/12/2026.", error);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ObtenerError_SinDescripcion_EsValidaPorqueEsOpcional(string? descripcion)
        {
            // Act
            var error = ValidacionClase.ObtenerError(Hoy, descripcion, Hoy);

            // Assert
            Assert.Null(error);
        }

        [Fact]
        public void ObtenerError_DescripcionDeExactamenteElLimite_EsValida()
        {
            // Arrange
            var descripcion = new string('a', ValidacionClase.LargoMaximoDescripcion);

            // Act
            var error = ValidacionClase.ObtenerError(Hoy, descripcion, Hoy);

            // Assert
            Assert.Null(error);
        }

        [Fact]
        public void ObtenerError_DescripcionDeMasDelLimite_IndicaElLimite()
        {
            // Arrange
            var descripcion = new string('a', ValidacionClase.LargoMaximoDescripcion + 1);

            // Act
            var error = ValidacionClase.ObtenerError(Hoy, descripcion, Hoy);

            // Assert
            Assert.Equal("La descripción admite hasta 200 caracteres.", error);
        }

        [Fact]
        public void ObtenerError_FechaYDescripcionIncorrectas_ReportaPrimeroLaFecha()
        {
            // Arrange
            var descripcion = new string('a', ValidacionClase.LargoMaximoDescripcion + 1);

            // Act
            var error = ValidacionClase.ObtenerError(new DateTime(2027, 3, 1), descripcion, Hoy);

            // Assert
            Assert.StartsWith("La fecha de la clase", error);
        }
    }
}
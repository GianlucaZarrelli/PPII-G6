using ProyectoFinal_PPII_G6.Services;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas unitarias de las reglas del título y de la fecha de entrega de un trabajo práctico.
    /// </summary>
    public class ValidacionTrabajoTests
    {
        private static readonly DateTime Hoy = new(2026, 10, 29);

        [Theory]
        [InlineData(2026, 10, 30)]
        [InlineData(2026, 1, 1)]
        [InlineData(2026, 12, 31)]
        [InlineData(2026, 3, 15)]
        public void ObtenerError_FechaDentroDelAnio_RetornaNull(int anio, int mes, int dia)
        {
            // Act: se aceptan los dos extremos del año y también fechas pasadas del mismo año
            var error = ValidacionTrabajo.ObtenerError("TP 1", new DateTime(anio, mes, dia), Hoy);

            // Assert
            Assert.Null(error);
        }

        [Theory]
        [InlineData(2025, 12, 31)]
        [InlineData(2027, 1, 1)]
        [InlineData(2062, 5, 5)]
        public void ObtenerError_FechaFueraDelAnio_IndicaElRango(int anio, int mes, int dia)
        {
            // Act
            var error = ValidacionTrabajo.ObtenerError("TP 1", new DateTime(anio, mes, dia), Hoy);

            // Assert: el 2062 es el error de tipeo que esta regla evita
            Assert.Equal("La fecha de entrega tiene que estar entre el 01/01/2026 y el 31/12/2026.", error);
        }

        [Fact]
        public void ObtenerError_FechaConHora_SoloCuentaElDia()
        {
            // Act: el último día del año a las 23:59 sigue siendo ese día
            var error = ValidacionTrabajo.ObtenerError("TP 1", new DateTime(2026, 12, 31, 23, 59, 0), Hoy);

            // Assert
            Assert.Null(error);
        }

        [Fact]
        public void ObtenerError_ElRangoAcompaniaAlAnioDeHoy()
        {
            // Act: con "hoy" en 2027, el 31/12/2026 ya queda fuera
            var error = ValidacionTrabajo.ObtenerError("TP 1", new DateTime(2026, 12, 31), new DateTime(2027, 2, 1));

            // Assert
            Assert.Equal("La fecha de entrega tiene que estar entre el 01/01/2027 y el 31/12/2027.", error);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ObtenerError_TituloVacio_PideElTitulo(string? titulo)
        {
            // Act
            var error = ValidacionTrabajo.ObtenerError(titulo, new DateTime(2026, 10, 30), Hoy);

            // Assert
            Assert.Equal("El título del trabajo es obligatorio.", error);
        }

        [Fact]
        public void ObtenerError_TituloDeMasDeCientoCincuentaCaracteres_IndicaElLimite()
        {
            // Arrange
            var titulo = new string('a', ValidacionTrabajo.LargoMaximoTitulo + 1);

            // Act
            var error = ValidacionTrabajo.ObtenerError(titulo, new DateTime(2026, 10, 30), Hoy);

            // Assert
            Assert.Equal("El título admite hasta 150 caracteres.", error);
        }

        [Fact]
        public void ObtenerError_TituloDeExactamenteElLimite_EsValido()
        {
            // Arrange
            var titulo = new string('a', ValidacionTrabajo.LargoMaximoTitulo);

            // Act
            var error = ValidacionTrabajo.ObtenerError(titulo, new DateTime(2026, 10, 30), Hoy);

            // Assert
            Assert.Null(error);
        }

        [Fact]
        public void ObtenerError_TituloYFechaIncorrectos_ReportaPrimeroElTitulo()
        {
            // Act
            var error = ValidacionTrabajo.ObtenerError("", new DateTime(2062, 5, 5), Hoy);

            // Assert: se informa un problema por vez, en el mismo orden en que se completa el formulario
            Assert.Equal("El título del trabajo es obligatorio.", error);
        }
    }
}
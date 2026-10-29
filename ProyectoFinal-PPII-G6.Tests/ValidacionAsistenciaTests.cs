using ProyectoFinal_PPII_G6.Services;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas unitarias de la regla de la fecha del registro de asistencia.
    /// </summary>
    public class ValidacionAsistenciaTests
    {
        private static readonly DateTime Hoy = new(2026, 10, 29);

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-300)]
        public void ObtenerErrorDeFecha_HoyOUnaFechaPasada_RetornaNull(int diasDesdeHoy)
        {
            // Act
            var error = ValidacionAsistencia.ObtenerErrorDeFecha(Hoy.AddDays(diasDesdeHoy), Hoy);

            // Assert
            Assert.Null(error);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(30)]
        public void ObtenerErrorDeFecha_FechaFutura_NoSePermite(int diasDesdeHoy)
        {
            // Act
            var error = ValidacionAsistencia.ObtenerErrorDeFecha(Hoy.AddDays(diasDesdeHoy), Hoy);

            // Assert
            Assert.Equal("No se puede registrar asistencia de una fecha futura.", error);
        }

        [Fact]
        public void ObtenerErrorDeFecha_ConHora_SoloCuentaElDia()
        {
            // Arrange: a las 23:59 de hoy, la fecha de hoy sigue siendo válida y la de mañana no
            var casiMedianoche = new DateTime(2026, 10, 29, 23, 59, 0);

            // Act
            var hoyConHora = ValidacionAsistencia.ObtenerErrorDeFecha(new DateTime(2026, 10, 29), casiMedianoche);
            var manianaTemprano = ValidacionAsistencia.ObtenerErrorDeFecha(new DateTime(2026, 10, 30), casiMedianoche);

            // Assert
            Assert.Null(hoyConHora);
            Assert.NotNull(manianaTemprano);
        }
    }
}
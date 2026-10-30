using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas unitarias de las reglas del registro de un contacto con un estudiante.
    /// </summary>
    public class ValidacionContactoTests
    {
        private static readonly DateTime Ahora = new(2026, 10, 29, 15, 30, 0);

        [Fact]
        public void ObtenerError_ContactoCompletoDeHoy_RetornaNull()
        {
            // Act
            var error = ValidacionContacto.ObtenerError(Ahora.AddHours(-3), MedioContacto.Llamada, "Atendió y prometió entregar el TP.", Ahora);

            // Assert
            Assert.Null(error);
        }

        [Fact]
        public void ObtenerError_ContactoDeEsteMismoMomento_EsValido()
        {
            // Act: el límite es inclusivo: se puede registrar un contacto recién hecho
            var error = ValidacionContacto.ObtenerError(Ahora, MedioContacto.Mail, "Se envió el mail.", Ahora);

            // Assert
            Assert.Null(error);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(1440)]
        public void ObtenerError_FechaYHoraFuturas_NoSePermiten(int minutosEnElFuturo)
        {
            // Act
            var error = ValidacionContacto.ObtenerError(Ahora.AddMinutes(minutosEnElFuturo), MedioContacto.Llamada, "Resultado.", Ahora);

            // Assert
            Assert.Equal("La fecha y la hora del contacto no pueden ser futuras.", error);
        }

        [Fact]
        public void ObtenerError_SinMedio_PideElMedio()
        {
            // Act
            var error = ValidacionContacto.ObtenerError(Ahora.AddHours(-1), null, "Resultado.", Ahora);

            // Assert
            Assert.Equal("Elegí el medio de contacto.", error);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ObtenerError_SinResultado_PideElResultado(string? resultado)
        {
            // Act
            var error = ValidacionContacto.ObtenerError(Ahora.AddHours(-1), MedioContacto.Presencial, resultado, Ahora);

            // Assert
            Assert.Equal("El resultado del contacto es obligatorio.", error);
        }

        [Fact]
        public void ObtenerError_ResultadoDeMasDeQuinientosCaracteres_IndicaElLimite()
        {
            // Arrange
            var resultado = new string('a', ValidacionContacto.LargoMaximoResultado + 1);

            // Act
            var error = ValidacionContacto.ObtenerError(Ahora.AddHours(-1), MedioContacto.Mensaje, resultado, Ahora);

            // Assert
            Assert.Equal("El resultado admite hasta 500 caracteres.", error);
        }

        [Fact]
        public void ObtenerError_ResultadoDeExactamenteElLimiteConEspacios_EsValido()
        {
            // Arrange: los espacios de los extremos no cuentan para el largo
            var resultado = "  " + new string('a', ValidacionContacto.LargoMaximoResultado) + "  ";

            // Act
            var error = ValidacionContacto.ObtenerError(Ahora.AddHours(-1), MedioContacto.Mensaje, resultado, Ahora);

            // Assert
            Assert.Null(error);
        }

        [Fact]
        public void ObtenerError_TodoIncorrecto_ReportaPrimeroLaFecha()
        {
            // Act: se informa un problema por vez, en el orden del formulario
            var error = ValidacionContacto.ObtenerError(Ahora.AddDays(1), null, "", Ahora);

            // Assert
            Assert.Equal("La fecha y la hora del contacto no pueden ser futuras.", error);
        }

        [Fact]
        public void ObtenerError_SinMedioNiResultado_ReportaPrimeroElMedio()
        {
            // Act
            var error = ValidacionContacto.ObtenerError(Ahora.AddHours(-1), null, "", Ahora);

            // Assert
            Assert.Equal("Elegí el medio de contacto.", error);
        }

        [Fact]
        public void ObtenerError_CadaMedioDeContacto_EsValido()
        {
            // Act
            var errores = Enum.GetValues<MedioContacto>()
                .Select(medio => ValidacionContacto.ObtenerError(Ahora.AddHours(-1), medio, "Resultado.", Ahora))
                .ToList();

            // Assert
            Assert.All(errores, error => Assert.Null(error));
        }

        [Fact]
        public void MedioContacto_LosNombresCoincidenConLaRestriccionDeLaBase()
        {
            // Act: estos nombres son los que se guardan en la columna Medio y los que permite
            // la restricción CK_Contactos_Medio de crear_base.sql
            var nombres = Enum.GetNames<MedioContacto>();

            // Assert: si se agrega un medio, esta prueba obliga a actualizar también el script
            Assert.Equal(new[] { "Llamada", "Mail", "Presencial", "Mensaje" }, nombres);
        }
    }
}
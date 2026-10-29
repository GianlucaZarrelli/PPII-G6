using ProyectoFinal_PPII_G6.Services;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas unitarias de las reglas del DNI de un alumno.
    /// </summary>
    public class ValidacionDniTests
    {
        [Theory]
        [InlineData("30.123.456", "30123456")]
        [InlineData(" 30 123 456 ", "30123456")]
        [InlineData("30123456", "30123456")]
        [InlineData(null, "")]
        public void Normalizar_QuitaPuntosYEspacios(string? dni, string esperado)
        {
            // Act
            var resultado = ValidacionDni.Normalizar(dni);

            // Assert
            Assert.Equal(esperado, resultado);
        }

        [Theory]
        [InlineData("30123456")]
        [InlineData("30.123.456")]
        [InlineData("  30123456  ")]
        [InlineData("1234567")]
        public void ObtenerError_DniValido_RetornaNull(string dni)
        {
            // Act
            var error = ValidacionDni.ObtenerError(dni);

            // Assert: puntos, espacios y 7 u 8 dígitos son válidos
            Assert.Null(error);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ObtenerError_DniVacio_PideElDni(string? dni)
        {
            // Act
            var error = ValidacionDni.ObtenerError(dni);

            // Assert
            Assert.Equal("El DNI es obligatorio.", error);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("3012345a")]
        [InlineData("30-123-456")]
        public void ObtenerError_DniConLetrasOSimbolos_PideSoloNumeros(string dni)
        {
            // Act
            var error = ValidacionDni.ObtenerError(dni);

            // Assert: antes "abc" se aceptaba y fallaba recién al guardar
            Assert.Equal("El DNI solo puede tener números.", error);
        }

        [Theory]
        [InlineData("123")]
        [InlineData("12345")]
        [InlineData("1234567890")]
        [InlineData("123456789012345678901")]
        public void ObtenerError_DniDeLargoIncorrecto_IndicaElRango(string dni)
        {
            // Act
            var error = ValidacionDni.ObtenerError(dni);

            // Assert: antes un DNI largo terminaba en un mensaje que hablaba de DNI repetido
            Assert.Equal("El DNI tiene que tener entre 6 y 9 dígitos.", error);
        }
    }
}
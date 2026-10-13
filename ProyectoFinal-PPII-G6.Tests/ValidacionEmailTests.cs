using ProyectoFinal_PPII_G6.Services;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas unitarias de la validación del email de un alumno.
    /// </summary>
    public class ValidacionEmailTests
    {
        [Theory]
        [InlineData("ana@demo.test")]
        [InlineData("ana.ramirez@demo.test")]
        [InlineData("  lucia.fernandez@gmail.com  ")]
        public void EsValido_EmailConFormatoCorrecto_RetornaTrue(string email)
        {
            // Act
            var resultado = ValidacionEmail.EsValido(email);

            // Assert: los espacios de los costados no invalidan el email
            Assert.True(resultado);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("ana")]
        [InlineData("ana@")]
        [InlineData("@demo.test")]
        [InlineData("ana@demo")]
        [InlineData("Ana <ana@demo.test>")]
        public void EsValido_EmailVacioOMalFormado_RetornaFalse(string? email)
        {
            // Act
            var resultado = ValidacionEmail.EsValido(email);

            // Assert
            Assert.False(resultado);
        }

        [Fact]
        public void EsValido_EmailMasLargoQueLaColumna_RetornaFalse()
        {
            // Arrange: 105 caracteres, y la columna Email admite 100
            var email = new string('a', 95) + "@demo.test";

            // Act
            var resultado = ValidacionEmail.EsValido(email);

            // Assert
            Assert.False(resultado);
        }
    }
}
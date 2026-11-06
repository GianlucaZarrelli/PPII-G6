using ProyectoFinal_PPII_G6.Services;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas de las reglas del nombre de una comisión nueva: obligatorio, que entre en la base y que no esté repetido.
    /// </summary>
    public class ValidacionComisionTests
    {
        private static readonly string[] Existentes = { "A", "B", "C" };

        [Theory]
        [InlineData("D")]
        [InlineData("  D  ")]
        [InlineData("Turno noche")]
        public void ObtenerError_NombreNuevo_RetornaNull(string nombre)
        {
            // Act
            var error = ValidacionComision.ObtenerError(nombre, Existentes);

            // Assert
            Assert.Null(error);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ObtenerError_SinNombre_PideElNombre(string? nombre)
        {
            // Act
            var error = ValidacionComision.ObtenerError(nombre, Existentes);

            // Assert
            Assert.Equal("El nombre de la comisión es obligatorio.", error);
        }

        [Fact]
        public void ObtenerError_NombreDeExactamenteElLimiteConEspacios_EsValido()
        {
            // Arrange: los espacios de los extremos no cuentan para el largo
            var nombre = " " + new string('a', ValidacionComision.LargoMaximoNombre) + " ";

            // Act
            var error = ValidacionComision.ObtenerError(nombre, Existentes);

            // Assert
            Assert.Null(error);
        }

        [Fact]
        public void ObtenerError_NombreDeMasDelLimite_IndicaElLimite()
        {
            // Arrange
            var nombre = new string('a', ValidacionComision.LargoMaximoNombre + 1);

            // Act
            var error = ValidacionComision.ObtenerError(nombre, Existentes);

            // Assert
            Assert.Equal("El nombre de la comisión admite hasta 100 caracteres.", error);
        }

        [Theory]
        [InlineData("A")]
        [InlineData("a")]
        [InlineData("  b  ")]
        public void ObtenerError_NombreRepetido_AvisaSinImportarMayusculasNiEspacios(string nombre)
        {
            // Act
            var error = ValidacionComision.ObtenerError(nombre, Existentes);

            // Assert
            Assert.Equal("Ya existe una comisión con ese nombre.", error);
        }

        [Fact]
        public void ObtenerError_SinComisionesExistentes_CualquierNombreValidoSirve()
        {
            // Act
            var error = ValidacionComision.ObtenerError("A", Array.Empty<string>());

            // Assert
            Assert.Null(error);
        }
    }
}
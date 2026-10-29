using ProyectoFinal_PPII_G6.Services;
using ProyectoFinal_PPII_G6.Presentacion;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Verifica que cada nivel tenga su propio texto y color, para que las pantallas no vuelvan a desincronizarse.
    /// </summary>
    public class PresentacionNivelTests
    {
        [Fact]
        public void CadaNivelTieneTextoYColorPropios()
        {
            // Arrange
            var niveles = Enum.GetValues<NivelSeguimiento>();

            // Act
            var textos = niveles.Select(PresentacionNivel.Texto).ToList();
            var colores = niveles.Select(PresentacionNivel.Color).ToList();

            // Assert
            Assert.All(textos, t => Assert.False(string.IsNullOrWhiteSpace(t)));
            Assert.Equal(niveles.Length, textos.Distinct().Count());
            Assert.Equal(niveles.Length, colores.Distinct().Count());
        }
    }
}
using ProyectoFinal_PPII_G6.Presentacion;
using ProyectoFinal_PPII_G6.Services;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Verifica que cada estado de una fila de la importación tenga su propio texto y un color válido.
    /// </summary>
    public class PresentacionEstadoFilaTests
    {
        [Fact]
        public void CadaEstadoTieneTextoPropioYColor()
        {
            // Arrange
            var estados = Enum.GetValues<EstadoFila>();

            // Act
            var textos = estados.Select(PresentacionEstadoFila.Texto).ToList();
            var colores = estados.Select(PresentacionEstadoFila.Color).ToList();

            // Assert: los textos no se repiten (los errores comparten el rojo a propósito)
            Assert.All(textos, t => Assert.False(string.IsNullOrWhiteSpace(t)));
            Assert.Equal(estados.Length, textos.Distinct().Count());
            Assert.All(colores, c => Assert.True(c.StartsWith('#')));
        }
    }
}
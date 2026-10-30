using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Presentacion;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Verifica que cada medio de contacto tenga su propio texto para mostrar.
    /// </summary>
    public class PresentacionMedioTests
    {
        [Fact]
        public void CadaMedioTieneUnTextoPropio()
        {
            // Arrange
            var medios = Enum.GetValues<MedioContacto>();

            // Act
            var textos = medios.Select(PresentacionMedio.Texto).ToList();

            // Assert
            Assert.All(textos, t => Assert.False(string.IsNullOrWhiteSpace(t)));
            Assert.Equal(medios.Length, textos.Distinct().Count());
        }

        [Fact]
        public void ElMensajeAclaraQueIncluyeWhatsApp()
        {
            // Act
            var texto = PresentacionMedio.Texto(MedioContacto.Mensaje);

            // Assert
            Assert.Equal("Mensaje (WhatsApp)", texto);
        }
    }
}
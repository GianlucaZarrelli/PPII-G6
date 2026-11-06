using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Presentacion;
using ProyectoFinal_PPII_G6.Services;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas de cómo se muestra el contacto con un alumno: el texto y el color según haya o no contactos y su nivel.
    /// </summary>
    public class PresentacionContactoTests
    {
        private static readonly DateTime Ultimo = new(2026, 10, 28, 10, 30, 0);

        private static ResumenAlumno CrearResumen(NivelSeguimiento nivel, DateTime? ultimo = null, int cantidad = 0) => new()
        {
            Estudiante = new Estudiante(1, "90000004", "Joaquín", "Pérez"),
            Nivel = nivel,
            UltimoContacto = ultimo,
            CantidadContactos = cantidad
        };

        [Fact]
        public void Texto_SinContactos_DiceSinContactar()
        {
            // Act
            var texto = PresentacionContacto.Texto(CrearResumen(NivelSeguimiento.Rojo));

            // Assert
            Assert.Equal("Sin contactar", texto);
        }

        [Fact]
        public void Texto_ConUnContacto_MuestraLaFechaYLaHoraEnSingular()
        {
            // Act
            var texto = PresentacionContacto.Texto(CrearResumen(NivelSeguimiento.Rojo, Ultimo, 1));

            // Assert
            Assert.Equal("Último contacto: 28/10/2026 10:30 (1 contacto)", texto);
        }

        [Fact]
        public void Texto_ConVariosContactos_MuestraElUltimoYLaCantidadEnPlural()
        {
            // Act
            var texto = PresentacionContacto.Texto(CrearResumen(NivelSeguimiento.Amarillo, Ultimo, 3));

            // Assert
            Assert.Equal("Último contacto: 28/10/2026 10:30 (3 contactos)", texto);
        }

        [Theory]
        [InlineData(NivelSeguimiento.Rojo)]
        [InlineData(NivelSeguimiento.Amarillo)]
        [InlineData(NivelSeguimiento.Verde)]
        [InlineData(NivelSeguimiento.SinDatos)]
        public void Color_ConContacto_EsVerdeSeaCualSeaElNivel(NivelSeguimiento nivel)
        {
            // Act
            var color = PresentacionContacto.Color(CrearResumen(nivel, Ultimo, 1));

            // Assert
            Assert.Equal("#388E3C", color);
        }

        [Theory]
        [InlineData(NivelSeguimiento.Rojo)]
        [InlineData(NivelSeguimiento.Amarillo)]
        public void Color_EnRiesgoYSinContactar_EsNaranjaPorqueEsUnPendiente(NivelSeguimiento nivel)
        {
            // Act
            var color = PresentacionContacto.Color(CrearResumen(nivel));

            // Assert
            Assert.Equal("#F57C00", color);
        }

        [Theory]
        [InlineData(NivelSeguimiento.Verde)]
        [InlineData(NivelSeguimiento.SinDatos)]
        public void Color_SinRiesgoYSinContactar_EsGrisPorqueNoHayNadaPendiente(NivelSeguimiento nivel)
        {
            // Act
            var color = PresentacionContacto.Color(CrearResumen(nivel));

            // Assert
            Assert.Equal("#757575", color);
        }

        [Fact]
        public void FechaHora_UsaElFormatoDeLaAplicacion()
        {
            // Act
            var texto = PresentacionContacto.FechaHora(new DateTime(2026, 11, 3, 9, 5, 0));

            // Assert
            Assert.Equal("03/11/2026 09:05", texto);
        }
    }
}
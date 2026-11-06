using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Presentacion;
using ProyectoFinal_PPII_G6.Services;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas de cómo se arma el calendario de una comisión y de cómo se muestra cada evento.
    /// </summary>
    public class CalendarioComisionTests
    {
        private static readonly DateTime Hoy = new(2026, 10, 30);

        [Fact]
        public void Armar_JuntaClasesYTrabajosOrdenadosPorFecha()
        {
            // Arrange
            var clases = new[]
            {
                new Clase(1, new DateTime(2026, 11, 6), "Clase 2", 1),
                new Clase(2, new DateTime(2026, 10, 23), "Clase 1", 1)
            };
            var trabajos = new[] { new TrabajoPractico(1, 1, "TP1", new DateTime(2026, 10, 30)) };

            // Act
            var eventos = CalendarioComision.Armar(clases, trabajos);

            // Assert
            Assert.Equal(new[] { "Clase 1", "TP1", "Clase 2" }, eventos.Select(e => e.Titulo));
            Assert.Equal(new[] { TipoEventoCalendario.Clase, TipoEventoCalendario.Trabajo, TipoEventoCalendario.Clase },
                eventos.Select(e => e.Tipo));
        }

        [Fact]
        public void Armar_ElMismoDia_VaPrimeroLaClaseYDespuesLaEntrega()
        {
            // Arrange
            var clases = new[] { new Clase(1, new DateTime(2026, 10, 30), "Clase", 1) };
            var trabajos = new[] { new TrabajoPractico(1, 1, "TP1", new DateTime(2026, 10, 30)) };

            // Act
            var eventos = CalendarioComision.Armar(clases, trabajos);

            // Assert
            Assert.Equal(new[] { TipoEventoCalendario.Clase, TipoEventoCalendario.Trabajo }, eventos.Select(e => e.Tipo));
        }

        [Fact]
        public void Armar_UnaClaseSinDescripcion_MuestraElTextoPorDefecto()
        {
            // Arrange
            var clases = new[] { new Clase(1, new DateTime(2026, 10, 30), "  ", 1) };

            // Act
            var eventos = CalendarioComision.Armar(clases, Array.Empty<TrabajoPractico>());

            // Assert
            Assert.Equal(CalendarioComision.TituloClaseSinDescripcion, eventos[0].Titulo);
        }

        [Fact]
        public void Armar_SinClasesNiTrabajos_DevuelveUnCalendarioVacio()
        {
            // Act
            var eventos = CalendarioComision.Armar(Array.Empty<Clase>(), Array.Empty<TrabajoPractico>());

            // Assert
            Assert.Empty(eventos);
        }

        [Theory]
        [InlineData(30, "Hoy")]
        [InlineData(31, "Mañana")]
        [InlineData(29, "Ayer")]
        public void Etapa_HoyMananaYAyer_TienenSuPropioTexto(int diaDeOctubre, string esperado)
        {
            // Act
            var etapa = PresentacionCalendario.Etapa(new DateTime(2026, 10, diaDeOctubre), Hoy);

            // Assert
            Assert.Equal(esperado, etapa);
        }

        [Fact]
        public void Etapa_FechasLejanas_DiceCuantosDiasFaltanOPasaron()
        {
            // Act
            var futura = PresentacionCalendario.Etapa(new DateTime(2026, 11, 3), Hoy);
            var pasada = PresentacionCalendario.Etapa(new DateTime(2026, 10, 25), Hoy);

            // Assert
            Assert.Equal("En 4 días", futura);
            Assert.Equal("Hace 5 días", pasada);
        }

        [Fact]
        public void Color_PasadoGrisHoyVioletaYFuturoVerde()
        {
            // Act
            var pasado = PresentacionCalendario.Color(new DateTime(2026, 10, 29), Hoy);
            var hoy = PresentacionCalendario.Color(new DateTime(2026, 10, 30), Hoy);
            var futuro = PresentacionCalendario.Color(new DateTime(2026, 10, 31), Hoy);

            // Assert
            Assert.Equal("#757575", pasado);
            Assert.Equal("#512BD4", hoy);
            Assert.Equal("#388E3C", futuro);
        }

        [Fact]
        public void Tipo_ClaseYEntregaDeTrabajo_TienenSuTexto()
        {
            // Act & Assert
            Assert.Equal("Clase", PresentacionCalendario.Tipo(TipoEventoCalendario.Clase));
            Assert.Equal("Entrega de trabajo", PresentacionCalendario.Tipo(TipoEventoCalendario.Trabajo));
        }
    }
}
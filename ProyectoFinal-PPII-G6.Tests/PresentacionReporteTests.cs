using ProyectoFinal_PPII_G6.Presentacion;
using ProyectoFinal_PPII_G6.Services;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas de los textos del reporte de una comisión y del tamaño de las barras del gráfico.
    /// </summary>
    public class PresentacionReporteTests
    {
        private static ReporteComision ConDatos(int altoRiesgo, int moderado, int alDia, int sinDatos) => new()
        {
            Estado = EstadoReporte.ConDatos,
            Total = altoRiesgo + moderado + alDia + sinDatos,
            AltoRiesgo = altoRiesgo,
            RiesgoModerado = moderado,
            AlDia = alDia,
            EstudiantesSinDatos = sinDatos
        };

        [Fact]
        public void Titular_ConDatos_DiceCuantosDeCuantosEstanEnAltoRiesgo()
        {
            // Act
            var titular = PresentacionReporte.Titular(ConDatos(altoRiesgo: 7, moderado: 5, alDia: 18, sinDatos: 0));

            // Assert
            Assert.Equal("7 de 30 estudiantes en alto riesgo de abandonar", titular);
        }

        [Fact]
        public void Titular_ConUnSoloEstudiante_VaEnSingular()
        {
            // Act
            var titular = PresentacionReporte.Titular(ConDatos(altoRiesgo: 1, moderado: 0, alDia: 0, sinDatos: 0));

            // Assert
            Assert.Equal("1 de 1 estudiante en alto riesgo de abandonar", titular);
        }

        [Fact]
        public void Titular_ConDatosPeroNadieEnAltoRiesgo_DiceCeroPorqueSiHayDatos()
        {
            // Act
            var titular = PresentacionReporte.Titular(ConDatos(altoRiesgo: 0, moderado: 2, alDia: 8, sinDatos: 0));

            // Assert
            Assert.Equal("0 de 10 estudiantes en alto riesgo de abandonar", titular);
        }

        [Fact]
        public void Titular_SinDatos_ExplicaQueTodaviaNoSePuedeCalcular()
        {
            // Act
            var titular = PresentacionReporte.Titular(new ReporteComision { Estado = EstadoReporte.SinDatos, Total = 10 });

            // Assert
            Assert.Equal("Todavía no hay datos para calcular el riesgo de esta comisión.", titular);
        }

        [Fact]
        public void Titular_SinEstudiantes_DiceQueLaComisionNoTieneEstudiantes()
        {
            // Act
            var titular = PresentacionReporte.Titular(new ReporteComision { Estado = EstadoReporte.SinEstudiantes });

            // Assert
            Assert.Equal("La comisión todavía no tiene estudiantes.", titular);
        }

        [Fact]
        public void Detalle_ConDatos_ListaLosOtrosNiveles()
        {
            // Act
            var detalle = PresentacionReporte.Detalle(ConDatos(altoRiesgo: 7, moderado: 5, alDia: 17, sinDatos: 1));

            // Assert
            Assert.Equal("RIESGO MODERADO: 5  |  AL DÍA: 17  |  SIN DATOS: 1", detalle);
        }

        [Fact]
        public void Detalle_SinDatos_EstaVacio()
        {
            // Act
            var detalle = PresentacionReporte.Detalle(new ReporteComision { Estado = EstadoReporte.SinDatos, Total = 10 });

            // Assert
            Assert.Equal(string.Empty, detalle);
        }

        [Theory]
        [InlineData(7, 30, "23 %")]
        [InlineData(1, 3, "33 %")]
        [InlineData(2, 3, "67 %")]
        [InlineData(0, 30, "0 %")]
        [InlineData(30, 30, "100 %")]
        [InlineData(0, 0, "0 %")]
        public void Porcentaje_SinDecimales_ComoElPanel(int cantidad, int total, string esperado)
        {
            // Act & Assert
            Assert.Equal(esperado, PresentacionReporte.Porcentaje(cantidad, total));
        }

        [Theory]
        [InlineData(30, 30, 260)]
        [InlineData(15, 30, 130)]
        [InlineData(0, 30, 0)]
        [InlineData(0, 0, 0)]
        public void Ancho_EsProporcionalALaCantidad(int cantidad, int total, double esperado)
        {
            // Act & Assert
            Assert.Equal(esperado, PresentacionReporte.Ancho(cantidad, total, 260), 6);
        }
    }
}
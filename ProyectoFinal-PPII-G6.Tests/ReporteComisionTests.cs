using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas del reporte de una comisión: cuenta los niveles ya calculados y distingue los tres estados
    /// (sin estudiantes, sin datos y con datos).
    /// </summary>
    public class ReporteComisionTests
    {
        private static ResumenAlumno Resumen(NivelSeguimiento nivel, int presentes = 0, int justificadas = 0, int entregados = 0) => new()
        {
            Estudiante = new Estudiante(1, "30123456", "Ana", "Gómez"),
            Nivel = nivel,
            Presentes = presentes,
            Justificadas = justificadas,
            Entregados = entregados
        };

        [Fact]
        public void Armar_ConEstudiantesEnDistintosNiveles_CuentaCadaNivel()
        {
            // Arrange
            var resumenes = new[]
            {
                Resumen(NivelSeguimiento.Rojo, presentes: 1), Resumen(NivelSeguimiento.Rojo, entregados: 1),
                Resumen(NivelSeguimiento.Amarillo, presentes: 5),
                Resumen(NivelSeguimiento.Verde, presentes: 6), Resumen(NivelSeguimiento.Verde, presentes: 6), Resumen(NivelSeguimiento.Verde, presentes: 6),
                Resumen(NivelSeguimiento.SinDatos)
            };

            // Act
            var reporte = ReporteComision.Armar(resumenes);

            // Assert
            Assert.Equal(EstadoReporte.ConDatos, reporte.Estado);
            Assert.Equal(7, reporte.Total);
            Assert.Equal(2, reporte.AltoRiesgo);
            Assert.Equal(1, reporte.RiesgoModerado);
            Assert.Equal(3, reporte.AlDia);
            Assert.Equal(1, reporte.EstudiantesSinDatos);
        }

        [Fact]
        public void Armar_SinEstudiantes_DiceQueLaComisionNoTieneEstudiantes()
        {
            // Act
            var reporte = ReporteComision.Armar(Array.Empty<ResumenAlumno>());

            // Assert
            Assert.Equal(EstadoReporte.SinEstudiantes, reporte.Estado);
            Assert.Equal(0, reporte.Total);
        }

        [Fact]
        public void Armar_ConEstudiantesSinClasesNiTrabajos_DiceQueNoHayDatos()
        {
            // Arrange: todos "Sin datos", como una comisión recién importada
            var resumenes = new[] { Resumen(NivelSeguimiento.SinDatos), Resumen(NivelSeguimiento.SinDatos) };

            // Act
            var reporte = ReporteComision.Armar(resumenes);

            // Assert
            Assert.Equal(EstadoReporte.SinDatos, reporte.Estado);
            Assert.Equal(2, reporte.Total);
        }

        [Fact]
        public void Armar_ConClasesYTrabajosPeroNingunRegistro_DiceQueNoHayDatosEnVezDeTodosEnRiesgo()
        {
            // Arrange: sin registros, el cálculo cuenta ausencias y trabajos sin entregar, y da alto riesgo para todos
            var resumenes = new[] { Resumen(NivelSeguimiento.Rojo), Resumen(NivelSeguimiento.Rojo), Resumen(NivelSeguimiento.Rojo) };

            // Act
            var reporte = ReporteComision.Armar(resumenes);

            // Assert
            Assert.Equal(EstadoReporte.SinDatos, reporte.Estado);
        }

        [Theory]
        [InlineData(1, 0, 0)]
        [InlineData(0, 1, 0)]
        [InlineData(0, 0, 1)]
        public void Armar_ConUnSoloRegistroDeUnEstudiante_YaHayDatos(int presentes, int justificadas, int entregados)
        {
            // Arrange: uno solo tiene un registro y los otros ninguno
            var resumenes = new[]
            {
                Resumen(NivelSeguimiento.Rojo, presentes, justificadas, entregados),
                Resumen(NivelSeguimiento.Rojo),
                Resumen(NivelSeguimiento.SinDatos)
            };

            // Act
            var reporte = ReporteComision.Armar(resumenes);

            // Assert
            Assert.Equal(EstadoReporte.ConDatos, reporte.Estado);
        }
    }
}
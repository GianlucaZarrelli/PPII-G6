using ProyectoFinal_PPII_G6.Services;
using ProyectoFinal_PPII_G6.Presentacion;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas unitarias de los textos que explican de dónde sale cada porcentaje.
    /// </summary>
    public class TextosSeguimientoTests
    {
        [Fact]
        public void AsistenciaCorta_SinJustificadas_MuestraPresentesSobreTotal()
        {
            // Arrange
            var resumen = CrearResumen(clases: 6, presentes: 4, justificadas: 0, porcentaje: 66.67);

            // Act
            var texto = TextosSeguimiento.AsistenciaCorta(resumen);

            // Assert
            Assert.Equal("Asistencia 4/6 (67 %)", texto);
        }

        [Fact]
        public void AsistenciaCorta_ConJustificadas_LasMenciona()
        {
            // Arrange
            var resumen = CrearResumen(clases: 6, presentes: 5, justificadas: 1, porcentaje: 91.67);

            // Act
            var texto = TextosSeguimiento.AsistenciaCorta(resumen);

            // Assert
            Assert.Equal("Asistencia 5 + 1 just. de 6 (92 %)", texto);
        }

        [Fact]
        public void AsistenciaCorta_SinClases_AvisaQueNoHayClases()
        {
            // Arrange
            var resumen = CrearResumen(clases: 0, presentes: 0, justificadas: 0, porcentaje: 100);

            // Act
            var texto = TextosSeguimiento.AsistenciaCorta(resumen);

            // Assert
            Assert.Equal("Asistencia: sin clases", texto);
        }

        [Fact]
        public void EntregasCorta_SinTrabajosVencidos_AvisaQueNoHayTrabajos()
        {
            // Arrange
            var resumen = new ResumenAlumno { TrabajosContados = 0, PorcentajeEntregas = 100 };

            // Act
            var texto = TextosSeguimiento.EntregasCorta(resumen);

            // Assert
            Assert.Equal("Entregas: sin trabajos vencidos", texto);
        }

        private static ResumenAlumno CrearResumen(int clases, int presentes, int justificadas, double porcentaje) =>
            new()
            {
                ClasesContadas = clases,
                Presentes = presentes,
                Justificadas = justificadas,
                PorcentajeAsistencia = porcentaje
            };
    }
}
using ProyectoFinal_PPII_G6.Services;
using ProyectoFinal_PPII_G6.Presentacion;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas unitarias de los textos que explican de dónde sale cada porcentaje y la racha.
    /// </summary>
    public class TextosSeguimientoTests
    {
        [Fact]
        public void AsistenciaCorta_MuestraInasistenciasDeTotalDeClases()
        {
            // Arrange: 4 presentes de 6 → 2 inasistencias (33 %)
            var resumen = CrearResumen(clases: 6, presentes: 4, justificadas: 0, porcentajeAsistencia: 66.67);

            // Act
            var texto = TextosSeguimiento.AsistenciaCorta(resumen);

            // Assert
            Assert.Equal("Inasistencias 2 de 6 clases (33 %)", texto);
        }

        [Fact]
        public void AsistenciaCorta_ConJustificadas_LasMenciona()
        {
            // Arrange: 5 presentes + 1 just. de 6 → 1 inasistencia contada, 8 % inasistencias efectivas
            var resumen = CrearResumen(clases: 6, presentes: 5, justificadas: 1, porcentajeAsistencia: 91.67);

            // Act
            var texto = TextosSeguimiento.AsistenciaCorta(resumen);

            // Assert
            Assert.Equal("Inasistencias 1 de 6 clases (8 %), 1 just.", texto);
        }

        [Fact]
        public void AsistenciaCorta_SinClases_AvisaQueNoHayClases()
        {
            var resumen = CrearResumen(clases: 0, presentes: 0, justificadas: 0, porcentajeAsistencia: 100);

            var texto = TextosSeguimiento.AsistenciaCorta(resumen);

            Assert.Equal("Asistencia: sin clases", texto);
        }

        [Fact]
        public void EntregasCorta_SinTrabajosVencidos_AvisaQueNoHayTrabajos()
        {
            var resumen = new ResumenAlumno { TrabajosContados = 0, PorcentajeEntregas = 100 };

            var texto = TextosSeguimiento.EntregasCorta(resumen);

            Assert.Equal("Entregas: sin trabajos vencidos", texto);
        }

        [Fact]
        public void RachaCorta_DistingueSinDatosDeSinRachaYConRacha()
        {
            Assert.Equal(
                "Ausencias seguidas: sin datos",
                TextosSeguimiento.RachaCorta(new ResumenAlumno { Racha = ResultadoRacha.SinDatos() }));

            Assert.Equal(
                "Sin ausencias seguidas",
                TextosSeguimiento.RachaCorta(new ResumenAlumno { Racha = ResultadoRacha.SinRacha() }));

            Assert.Equal(
                "4 ausencias seguidas",
                TextosSeguimiento.RachaCorta(new ResumenAlumno { Racha = ResultadoRacha.Con(4) }));
        }

        private static ResumenAlumno CrearResumen(int clases, int presentes, int justificadas, double porcentajeAsistencia) =>
            new()
            {
                ClasesContadas = clases,
                Presentes = presentes,
                Justificadas = justificadas,
                PorcentajeAsistencia = porcentajeAsistencia
            };
    }
}

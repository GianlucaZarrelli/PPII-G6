using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Criterios de aceptación de R1 · Ausencias seguidas.
    /// </summary>
    public class RachaAusenciasTests
    {
        private static readonly DateTime Hoy = new(2026, 10, 26);
        private static readonly DateTime Primera = new(2026, 9, 7);

        [Fact]
        public void CA1_RachaVigente_CuatroAusenciasSeguidasAlFinal()
        {
            // Arrange: presente en las 2 primeras, ausente en las 4 últimas
            var alumno = CrearAlumno();
            var clases = CrearClases(6);
            var asistenciasAlumno = new List<Asistencia>
            {
                new(1, 1, 1, true, false),
                new(2, 1, 2, true, false),
                new(3, 1, 3, false, false),
                new(4, 1, 4, false, false),
                new(5, 1, 5, false, false),
                new(6, 1, 6, false, false)
            };

            // Act
            var racha = RachaAusencias.Calcular(alumno, clases, asistenciasAlumno, asistenciasAlumno, Hoy);

            // Assert
            Assert.Equal(EstadoRacha.ConRacha, racha.Estado);
            Assert.Equal(4, racha.Cantidad);
        }

        [Fact]
        public void CA2_PresenciaCortaLaRacha_NoIndicaAusenciasSeguidas()
        {
            // Arrange: ausente en 3, 4 y 5; presente en 6
            var alumno = CrearAlumno();
            var clases = CrearClases(6);
            var asistenciasAlumno = new List<Asistencia>
            {
                new(1, 1, 1, true, false),
                new(2, 1, 2, true, false),
                new(3, 1, 3, false, false),
                new(4, 1, 4, false, false),
                new(5, 1, 5, false, false),
                new(6, 1, 6, true, false)
            };

            // Act
            var racha = RachaAusencias.Calcular(alumno, clases, asistenciasAlumno, asistenciasAlumno, Hoy);

            // Assert
            Assert.Equal(EstadoRacha.SinRacha, racha.Estado);
        }

        [Fact]
        public void CA3_ClasesSinAsistenciaTomada_NoSumanNiCortan()
        {
            // Arrange: 6 con lista (P,P,A,A,A,A) + 1 pasada sin lista + 2 futuras
            var alumno = CrearAlumno();
            var clases = CrearClases(6);
            clases.Add(new Clase(7, Hoy.AddDays(-1), "Sin lista", 1));
            clases.Add(new Clase(8, Hoy.AddDays(7), "Futura 1", 1));
            clases.Add(new Clase(9, Hoy.AddDays(14), "Futura 2", 1));

            var asistenciasAlumno = new List<Asistencia>
            {
                new(1, 1, 1, true, false),
                new(2, 1, 2, true, false),
                new(3, 1, 3, false, false),
                new(4, 1, 4, false, false),
                new(5, 1, 5, false, false),
                new(6, 1, 6, false, false)
            };
            // La comisión solo tiene lista en las 6 primeras (no en la 7)
            var asistenciasComision = asistenciasAlumno.ToList();

            // Act
            var racha = RachaAusencias.Calcular(alumno, clases, asistenciasComision, asistenciasAlumno, Hoy);

            // Assert: sigue en 4
            Assert.Equal(EstadoRacha.ConRacha, racha.Estado);
            Assert.Equal(4, racha.Cantidad);
        }

        [Fact]
        public void CA4_SinRegistros_IndicaSinDatosYNoCero()
        {
            // Arrange
            var alumno = CrearAlumno();
            var clases = CrearClases(6);
            var asistenciasComision = new List<Asistencia>
            {
                new(1, 2, 1, true, false),
                new(2, 2, 2, true, false)
            };

            // Act
            var racha = RachaAusencias.Calcular(alumno, clases, asistenciasComision, Array.Empty<Asistencia>(), Hoy);

            // Assert
            Assert.Equal(EstadoRacha.SinDatos, racha.Estado);
            Assert.NotEqual(EstadoRacha.SinRacha, racha.Estado);
        }

        [Fact]
        public void IncorporacionTardia_LaRachaSoloMiraClasesDesdeLaFecha()
        {
            // Arrange: 6 clases con lista; alumno desde la clase 4; ausente en 4, 5 y 6
            var alumno = new Estudiante(1, "30111222", "Ana", "Perez", "ana@demo.test", 1)
            {
                FechaAlta = Primera.AddDays(7 * 3)
            };
            var clases = CrearClases(6);
            var asistencias = new List<Asistencia>
            {
                new(1, 2, 1, true, false),
                new(2, 2, 2, true, false),
                new(3, 2, 3, true, false),
                new(4, 1, 4, false, false),
                new(5, 1, 5, false, false),
                new(6, 1, 6, false, false)
            };
            var delAlumno = asistencias.Where(a => a.EstudianteId == 1).ToList();

            // Act
            var racha = RachaAusencias.Calcular(alumno, clases, asistencias, delAlumno, Hoy);

            // Assert: no arrastra las 3 clases anteriores a su incorporación
            Assert.Equal(EstadoRacha.ConRacha, racha.Estado);
            Assert.Equal(3, racha.Cantidad);
        }

        [Fact]
        public void Evaluar_IncluyeLaRachaEnElResumen()
        {
            var alumno = CrearAlumno();
            var clases = CrearClases(4);
            var asistencias = new List<Asistencia>
            {
                new(1, 1, 1, true, false),
                new(2, 1, 2, false, false),
                new(3, 1, 3, false, false),
                new(4, 1, 4, false, false)
            };
            var servicio = new SeguimientoService(new RiesgoService());

            var resumen = servicio.Evaluar(
                alumno, clases, Array.Empty<TrabajoPractico>(), asistencias, Array.Empty<Entrega>(), Hoy,
                asistenciasComision: asistencias);

            Assert.Equal(EstadoRacha.ConRacha, resumen.Racha.Estado);
            Assert.Equal(3, resumen.Racha.Cantidad);
        }

        private static Estudiante CrearAlumno() =>
            new(1, "30111222", "Ana", "Perez", "ana@demo.test", 1) { FechaAlta = new DateTime(2026, 8, 1) };

        private static List<Clase> CrearClases(int cantidad) =>
            Enumerable.Range(0, cantidad)
                .Select(i => new Clase(i + 1, Primera.AddDays(7 * i), $"Clase {i + 1}", 1))
                .ToList();
    }
}

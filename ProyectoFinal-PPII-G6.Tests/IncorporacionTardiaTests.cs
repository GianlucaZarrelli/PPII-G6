using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Presentacion;
using ProyectoFinal_PPII_G6.Services;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Criterios de aceptación de R2 · Incorporación tardía.
    /// </summary>
    public class IncorporacionTardiaTests
    {
        private static readonly DateTime Hoy = new(2026, 10, 26);
        private readonly SeguimientoService _servicio = new(new RiesgoService());

        [Fact]
        public void CA1_AsistenciaDesdeLaIncorporacion_CuentaSoloCuatroClases()
        {
            // Arrange: 8 clases con asistencia; Díaz desde el 15/09 → 4 clases, faltó a 1
            var diaz = new Estudiante(1, "30111000", "Díaz", "Ana", "diaz@demo.test", 1)
            {
                FechaAlta = new DateTime(2026, 9, 15)
            };
            var clases = CrearOchoClases();
            // Clases desde el 15/09: ids 5..8. Presente en 3, ausente en 1 → 25 % inasistencias.
            var asistencias = new List<Asistencia>
            {
                new(1, 1, 5, true, false),
                new(2, 1, 6, true, false),
                new(3, 1, 7, false, false),
                new(4, 1, 8, true, false)
            };

            // Act
            var resumen = _servicio.Evaluar(
                diaz, clases, Array.Empty<TrabajoPractico>(), asistencias, Array.Empty<Entrega>(), Hoy);

            // Assert
            Assert.Equal(4, resumen.ClasesContadas);
            Assert.Equal(3, resumen.Presentes);
            Assert.Equal(75, resumen.PorcentajeAsistencia, 2);
            Assert.Equal("Inasistencias 1 de 4 clases (25 %)", TextosSeguimiento.AsistenciaCorta(resumen));
        }

        [Fact]
        public void CA2_TrabajosDesdeLaIncorporacion_NoCuentaElTpAnterior()
        {
            // Arrange: TP1 vence 10/09, TP2 vence 30/09; Díaz desde 15/09; no entregó ninguno
            var diaz = new Estudiante(1, "30111000", "Díaz", "Ana", "diaz@demo.test", 1)
            {
                FechaAlta = new DateTime(2026, 9, 15)
            };
            var trabajos = new List<TrabajoPractico>
            {
                new(1, 1, "TP1", new DateTime(2026, 9, 10)),
                new(2, 1, "TP2", new DateTime(2026, 9, 30))
            };

            // Act
            var resumen = _servicio.Evaluar(
                diaz, Array.Empty<Clase>(), trabajos, Array.Empty<Asistencia>(), Array.Empty<Entrega>(), Hoy);

            // Assert: 1 no entregado de 1, no 2 de 2
            Assert.Equal(1, resumen.TrabajosContados);
            Assert.Equal(0, resumen.Entregados);
            Assert.Equal(0, resumen.PorcentajeEntregas);
            Assert.Equal("No entregados 1 de 1 (100 %)", TextosSeguimiento.EntregasCorta(resumen));
        }

        [Fact]
        public void CA3_RecienIncorporado_SinDatosNiCeroPorcientoComoNivel()
        {
            // Arrange: Vega se incorporó hoy; no hubo clases ni vencimientos desde entonces
            var vega = new Estudiante(1, "30111001", "Vega", "Luis", "vega@demo.test", 1)
            {
                FechaAlta = Hoy
            };
            var clases = CrearOchoClases();
            var trabajos = new List<TrabajoPractico>
            {
                new(1, 1, "TP1", new DateTime(2026, 9, 10)),
                new(2, 1, "TP2", new DateTime(2026, 9, 30))
            };

            // Act
            var resumen = _servicio.Evaluar(
                vega, clases, trabajos, Array.Empty<Asistencia>(), Array.Empty<Entrega>(), Hoy);

            // Assert
            Assert.Equal(NivelSeguimiento.SinDatos, resumen.Nivel);
            Assert.Equal(0, resumen.ClasesContadas);
            Assert.Equal(0, resumen.TrabajosContados);
        }

        [Fact]
        public void CA4_SinFecha_CuentaTodaLaHistoria()
        {
            // Arrange: Pérez sin fecha; 8 clases y 2 trabajos
            var perez = new Estudiante(1, "30111002", "Pérez", "Juan", "perez@demo.test", 1)
            {
                FechaAlta = null
            };
            var clases = CrearOchoClases();
            var trabajos = new List<TrabajoPractico>
            {
                new(1, 1, "TP1", new DateTime(2026, 9, 10)),
                new(2, 1, "TP2", new DateTime(2026, 9, 30))
            };
            var asistencias = clases.Select((c, i) => new Asistencia(i + 1, 1, c.Id, true, false)).ToList();
            var entregas = trabajos.Select((t, i) => new Entrega(i + 1, 1, t.Id, true)).ToList();

            // Act
            var resumen = _servicio.Evaluar(perez, clases, trabajos, asistencias, entregas, Hoy);

            // Assert
            Assert.Equal(8, resumen.ClasesContadas);
            Assert.Equal(2, resumen.TrabajosContados);
            Assert.Equal(NivelSeguimiento.Verde, resumen.Nivel);
        }

        [Fact]
        public void RechazaFechaDeIncorporacionPosteriorAHoy()
        {
            var error = ValidacionIncorporacion.ObtenerError(Hoy.AddDays(1), Hoy);
            Assert.Equal("La fecha de incorporación no puede ser posterior a hoy.", error);
        }

        [Fact]
        public void AceptaFechaDeIncorporacionDeHoyOAnterior()
        {
            Assert.Null(ValidacionIncorporacion.ObtenerError(Hoy, Hoy));
            Assert.Null(ValidacionIncorporacion.ObtenerError(Hoy.AddDays(-10), Hoy));
            Assert.Null(ValidacionIncorporacion.ObtenerError(null, Hoy));
        }

        /// <summary>
        /// 8 clases: 4 antes del 15/09 y 4 desde el 15/09 inclusive.
        /// </summary>
        private static List<Clase> CrearOchoClases() =>
        [
            new(1, new DateTime(2026, 9, 1), "Clase 1", 1),
            new(2, new DateTime(2026, 9, 5), "Clase 2", 1),
            new(3, new DateTime(2026, 9, 8), "Clase 3", 1),
            new(4, new DateTime(2026, 9, 12), "Clase 4", 1),
            new(5, new DateTime(2026, 9, 15), "Clase 5", 1),
            new(6, new DateTime(2026, 9, 22), "Clase 6", 1),
            new(7, new DateTime(2026, 9, 29), "Clase 7", 1),
            new(8, new DateTime(2026, 10, 6), "Clase 8", 1)
        ];
    }
}

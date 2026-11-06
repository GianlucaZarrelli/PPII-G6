using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas de que las clases cargadas por adelantado en el calendario no cuentan para el riesgo hasta que llegan.
    /// </summary>
    public class SeguimientoClasesFuturasTests
    {
        private static readonly DateTime Hoy = new(2026, 10, 26);

        private readonly SeguimientoService _servicio = new(new RiesgoService());

        private static Estudiante CrearAlumno() => new(1, "90000001", "Lucía", "Fernández", "", 1)
        {
            FechaAlta = new DateTime(2026, 8, 1)
        };

        private static List<Clase> ClasesPasadas() => new()
        {
            new Clase(1, new DateTime(2026, 10, 5), null, 1),
            new Clase(2, new DateTime(2026, 10, 12), null, 1)
        };

        private static List<Clase> ClasesFuturas() => new()
        {
            new Clase(3, new DateTime(2026, 11, 2), null, 1),
            new Clase(4, new DateTime(2026, 11, 9), null, 1),
            new Clase(5, new DateTime(2026, 11, 16), null, 1)
        };

        private static List<Asistencia> AsistenciasDeLasPasadas() => new()
        {
            new Asistencia(1, 1, 1, true, false),
            new Asistencia(2, 1, 2, true, false)
        };

        private ResumenAlumno Evaluar(IEnumerable<Clase> clases) =>
            _servicio.Evaluar(CrearAlumno(), clases, new List<TrabajoPractico>(),
                AsistenciasDeLasPasadas(), new List<Entrega>(), Hoy);

        [Fact]
        public void Evaluar_ClasesFuturas_NoCuentanNiMarcanAusenteAlAlumno()
        {
            // Arrange: dos clases pasadas con asistencia perfecta y tres clases futuras cargadas por adelantado
            var todas = ClasesPasadas().Concat(ClasesFuturas()).ToList();

            // Act
            var resumen = Evaluar(todas);

            // Assert
            Assert.Equal(2, resumen.ClasesContadas);
            Assert.Equal(NivelSeguimiento.Verde, resumen.Nivel);
            Assert.Equal(100, resumen.PorcentajeAsistencia);
        }

        [Fact]
        public void Evaluar_CargarElCuatrimestreEntero_NoCambiaElResultado()
        {
            // Act
            var conSoloLasPasadas = Evaluar(ClasesPasadas());
            var conTodoElCalendario = Evaluar(ClasesPasadas().Concat(ClasesFuturas()).ToList());

            // Assert
            Assert.Equal(conSoloLasPasadas.Nivel, conTodoElCalendario.Nivel);
            Assert.Equal(conSoloLasPasadas.PorcentajeAsistencia, conTodoElCalendario.PorcentajeAsistencia);
            Assert.Equal(conSoloLasPasadas.ClasesContadas, conTodoElCalendario.ClasesContadas);
        }

        [Fact]
        public void Evaluar_UnaClaseDeHoy_SiCuenta()
        {
            // Arrange: la clase de hoy todavía no tiene asistencia cargada
            var clases = ClasesPasadas();
            clases.Add(new Clase(6, Hoy, null, 1));

            // Act
            var resumen = Evaluar(clases);

            // Assert
            Assert.Equal(3, resumen.ClasesContadas);
        }

        [Fact]
        public void Evaluar_ElDiaDeLaClaseFutura_YaCuenta()
        {
            // Arrange
            var clases = ClasesPasadas().Concat(ClasesFuturas()).ToList();
            var elDiaDeLaPrimeraFutura = new DateTime(2026, 11, 2);

            // Act
            var resumen = _servicio.Evaluar(CrearAlumno(), clases, new List<TrabajoPractico>(),
                AsistenciasDeLasPasadas(), new List<Entrega>(), elDiaDeLaPrimeraFutura);

            // Assert: llegó el día de la clase 3: ahora son 3 clases y la nueva todavía no tiene asistencia
            Assert.Equal(3, resumen.ClasesContadas);
        }
    }
}
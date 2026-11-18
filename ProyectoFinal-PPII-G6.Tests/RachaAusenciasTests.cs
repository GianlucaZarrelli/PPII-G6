using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Presentacion;
using ProyectoFinal_PPII_G6.Services;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Criterios de aceptación de la racha de ausencias en el panel.
    /// </summary>
    public class RachaAusenciasTests
    {
        private static readonly DateTime Hoy = new(2026, 10, 26);
        private static readonly DateTime PrimeraClase = new(2026, 9, 7);

        [Fact]
        public void CA1_RachaVigente_CuatroAusenciasSeguidas()
        {
            // Arrange: 6 clases con lista; Pérez presente en las 2 primeras y ausente en las 4 últimas
            var clases = CrearClasesConLista(6);
            var perez = CrearAlumno(1, "Perez");
            var asistenciasPerez = CrearAsistenciasAlumno(perez.Id, clases, presentesAlInicio: 2);
            var asistenciasComision = asistenciasPerez.ToList();

            // Act
            var racha = RachaAusencias.Calcular(clases, asistenciasComision, asistenciasPerez, Hoy);

            // Assert
            Assert.Equal(EstadoRacha.ConRacha, racha.Estado);
            Assert.Equal(4, racha.Cantidad);
        }

        [Fact]
        public void CA2_PresenciaCortaLaRacha_NoIndicaAusenciasSeguidas()
        {
            // Arrange: Gómez ausente en 3, 4 y 5 y presente en la 6
            var clases = CrearClasesConLista(6);
            var gomez = CrearAlumno(2, "Gomez");
            var asistenciasGomez = new List<Asistencia>
            {
                new(1, gomez.Id, clases[0].Id, true, false),
                new(2, gomez.Id, clases[1].Id, true, false),
                new(3, gomez.Id, clases[2].Id, false, false),
                new(4, gomez.Id, clases[3].Id, false, false),
                new(5, gomez.Id, clases[4].Id, false, false),
                new(6, gomez.Id, clases[5].Id, true, false)
            };

            // Act
            var racha = RachaAusencias.Calcular(clases, asistenciasGomez, asistenciasGomez, Hoy);

            // Assert
            Assert.Equal(EstadoRacha.SinRacha, racha.Estado);
            Assert.Equal(0, racha.Cantidad);
            Assert.Equal(string.Empty, TextosSeguimiento.RachaCorta(new ResumenAlumno { Racha = racha }));
        }

        [Fact]
        public void CA3_ClasesSinAsistenciaTomada_NoSumanNiCortan()
        {
            // Arrange: las 6 con lista de Pérez + 2 futuras + 1 pasada sin lista
            var clasesConLista = CrearClasesConLista(6);
            var pasadaSinLista = new Clase(100, PrimeraClase.AddDays(7 * 6), "Sin lista", 1);
            var futura1 = new Clase(101, Hoy.AddDays(7), "Futura 1", 1);
            var futura2 = new Clase(102, Hoy.AddDays(14), "Futura 2", 1);
            var todas = clasesConLista.Concat(new[] { pasadaSinLista, futura1, futura2 }).ToList();

            var perez = CrearAlumno(1, "Perez");
            var asistenciasPerez = CrearAsistenciasAlumno(perez.Id, clasesConLista, presentesAlInicio: 2);

            // Act
            var racha = RachaAusencias.Calcular(todas, asistenciasPerez, asistenciasPerez, Hoy);

            // Assert: sigue en 4; las sin lista no entran al cálculo
            Assert.Equal(EstadoRacha.ConRacha, racha.Estado);
            Assert.Equal(4, racha.Cantidad);
        }

        [Fact]
        public void CA4_SinRegistros_IndicaSinDatosYNoCero()
        {
            // Arrange: hay clases con lista (otros alumnos), Díaz no tiene ningún registro
            var clases = CrearClasesConLista(6);
            var otro = CrearAlumno(10, "Otro");
            var diaz = CrearAlumno(3, "Diaz");
            var asistenciasComision = CrearAsistenciasAlumno(otro.Id, clases, presentesAlInicio: 6);

            // Act
            var racha = RachaAusencias.Calcular(clases, asistenciasComision, Array.Empty<Asistencia>(), Hoy);

            // Assert
            Assert.Equal(EstadoRacha.SinDatos, racha.Estado);
            Assert.NotEqual(EstadoRacha.SinRacha, racha.Estado);
            Assert.Equal("sin datos de asistencia", TextosSeguimiento.RachaCorta(new ResumenAlumno { Racha = racha }));
            Assert.DoesNotContain("0 ausencias", TextosSeguimiento.RachaCorta(new ResumenAlumno { Racha = racha }));
        }

        [Fact]
        public void Evaluar_IncluyeRachaEnElResumenDelPanel()
        {
            // Arrange: mismo escenario CA1, con asistencias de comisión
            var clases = CrearClasesConLista(6);
            var perez = CrearAlumno(1, "Perez");
            var asistenciasPerez = CrearAsistenciasAlumno(perez.Id, clases, presentesAlInicio: 2);
            var servicio = new SeguimientoService(new RiesgoService());

            // Act
            var resumen = servicio.Evaluar(
                perez, clases, new List<TrabajoPractico>(),
                asistenciasPerez, new List<Entrega>(), Hoy,
                asistenciasComision: asistenciasPerez);

            // Assert
            Assert.Equal(EstadoRacha.ConRacha, resumen.Racha.Estado);
            Assert.Equal(4, resumen.Racha.Cantidad);
            Assert.Contains("4 ausencias seguidas", TextosSeguimiento.RachaCorta(resumen));
        }

        [Fact]
        public void Justificada_NoCortaLaRacha()
        {
            // Arrange: última clase ausente justificada; antes también ausente
            var clases = CrearClasesConLista(3);
            var alumno = CrearAlumno(1, "Perez");
            var asistencias = new List<Asistencia>
            {
                new(1, alumno.Id, clases[0].Id, true, false),
                new(2, alumno.Id, clases[1].Id, false, false),
                new(3, alumno.Id, clases[2].Id, false, true)
            };

            // Act
            var racha = RachaAusencias.Calcular(clases, asistencias, asistencias, Hoy);

            // Assert
            Assert.Equal(EstadoRacha.ConRacha, racha.Estado);
            Assert.Equal(2, racha.Cantidad);
        }

        private static Estudiante CrearAlumno(int id, string apellido) =>
            new Estudiante(id, $"30{id:000000}", "Alumno", apellido, $"{apellido.ToLower()}@demo.test", 1)
            {
                FechaAlta = new DateTime(2026, 8, 1)
            };

        private static List<Clase> CrearClasesConLista(int cantidad) =>
            Enumerable.Range(0, cantidad)
                .Select(i => new Clase(i + 1, PrimeraClase.AddDays(7 * i), $"Clase {i + 1}", 1))
                .ToList();

        private static List<Asistencia> CrearAsistenciasAlumno(int estudianteId, List<Clase> clases, int presentesAlInicio) =>
            clases.Select((c, i) => new Asistencia(estudianteId * 100 + i + 1, estudianteId, c.Id, i < presentesAlInicio, false))
                .ToList();
    }
}
using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas unitarias de qué clases y trabajos le cuentan a cada alumno, y del orden del panel.
    /// </summary>
    public class SeguimientoServiceTests
    {
        private static readonly DateTime Hoy = new(2026, 10, 26);
        private static readonly DateTime PrimeraClase = new(2026, 9, 7);

        private readonly SeguimientoService _servicio = new(new RiesgoService());

        [Fact]
        public void Evaluar_AlumnoAlDia_RetornaVerde()
        {
            // Arrange: alta antes de las clases, 6/6 presentes y 3/3 trabajos vencidos entregados
            var alumno = CrearAlumno(new DateTime(2026, 8, 1));
            var clases = CrearClases(6);
            var trabajos = CrearTrabajos(new DateTime(2026, 9, 20), new DateTime(2026, 10, 1), new DateTime(2026, 10, 10));

            // Act
            var resumen = _servicio.Evaluar(alumno, clases, trabajos,
                CrearAsistencias(clases, 6), CrearEntregas(trabajos, 3), Hoy);

            // Assert
            Assert.Equal(NivelSeguimiento.Verde, resumen.Nivel);
            Assert.Equal(6, resumen.ClasesContadas);
            Assert.Equal(3, resumen.TrabajosContados);
        }

        [Fact]
        public void Evaluar_AlumnoQueIngresaDespuesDeTodasLasClases_RetornaSinDatos()
        {
            // Arrange: se da de alta hoy, cuando la comisión ya dio 6 clases y no hay trabajos vencidos
            var alumno = CrearAlumno(Hoy);
            var clases = CrearClases(6);

            // Act
            var resumen = _servicio.Evaluar(alumno, clases, new List<TrabajoPractico>(),
                new List<Asistencia>(), new List<Entrega>(), Hoy);

            // Assert: antes contaba 0/6 y salía en rojo; ahora no hay nada que evaluar
            Assert.Equal(NivelSeguimiento.SinDatos, resumen.Nivel);
            Assert.Equal(0, resumen.ClasesContadas);
        }

        [Fact]
        public void Evaluar_AlumnoQueIngresaAMitad_SoloCuentaLasClasesDesdeSuAlta()
        {
            // Arrange: alta el 5/10, así que le tocan las 2 últimas clases de las 6, y asistió a las dos
            var alumno = CrearAlumno(new DateTime(2026, 10, 5));
            var clases = CrearClases(6);
            var asistencias = new List<Asistencia>
            {
                new(1, 1, clases[4].Id, true, false),
                new(2, 1, clases[5].Id, true, false)
            };

            // Act
            var resumen = _servicio.Evaluar(alumno, clases, new List<TrabajoPractico>(),
                asistencias, new List<Entrega>(), Hoy);

            // Assert
            Assert.Equal(2, resumen.ClasesContadas);
            Assert.Equal(2, resumen.Presentes);
            Assert.Equal(NivelSeguimiento.Verde, resumen.Nivel);
        }

        [Fact]
        public void Evaluar_ClaseAnteriorAlAltaConRegistro_SeCuenta()
        {
            // Arrange: alta hoy, pero la primera clase tiene una asistencia cargada a mano
            var alumno = CrearAlumno(Hoy);
            var clases = CrearClases(6);
            var asistencias = new List<Asistencia> { new(1, 1, clases[0].Id, true, false) };

            // Act
            var resumen = _servicio.Evaluar(alumno, clases, new List<TrabajoPractico>(),
                asistencias, new List<Entrega>(), Hoy);

            // Assert: si alguien registró esa clase para el alumno, cuenta
            Assert.Equal(1, resumen.ClasesContadas);
            Assert.Equal(1, resumen.Presentes);
        }

        [Fact]
        public void Evaluar_TrabajoConVencimientoFuturo_NoBajaElPorcentaje()
        {
            // Arrange: entregó los 2 trabajos vencidos; el tercero vence el 30/10 y todavía no corresponde
            var alumno = CrearAlumno(new DateTime(2026, 8, 1));
            var clases = CrearClases(6);
            var trabajos = CrearTrabajos(new DateTime(2026, 9, 20), new DateTime(2026, 10, 1), new DateTime(2026, 10, 30));

            // Act
            var resumen = _servicio.Evaluar(alumno, clases, trabajos,
                CrearAsistencias(clases, 6), CrearEntregas(trabajos, 2), Hoy);

            // Assert: antes bajaba a 2/3 y pasaba a riesgo moderado
            Assert.Equal(2, resumen.TrabajosContados);
            Assert.Equal(100, resumen.PorcentajeEntregas);
            Assert.Equal(NivelSeguimiento.Verde, resumen.Nivel);
        }

        [Fact]
        public void Evaluar_TrabajoVencidoSinEntrega_SiCuenta()
        {
            // Arrange: el tercer trabajo ya venció y no lo entregó
            var alumno = CrearAlumno(new DateTime(2026, 8, 1));
            var clases = CrearClases(6);
            var trabajos = CrearTrabajos(new DateTime(2026, 9, 20), new DateTime(2026, 10, 1), new DateTime(2026, 10, 20));

            // Act
            var resumen = _servicio.Evaluar(alumno, clases, trabajos,
                CrearAsistencias(clases, 6), CrearEntregas(trabajos, 2), Hoy);

            // Assert: 2 de 3 es 66,7 %, que cae en amarillo
            Assert.Equal(3, resumen.TrabajosContados);
            Assert.Equal(66.67, resumen.PorcentajeEntregas, 2);
            Assert.Equal(NivelSeguimiento.Amarillo, resumen.Nivel);
        }

        [Fact]
        public void Evaluar_TrabajoQueVenceHoy_TodaviaNoCuenta()
        {
            // Arrange: el tercer trabajo vence hoy, así que todavía se puede entregar
            var alumno = CrearAlumno(new DateTime(2026, 8, 1));
            var clases = CrearClases(6);
            var trabajos = CrearTrabajos(new DateTime(2026, 9, 20), new DateTime(2026, 10, 1), Hoy);

            // Act
            var resumen = _servicio.Evaluar(alumno, clases, trabajos,
                CrearAsistencias(clases, 6), CrearEntregas(trabajos, 2), Hoy);

            // Assert: recién cuenta desde el día siguiente
            Assert.Equal(2, resumen.TrabajosContados);
            Assert.Equal(NivelSeguimiento.Verde, resumen.Nivel);
        }

        [Fact]
        public void Evaluar_SinClasesNiTrabajosContables_RetornaSinDatos()
        {
            // Arrange: alta hoy y el único trabajo vence dentro de unos días
            var alumno = CrearAlumno(Hoy);
            var clases = CrearClases(6);
            var trabajos = CrearTrabajos(new DateTime(2026, 11, 5));

            // Act
            var resumen = _servicio.Evaluar(alumno, clases, trabajos,
                new List<Asistencia>(), new List<Entrega>(), Hoy);

            // Assert
            Assert.Equal(NivelSeguimiento.SinDatos, resumen.Nivel);
            Assert.Equal(0, resumen.TrabajosContados);
        }

        [Fact]
        public void Evaluar_AsistenciaPerfectaPeroEntregasBajas_RetornaRojo()
        {
            // Arrange: viene a todas las clases pero entregó 1 de 3 trabajos vencidos
            var alumno = CrearAlumno(new DateTime(2026, 8, 1));
            var clases = CrearClases(6);
            var trabajos = CrearTrabajos(new DateTime(2026, 9, 20), new DateTime(2026, 10, 1), new DateTime(2026, 10, 10));

            // Act
            var resumen = _servicio.Evaluar(alumno, clases, trabajos,
                CrearAsistencias(clases, 6), CrearEntregas(trabajos, 1), Hoy);

            // Assert: el caso de Joaquín en los datos de demostración
            Assert.Equal(NivelSeguimiento.Rojo, resumen.Nivel);
            Assert.Equal(100, resumen.PorcentajeAsistencia);
            Assert.Equal(33.33, resumen.PeorPorcentaje, 2);
        }

        [Fact]
        public void Evaluar_AusenciaJustificada_SeInformaEnElResumen()
        {
            // Arrange: 5 presentes y la última clase ausente con justificación
            var alumno = CrearAlumno(new DateTime(2026, 8, 1));
            var clases = CrearClases(6);
            var asistencias = CrearAsistencias(clases.Take(5).ToList(), 5);
            asistencias.Add(new Asistencia(6, 1, clases[5].Id, false, true));

            // Act
            var resumen = _servicio.Evaluar(alumno, clases, new List<TrabajoPractico>(),
                asistencias, new List<Entrega>(), Hoy);

            // Assert: (5 + 0,5) / 6 = 91,7 %
            Assert.Equal(5, resumen.Presentes);
            Assert.Equal(1, resumen.Justificadas);
            Assert.Equal(91.67, resumen.PorcentajeAsistencia, 2);
            Assert.Equal(NivelSeguimiento.Verde, resumen.Nivel);
        }

        [Fact]
        public void Ordenar_PonePrimeroAlMasGraveYAlFinalSinDatos()
        {
            // Arrange
            var resumenes = new List<ResumenAlumno>
            {
                CrearResumen("Sin datos", NivelSeguimiento.SinDatos, 100, 100),
                CrearResumen("Verde", NivelSeguimiento.Verde, 100, 100),
                CrearResumen("Amarillo", NivelSeguimiento.Amarillo, 83, 100),
                CrearResumen("Rojo leve", NivelSeguimiento.Rojo, 70, 100),
                CrearResumen("Rojo grave", NivelSeguimiento.Rojo, 50, 0)
            };

            // Act
            var nombres = _servicio.Ordenar(resumenes).Select(r => r.Estudiante.Nombre).ToList();

            // Assert
            Assert.Equal(new[] { "Rojo grave", "Rojo leve", "Amarillo", "Verde", "Sin datos" }, nombres);
        }

        [Fact]
        public void Ordenar_ConElMismoNivelYPorcentaje_DesempataPorNombre()
        {
            // Arrange
            var resumenes = new List<ResumenAlumno>
            {
                CrearResumen("Zoe", NivelSeguimiento.Verde, 100, 100),
                CrearResumen("Ana", NivelSeguimiento.Verde, 100, 100)
            };

            // Act
            var nombres = _servicio.Ordenar(resumenes).Select(r => r.Estudiante.Nombre).ToList();

            // Assert
            Assert.Equal(new[] { "Ana", "Zoe" }, nombres);
        }

        // Helpers para armar los datos de prueba

        private static Estudiante CrearAlumno(DateTime alta) =>
            new Estudiante(1, "30111222", "Ana", "Perez", "ana@demo.test", 1) { FechaAlta = alta };

        /// <summary>
        /// Clases semanales a partir del 7/9, todas anteriores a "hoy".
        /// </summary>
        private static List<Clase> CrearClases(int cantidad) =>
            Enumerable.Range(0, cantidad)
                .Select(i => new Clase(i + 1, PrimeraClase.AddDays(7 * i), $"Clase {i + 1}", 1))
                .ToList();

        /// <summary>
        /// Las primeras "presentes" clases con asistencia y el resto ausente sin justificar.
        /// </summary>
        private static List<Asistencia> CrearAsistencias(List<Clase> clases, int presentes) =>
            clases.Select((c, i) => new Asistencia(i + 1, 1, c.Id, i < presentes, false)).ToList();

        private static List<TrabajoPractico> CrearTrabajos(params DateTime[] vencimientos) =>
            vencimientos.Select((v, i) => new TrabajoPractico(i + 1, 1, $"TP{i + 1}", v)).ToList();

        /// <summary>
        /// Los primeros "entregados" trabajos de la lista figuran como entregados y el resto como no entregados.
        /// </summary>
        private static List<Entrega> CrearEntregas(List<TrabajoPractico> trabajos, int entregados) =>
            trabajos.Select((t, i) => new Entrega(i + 1, 1, t.Id, i < entregados)).ToList();

        private static ResumenAlumno CrearResumen(string nombre, NivelSeguimiento nivel, double asistencia, double entregas) =>
            new()
            {
                Estudiante = new Estudiante(1, "1", nombre, "X", "x@demo.test", 1),
                Nivel = nivel,
                PorcentajeAsistencia = asistencia,
                PorcentajeEntregas = entregas
            };
    }
}
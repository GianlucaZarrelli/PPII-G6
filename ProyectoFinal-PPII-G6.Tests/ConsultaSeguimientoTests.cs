using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas de que la consulta de seguimiento le asigna a cada alumno sus propios contactos, con una base falsa.
    /// </summary>
    public class ConsultaSeguimientoTests
    {
        private readonly DatosEnMemoria _datos = new();
        private readonly ConsultaSeguimiento _consulta;

        public ConsultaSeguimientoTests()
        {
            _consulta = new ConsultaSeguimiento(_datos, new SeguimientoService(new RiesgoService()));

            _datos.Comisiones.Add(new Comision(1, "A"));
            _datos.Comisiones.Add(new Comision(2, "C"));
            _datos.Clases.Add(new Clase(1, new DateTime(2026, 9, 7), null, 1));
            _datos.Clases.Add(new Clase(2, new DateTime(2026, 9, 7), null, 2));
            _datos.AgregarAlumno(1, "Joaquín", "Pérez", 1);
            _datos.AgregarAlumno(2, "Mateo", "Gómez", 1);
            _datos.AgregarAlumno(3, "Facundo", "Medina", 2);

            _datos.Contactos.Add(new Contacto(1, 1, new DateTime(2026, 10, 21, 16, 0, 0), MedioContacto.Mail, "No respondió."));
            _datos.Contactos.Add(new Contacto(2, 1, new DateTime(2026, 10, 28, 10, 30, 0), MedioContacto.Llamada, "Atendió."));
            _datos.Contactos.Add(new Contacto(3, 3, new DateTime(2026, 10, 27, 18, 15, 0), MedioContacto.Mensaje, "Respondió."));
        }

        [Fact]
        public async Task ObtenerComision_CadaAlumnoRecibeSoloSusContactos()
        {
            // Act
            var resumenes = await _consulta.ObtenerComisionAsync(1);

            // Assert
            var joaquin = resumenes.Single(r => r.Estudiante.Id == 1);
            var mateo = resumenes.Single(r => r.Estudiante.Id == 2);

            Assert.Equal(new DateTime(2026, 10, 28, 10, 30, 0), joaquin.UltimoContacto);
            Assert.Equal(2, joaquin.CantidadContactos);
            Assert.Null(mateo.UltimoContacto);
            Assert.Equal(0, mateo.CantidadContactos);
        }

        [Fact]
        public async Task ObtenerTodas_IncluyeLosContactosDeTodasLasComisiones()
        {
            // Act
            var resumenes = await _consulta.ObtenerTodasAsync();

            // Assert
            Assert.Equal(3, resumenes.Count);
            Assert.Equal(new DateTime(2026, 10, 27, 18, 15, 0), resumenes.Single(r => r.Estudiante.Id == 3).UltimoContacto);
            Assert.Equal(2, resumenes.Count(r => r.UltimoContacto != null));
        }

        [Fact]
        public async Task ObtenerFicha_TraeLosContactosDelAlumnoDelMasRecienteAlMasAntiguo()
        {
            // Arrange
            var joaquin = _datos.Estudiantes.Single(e => e.Id == 1);

            // Act
            var ficha = await _consulta.ObtenerFichaAsync(joaquin);

            // Assert
            Assert.Equal(new[] { 2, 1 }, ficha.Contactos.Select(c => c.Id));
            Assert.Equal(2, ficha.Resumen.CantidadContactos);
            Assert.Equal(new DateTime(2026, 10, 28, 10, 30, 0), ficha.Resumen.UltimoContacto);
        }

        [Fact]
        public async Task ObtenerFicha_DeUnAlumnoSinContactos_TraeLaListaVacia()
        {
            // Arrange
            var mateo = _datos.Estudiantes.Single(e => e.Id == 2);

            // Act
            var ficha = await _consulta.ObtenerFichaAsync(mateo);

            // Assert
            Assert.Empty(ficha.Contactos);
            Assert.Null(ficha.Resumen.UltimoContacto);
        }
    }
}
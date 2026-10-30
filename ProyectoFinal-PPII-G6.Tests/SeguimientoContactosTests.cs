using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas de qué sabe el resumen de un alumno sobre sus contactos. Contactar a un alumno no cambia su nivel.
    /// </summary>
    public class SeguimientoContactosTests
    {
        private static readonly DateTime Hoy = new(2026, 10, 26);

        private readonly SeguimientoService _servicio = new(new RiesgoService());

        private static Estudiante CrearAlumno() => new(1, "90000004", "Joaquín", "Pérez", "", 1)
        {
            FechaAlta = new DateTime(2026, 8, 1)
        };

        private static List<Clase> CrearClases() => new()
        {
            new Clase(1, new DateTime(2026, 9, 7), null, 1),
            new Clase(2, new DateTime(2026, 9, 14), null, 1)
        };

        /// <summary>
        /// Evalúa a un alumno que faltó a las dos clases (nivel rojo) con los contactos indicados.
        /// </summary>
        private ResumenAlumno EvaluarAlumnoAusente(IEnumerable<Contacto>? contactos)
        {
            var asistencias = new List<Asistencia> { new(1, 1, 1, false, false), new(2, 1, 2, false, false) };

            return _servicio.Evaluar(CrearAlumno(), CrearClases(), new List<TrabajoPractico>(),
                asistencias, new List<Entrega>(), Hoy, contactos);
        }

        [Fact]
        public void Evaluar_SinContactos_NoTieneUltimoContactoNiCantidad()
        {
            // Act
            var sinDatos = EvaluarAlumnoAusente(null);
            var listaVacia = EvaluarAlumnoAusente(new List<Contacto>());

            // Assert
            Assert.Null(sinDatos.UltimoContacto);
            Assert.Equal(0, sinDatos.CantidadContactos);
            Assert.Null(listaVacia.UltimoContacto);
            Assert.Equal(0, listaVacia.CantidadContactos);
        }

        [Fact]
        public void Evaluar_ConVariosContactos_ElUltimoEsElMasRecienteSinImportarElOrden()
        {
            // Arrange: llegan desordenados
            var contactos = new List<Contacto>
            {
                new(2, 1, new DateTime(2026, 10, 28, 10, 30, 0), MedioContacto.Llamada, "Atendió."),
                new(1, 1, new DateTime(2026, 10, 21, 16, 0, 0), MedioContacto.Mail, "No respondió."),
                new(3, 1, new DateTime(2026, 10, 24, 9, 0, 0), MedioContacto.Mensaje, "Visto.")
            };

            // Act
            var resumen = EvaluarAlumnoAusente(contactos);

            // Assert
            Assert.Equal(new DateTime(2026, 10, 28, 10, 30, 0), resumen.UltimoContacto);
            Assert.Equal(3, resumen.CantidadContactos);
        }

        [Fact]
        public void Evaluar_ContactarAlAlumno_NoCambiaSuNivelDeRiesgo()
        {
            // Arrange
            var contacto = new Contacto(1, 1, new DateTime(2026, 10, 25, 11, 0, 0), MedioContacto.Llamada, "Atendió.");

            // Act
            var sinContactar = EvaluarAlumnoAusente(null);
            var contactado = EvaluarAlumnoAusente(new[] { contacto });

            // Assert: sigue en rojo: que lo hayan llamado no significa que haya empezado a asistir
            Assert.Equal(NivelSeguimiento.Rojo, sinContactar.Nivel);
            Assert.Equal(NivelSeguimiento.Rojo, contactado.Nivel);
            Assert.Equal(sinContactar.PorcentajeAsistencia, contactado.PorcentajeAsistencia);
        }
    }
}
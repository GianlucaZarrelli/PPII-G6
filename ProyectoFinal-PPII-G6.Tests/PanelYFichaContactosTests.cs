using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;
using ProyectoFinal_PPII_G6.ViewModels;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas de que el panel y la ficha del alumno muestran si fue contactado y cuándo, con una base falsa.
    /// </summary>
    public class PanelYFichaContactosTests
    {
        private readonly DatosEnMemoria _datos = new();
        private readonly ConsultaSeguimiento _consulta;

        public PanelYFichaContactosTests()
        {
            _consulta = new ConsultaSeguimiento(_datos, new SeguimientoService(new RiesgoService()));

            _datos.Comisiones.Add(new Comision(1, "A"));
            _datos.Clases.Add(new Clase(1, new DateTime(2026, 9, 7), null, 1));
            _datos.AgregarAlumno(1, "Joaquín", "Pérez", 1);
            _datos.AgregarAlumno(2, "Mateo", "Gómez", 1);

            // Los dos faltaron a la única clase (nivel rojo); a Joaquín ya lo contactaron dos veces
            _datos.Asistencias.Add(new Asistencia(1, 1, 1, false, false));
            _datos.Asistencias.Add(new Asistencia(2, 2, 1, false, false));
            _datos.Contactos.Add(new Contacto(1, 1, new DateTime(2026, 10, 21, 16, 0, 0), MedioContacto.Mail, "No respondió."));
            _datos.Contactos.Add(new Contacto(2, 1, new DateTime(2026, 10, 28, 10, 30, 0), MedioContacto.Llamada, "Atendió."));
        }

        [Fact]
        public async Task Panel_MarcaALosContactadosYAlosPendientes()
        {
            // Arrange
            var vm = new PanelRiesgoViewModel(_datos, _consulta);

            // Act
            await vm.CargarAsync();

            // Assert
            var joaquin = vm.Alumnos.Single(a => a.EstudianteId == 1);
            var mateo = vm.Alumnos.Single(a => a.EstudianteId == 2);

            Assert.Equal("Último contacto: 28/10/2026 10:30 (2 contactos)", joaquin.ContactoTexto);
            Assert.Equal("#388E3C", joaquin.ContactoColor);
            Assert.Equal("Sin contactar", mateo.ContactoTexto);
            Assert.Equal("#F57C00", mateo.ContactoColor);
        }

        [Fact]
        public async Task Ficha_MuestraElHistorialDeContactosDelMasRecienteAlMasAntiguo()
        {
            // Arrange
            var vm = new DetalleEstudianteViewModel(_datos, _consulta, new SeleccionAlumnoService());
            await vm.CargarListaEstudiantesAsync();

            // Act
            vm.EstudianteSeleccionado = vm.ListaEstudiantes.Single(e => e.Id == 1);

            // Assert
            Assert.Equal(new[] { "28/10/2026 10:30", "21/10/2026 16:00" }, vm.HistorialContactos.Select(c => c.FechaHora));
            Assert.Equal(new[] { "Llamada", "Mail" }, vm.HistorialContactos.Select(c => c.Medio));
            Assert.Equal("Último contacto: 28/10/2026 10:30 (2 contactos)", vm.ContactoTexto);
        }

        [Fact]
        public async Task Ficha_DeUnAlumnoSinContactos_LoDiceYNoMuestraHistorial()
        {
            // Arrange
            var vm = new DetalleEstudianteViewModel(_datos, _consulta, new SeleccionAlumnoService());
            await vm.CargarListaEstudiantesAsync();

            // Act
            vm.EstudianteSeleccionado = vm.ListaEstudiantes.Single(e => e.Id == 2);

            // Assert
            Assert.Empty(vm.HistorialContactos);
            Assert.Equal("Sin contactar", vm.ContactoTexto);
            Assert.Equal("#F57C00", vm.ColorContacto);
        }

        [Fact]
        public async Task Ficha_AlCambiarDeAlumno_NoArrastraLosContactosDelAnterior()
        {
            // Arrange
            var vm = new DetalleEstudianteViewModel(_datos, _consulta, new SeleccionAlumnoService());
            await vm.CargarListaEstudiantesAsync();
            vm.EstudianteSeleccionado = vm.ListaEstudiantes.Single(e => e.Id == 1);

            // Act
            vm.EstudianteSeleccionado = vm.ListaEstudiantes.Single(e => e.Id == 2);

            // Assert
            Assert.Empty(vm.HistorialContactos);
        }
    }
}
using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.ViewModels;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas del flujo de la pantalla de contactos, con una base y avisos falsos.
    /// </summary>
    public class ContactosViewModelTests
    {
        private readonly DatosEnMemoria _datos = new();
        private readonly DialogoFalso _dialogo = new();

        /// <summary>
        /// Arma una comisión A con dos alumnos y devuelve el ViewModel con las comisiones ya cargadas.
        /// </summary>
        private async Task<ContactosViewModel> CrearConAlumnosAsync()
        {
            _datos.Comisiones.Add(new Comision(1, "A"));
            _datos.Estudiantes.Add(new Estudiante(1, "90000004", "Joaquín", "Pérez", "", 1));
            _datos.Estudiantes.Add(new Estudiante(2, "90000001", "Lucía", "Fernández", "", 1));

            var vm = new ContactosViewModel(_datos, _dialogo);
            await vm.CargarComisionesAsync();
            return vm;
        }

        /// <summary>
        /// Completa el formulario con un contacto válido: hoy a medianoche, que nunca es futuro.
        /// </summary>
        private static void CompletarFormulario(ContactosViewModel vm, string resultado = "Atendió y se comprometió a entregar.")
        {
            vm.EstudianteSeleccionado = vm.ListaEstudiantes.First(e => e.Apellido == "Pérez");
            vm.Fecha = DateTime.Today;
            vm.Hora = TimeSpan.Zero;
            vm.MedioSeleccionado = vm.Medios.First(m => m.Valor == MedioContacto.Llamada);
            vm.Resultado = resultado;
        }

        [Fact]
        public async Task CargarComisiones_ElijeLaPrimeraYMuestraSusAlumnosPorApellido()
        {
            // Act
            var vm = await CrearConAlumnosAsync();

            // Assert
            Assert.Equal("A", vm.ComisionSeleccionada?.Nombre);
            Assert.Equal(new[] { "Fernández", "Pérez" }, vm.ListaEstudiantes.Select(e => e.Apellido));
            Assert.Null(vm.EstudianteSeleccionado);
        }

        [Fact]
        public async Task Guardar_SinAlumno_AvisaYNoGuarda()
        {
            // Arrange
            var vm = await CrearConAlumnosAsync();
            vm.MedioSeleccionado = vm.Medios.First();
            vm.Resultado = "Algo.";

            // Act
            await vm.GuardarAsync();

            // Assert
            Assert.Equal(new[] { "Atención: Elegí un alumno antes de registrar el contacto." }, _dialogo.Avisos);
            Assert.Empty(_datos.Contactos);
        }

        [Fact]
        public async Task Guardar_SinMedio_PideElMedioYNoGuarda()
        {
            // Arrange
            var vm = await CrearConAlumnosAsync();
            CompletarFormulario(vm);
            vm.MedioSeleccionado = null;

            // Act
            await vm.GuardarAsync();

            // Assert
            Assert.Equal(new[] { "Atención: Elegí el medio de contacto." }, _dialogo.Avisos);
            Assert.Empty(_datos.Contactos);
        }

        [Fact]
        public async Task Guardar_SinResultado_PideElResultadoYNoGuarda()
        {
            // Arrange
            var vm = await CrearConAlumnosAsync();
            CompletarFormulario(vm, "   ");

            // Act
            await vm.GuardarAsync();

            // Assert
            Assert.Equal(new[] { "Atención: El resultado del contacto es obligatorio." }, _dialogo.Avisos);
            Assert.Empty(_datos.Contactos);
        }

        [Fact]
        public async Task Guardar_ConFechaFutura_AvisaYNoGuarda()
        {
            // Arrange
            var vm = await CrearConAlumnosAsync();
            CompletarFormulario(vm);
            vm.Fecha = DateTime.Today.AddDays(1);

            // Act
            await vm.GuardarAsync();

            // Assert
            Assert.Equal(new[] { "Atención: La fecha y la hora del contacto no pueden ser futuras." }, _dialogo.Avisos);
            Assert.Empty(_datos.Contactos);
        }

        [Fact]
        public async Task Guardar_ContactoValido_LoGuardaVinculadoAlAlumno()
        {
            // Arrange
            var vm = await CrearConAlumnosAsync();
            CompletarFormulario(vm, "  Atendió y se comprometió a entregar.  ");

            // Act
            await vm.GuardarAsync();

            // Assert
            var guardado = _datos.Contactos.Single();
            Assert.Equal(1, guardado.EstudianteId);
            Assert.Equal(MedioContacto.Llamada, guardado.Medio);
            Assert.Equal(DateTime.Today, guardado.FechaHora);
            Assert.Equal("Atendió y se comprometió a entregar.", guardado.Resultado);
        }

        [Fact]
        public async Task Guardar_ContactoValido_AvisaLimpiaElFormularioYActualizaElHistorial()
        {
            // Arrange
            var vm = await CrearConAlumnosAsync();
            CompletarFormulario(vm);

            // Act
            await vm.GuardarAsync();

            // Assert
            Assert.Equal(new[] { "Éxito: Contacto registrado." }, _dialogo.Avisos);
            Assert.Equal(string.Empty, vm.Resultado);
            Assert.Null(vm.MedioSeleccionado);
            Assert.Single(vm.Historial);
            Assert.Equal("Llamada", vm.Historial[0].Medio);
        }

        [Fact]
        public async Task Guardar_ConLaBaseCaida_AvisaElErrorYConservaLoEscrito()
        {
            // Arrange
            var vm = await CrearConAlumnosAsync();
            CompletarFormulario(vm);
            _datos.FallarAlGuardarContacto = true;

            // Act
            await vm.GuardarAsync();

            // Assert
            Assert.Equal(new[] { "Error: No se pudo registrar el contacto." }, _dialogo.Avisos);
            Assert.Equal("Atendió y se comprometió a entregar.", vm.Resultado);
            Assert.NotNull(vm.MedioSeleccionado);
        }

        [Fact]
        public async Task ElegirUnAlumno_MuestraSusContactosDelMasRecienteAlMasAntiguo()
        {
            // Arrange
            var vm = await CrearConAlumnosAsync();
            _datos.Contactos.Add(new Contacto(1, 1, new DateTime(2026, 10, 21, 16, 0, 0), MedioContacto.Mail, "No respondió."));
            _datos.Contactos.Add(new Contacto(2, 1, new DateTime(2026, 10, 28, 10, 30, 0), MedioContacto.Llamada, "Atendió."));
            _datos.Contactos.Add(new Contacto(3, 2, new DateTime(2026, 10, 27, 18, 15, 0), MedioContacto.Mensaje, "De otro alumno."));

            // Act
            vm.EstudianteSeleccionado = vm.ListaEstudiantes.First(e => e.Apellido == "Pérez");

            // Assert
            Assert.Equal(new[] { "28/10/2026 10:30", "21/10/2026 16:00" }, vm.Historial.Select(f => f.FechaHora));
            Assert.Equal(new[] { "Llamada", "Mail" }, vm.Historial.Select(f => f.Medio));
        }

        [Fact]
        public async Task ElegirUnAlumnoSinContactos_LoAvisaEnElEstado()
        {
            // Arrange
            var vm = await CrearConAlumnosAsync();

            // Act
            vm.EstudianteSeleccionado = vm.ListaEstudiantes.First(e => e.Apellido == "Pérez");

            // Assert
            Assert.Empty(vm.Historial);
            Assert.Equal("Joaquín Pérez todavía no tiene contactos registrados.", vm.Estado);
        }

        [Fact]
        public async Task VolverAMostrarLaPantalla_ConservaLaComisionYElAlumnoElegidos()
        {
            // Arrange
            var vm = await CrearConAlumnosAsync();
            vm.EstudianteSeleccionado = vm.ListaEstudiantes.First(e => e.Apellido == "Pérez");

            // Act: es lo que pasa al volver a la pestaña
            await vm.CargarComisionesAsync();

            // Assert
            Assert.Equal("A", vm.ComisionSeleccionada?.Nombre);
            Assert.Equal("Pérez", vm.EstudianteSeleccionado?.Apellido);
        }

        [Fact]
        public async Task ElContadorDelResultado_SigueLoEscritoYElMaximoEsElDeLaValidacion()
        {
            // Arrange
            var vm = await CrearConAlumnosAsync();

            // Act
            vm.Resultado = "Hola";

            // Assert
            Assert.Equal("4 / 500", vm.ContadorResultado);
            Assert.Equal(500, vm.LargoMaximoResultado);
        }

        [Fact]
        public async Task Medios_SonLosCuatroEnElOrdenDeLaEnumeracion()
        {
            // Act
            var vm = await CrearConAlumnosAsync();

            // Assert
            Assert.Equal(new[] { "Llamada", "Mail", "Presencial", "Mensaje (WhatsApp)" }, vm.Medios.Select(m => m.Texto));
        }
    }
}
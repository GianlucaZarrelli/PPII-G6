using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.ViewModels;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas de la creación de comisiones desde la pantalla de gestión de alumnos, con una base y avisos falsos.
    /// </summary>
    public class CrearComisionTests
    {
        private readonly DatosEnMemoria _datos = new();
        private readonly DialogoFalso _dialogo = new();

        /// <summary>
        /// Arma las comisiones A y B y devuelve el ViewModel, que carga las comisiones al crearse.
        /// </summary>
        private EstudianteViewModel CrearConDosComisiones()
        {
            _datos.Comisiones.Add(new Comision(1, "A"));
            _datos.Comisiones.Add(new Comision(2, "B"));
            return new EstudianteViewModel(_datos, _dialogo);
        }

        [Fact]
        public void SinComisiones_InvitaACrearLaPrimera()
        {
            // Act
            var vm = new EstudianteViewModel(_datos, _dialogo);

            // Assert
            Assert.Equal("Todavía no hay comisiones. Creá la primera con el campo de abajo.", vm.AvisoComisiones);
        }

        [Fact]
        public async Task CrearComision_ConNombreValido_LaGuardaLaMuestraYLaDejaElegida()
        {
            // Arrange
            var vm = CrearConDosComisiones();
            vm.NombreComision = "  D  ";

            // Act
            await vm.CrearComisionAsync();

            // Assert
            Assert.Equal(new[] { "A", "B", "D" }, _datos.Comisiones.Select(c => c.Nombre));
            Assert.Equal(new[] { "A", "B", "D" }, vm.Comisiones.Select(c => c.Nombre));
            Assert.Equal("D", vm.ComisionSeleccionada?.Nombre);
            Assert.Equal(string.Empty, vm.NombreComision);
        }

        [Fact]
        public async Task CrearComision_Valida_AvisaYMuestraSuNominaVacia()
        {
            // Arrange
            var vm = CrearConDosComisiones();
            vm.NombreComision = "D";

            // Act
            await vm.CrearComisionAsync();

            // Assert
            Assert.Equal(new[] { "Éxito: Comisión D creada." }, _dialogo.Avisos);
            Assert.Equal("Esta comisión todavía no tiene alumnos.", vm.EstadoLista);
        }

        [Fact]
        public async Task CrearLaPrimeraComision_QuitaElAvisoDeQueNoHay()
        {
            // Arrange
            var vm = new EstudianteViewModel(_datos, _dialogo);
            vm.NombreComision = "A";

            // Act
            await vm.CrearComisionAsync();

            // Assert
            Assert.Equal(string.Empty, vm.AvisoComisiones);
            Assert.Equal("A", vm.ComisionSeleccionada?.Nombre);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CrearComision_SinNombre_AvisaYNoCrea(string? nombre)
        {
            // Arrange
            var vm = CrearConDosComisiones();
            vm.NombreComision = nombre!;

            // Act
            await vm.CrearComisionAsync();

            // Assert: el aviso va en rojo debajo del campo, sin ventana emergente.
            Assert.Empty(_dialogo.Avisos);
            Assert.Equal("El nombre de la comisión es obligatorio.", vm.ErrorNombreComision);
            Assert.Equal(2, _datos.Comisiones.Count);
        }

        [Fact]
        public async Task CrearComision_Repetida_AvisaAunqueCambienLasMayusculas()
        {
            // Arrange
            var vm = CrearConDosComisiones();
            vm.NombreComision = "a";

            // Act
            await vm.CrearComisionAsync();

            // Assert
            Assert.Empty(_dialogo.Avisos);
            Assert.Equal("Ya existe una comisión con ese nombre.", vm.ErrorNombreComision);
            Assert.Equal(2, _datos.Comisiones.Count);
        }

        [Fact]
        public async Task CrearComision_ConNombreDemasiadoLargo_AvisaElLimiteYNoCrea()
        {
            // Arrange
            var vm = CrearConDosComisiones();
            vm.NombreComision = new string('a', vm.LargoMaximoNombreComision + 1);

            // Act
            await vm.CrearComisionAsync();

            // Assert
            Assert.Empty(_dialogo.Avisos);
            Assert.Equal("El nombre de la comisión admite hasta 100 caracteres.", vm.ErrorNombreComision);
            Assert.Equal(2, _datos.Comisiones.Count);
        }

        [Fact]
        public async Task CrearComision_ConLaBaseCaida_AvisaElErrorYConservaLoEscrito()
        {
            // Arrange
            var vm = CrearConDosComisiones();
            vm.NombreComision = "D";
            _datos.FallarAlCrearComision = true;

            // Act
            await vm.CrearComisionAsync();

            // Assert
            Assert.Equal(new[] { "Error: No se pudo crear la comisión. Revisá la conexión con la base de datos." }, _dialogo.Avisos);
            Assert.Equal("D", vm.NombreComision);
            Assert.Equal(2, vm.Comisiones.Count);
        }
    }
}
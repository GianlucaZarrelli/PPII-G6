using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.ViewModels;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas del flujo de la pantalla del calendario, con una base y avisos falsos.
    /// </summary>
    public class CalendarioViewModelTests
    {
        private readonly DatosEnMemoria _datos = new();
        private readonly DialogoFalso _dialogo = new();

        /// <summary>
        /// Arma las comisiones A y C, con una clase y un trabajo en la A, y devuelve el ViewModel con todo cargado.
        /// </summary>
        private async Task<CalendarioViewModel> CrearConCalendarioAsync()
        {
            _datos.Comisiones.Add(new Comision(1, "A"));
            _datos.Comisiones.Add(new Comision(2, "C"));
            _datos.Clases.Add(new Clase(1, new DateTime(2026, 9, 7), "Introducción", 1));
            _datos.Clases.Add(new Clase(2, new DateTime(2026, 9, 14), "Servicios", 2));
            _datos.Trabajos.Add(new TrabajoPractico(1, 1, "TP1", new DateTime(2026, 9, 30)));

            var vm = new CalendarioViewModel(_datos, _dialogo);
            await vm.CargarComisionesAsync();
            return vm;
        }

        [Fact]
        public async Task Cargar_MuestraElCalendarioDeLaPrimeraComisionOrdenadoPorFecha()
        {
            // Act
            var vm = await CrearConCalendarioAsync();

            // Assert
            Assert.Equal("A", vm.ComisionSeleccionada?.Nombre);
            Assert.Equal(new[] { "Introducción", "TP1" }, vm.Filas.Select(f => f.Titulo));
            Assert.Equal(new[] { "Clase", "Entrega de trabajo" }, vm.Filas.Select(f => f.Tipo));
            Assert.Equal("Comisión A  |  Clases: 1  |  Trabajos: 1", vm.Estado);
        }

        [Fact]
        public async Task CambiarDeComision_MuestraSoloSusFechas()
        {
            // Arrange
            var vm = await CrearConCalendarioAsync();

            // Act
            vm.ComisionSeleccionada = vm.Comisiones.Single(c => c.Nombre == "C");

            // Assert
            Assert.Equal(new[] { "Servicios" }, vm.Filas.Select(f => f.Titulo));
        }

        [Fact]
        public async Task UnaComisionSinFechas_LoAvisaEnElEstado()
        {
            // Arrange
            var vm = await CrearConCalendarioAsync();
            _datos.Comisiones.Add(new Comision(3, "B"));
            await vm.CargarComisionesAsync();

            // Act
            vm.ComisionSeleccionada = vm.Comisiones.Single(c => c.Nombre == "B");

            // Assert
            Assert.Empty(vm.Filas);
            Assert.Equal("La comisión B todavía no tiene fechas cargadas.", vm.Estado);
        }

        [Fact]
        public async Task AgregarClase_Valida_LaGuardaEnLaComisionYActualizaElCalendario()
        {
            // Arrange
            var vm = await CrearConCalendarioAsync();
            vm.FechaClase = new DateTime(DateTime.Today.Year, 12, 1);
            vm.DescripcionClase = "  Repaso final  ";

            // Act
            await vm.AgregarClaseAsync();

            // Assert
            var guardada = _datos.Clases.Single(c => c.Descripcion == "Repaso final");
            Assert.Equal(1, guardada.ComisionId);
            Assert.Equal(new DateTime(DateTime.Today.Year, 12, 1), guardada.Fecha);
            Assert.Equal(new[] { "Éxito: Clase agregada al calendario." }, _dialogo.Avisos);
            Assert.Equal(3, vm.Filas.Count);
            Assert.Equal(string.Empty, vm.DescripcionClase);
        }

        [Fact]
        public async Task AgregarClase_SinDescripcion_LaGuardaIgualYLaMuestraComoClase()
        {
            // Arrange
            var vm = await CrearConCalendarioAsync();
            vm.FechaClase = new DateTime(DateTime.Today.Year, 12, 2);

            // Act
            await vm.AgregarClaseAsync();

            // Assert
            Assert.Equal(3, vm.Filas.Count);
            Assert.Equal("Clase", vm.Filas.Last().Titulo);
        }

        [Fact]
        public async Task AgregarClase_ConUnaFechaYaCargada_AvisaYNoLaDuplica()
        {
            // Arrange
            var vm = await CrearConCalendarioAsync();
            vm.FechaClase = new DateTime(2026, 9, 7);

            // Act
            await vm.AgregarClaseAsync();

            // Assert
            Assert.Equal(new[] { "Atención: Ya hay una clase cargada ese día en la comisión." }, _dialogo.Avisos);
            Assert.Equal(2, _datos.Clases.Count);
        }

        [Fact]
        public async Task AgregarClase_ConFechaDeOtroAnio_AvisaElRangoYNoLaGuarda()
        {
            // Arrange
            var vm = await CrearConCalendarioAsync();
            vm.FechaClase = DateTime.Today.AddYears(1);

            // Act
            await vm.AgregarClaseAsync();

            // Assert
            Assert.StartsWith("Atención: La fecha de la clase tiene que estar entre el 01/01/", _dialogo.Avisos.Single());
            Assert.Equal(2, _datos.Clases.Count);
        }

        [Fact]
        public async Task AgregarClase_SinComisionElegida_PideElegirUna()
        {
            // Arrange
            var vm = new CalendarioViewModel(_datos, _dialogo);

            // Act
            await vm.AgregarClaseAsync();

            // Assert
            Assert.Equal(new[] { "Atención: Elegí una comisión antes de agregar una clase." }, _dialogo.Avisos);
        }

        [Fact]
        public async Task AgregarClase_ConUnaDescripcionDemasiadoLarga_AvisaYNoLaGuarda()
        {
            // Arrange
            var vm = await CrearConCalendarioAsync();
            vm.FechaClase = new DateTime(DateTime.Today.Year, 12, 3);
            vm.DescripcionClase = new string('a', vm.LargoMaximoDescripcion + 1);

            // Act
            await vm.AgregarClaseAsync();

            // Assert
            Assert.Equal(new[] { "Atención: La descripción admite hasta 200 caracteres." }, _dialogo.Avisos);
            Assert.Equal(2, _datos.Clases.Count);
        }
    }
}
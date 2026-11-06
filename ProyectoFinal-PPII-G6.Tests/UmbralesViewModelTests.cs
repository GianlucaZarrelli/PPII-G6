using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;
using ProyectoFinal_PPII_G6.ViewModels;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas del flujo de la pantalla de umbrales, con una base y avisos falsos.
    /// </summary>
    public class UmbralesViewModelTests
    {
        private readonly DatosEnMemoria _datos = new();
        private readonly DialogoFalso _dialogo = new();
        private readonly UmbralesViewModel _vm;

        /// <summary>
        /// Arma tres alumnos con 100, 80 y 60 % de asistencia (verde, amarillo y rojo con los umbrales por defecto).
        /// </summary>
        public UmbralesViewModelTests()
        {
            _datos.Comisiones.Add(new Comision(1, "A"));
            _datos.AgregarAlumno(1, "Lucía", "Fernández", 1);
            _datos.AgregarAlumno(2, "Mateo", "Gómez", 1);
            _datos.AgregarAlumno(3, "Joaquín", "Pérez", 1);

            for (var clase = 1; clase <= 5; clase++)
            {
                _datos.Clases.Add(new Clase(clase, new DateTime(2026, 9, 1).AddDays(clase), null, 1));
                _datos.Asistencias.Add(new Asistencia(clase * 10 + 1, 1, clase, true, false));
                _datos.Asistencias.Add(new Asistencia(clase * 10 + 2, 2, clase, clase <= 4, false));
                _datos.Asistencias.Add(new Asistencia(clase * 10 + 3, 3, clase, clase <= 3, false));
            }

            _vm = new UmbralesViewModel(_datos, new ConsultaSeguimiento(_datos, new SeguimientoService(new RiesgoService())), _dialogo);
        }

        [Fact]
        public async Task Cargar_MuestraLosUmbralesGuardadosYCuantosAlumnosHayEnCadaNivel()
        {
            // Act
            await _vm.CargarAsync();

            // Assert
            Assert.Equal("75", _vm.AsistenciaRojo);
            Assert.Equal("85", _vm.AsistenciaAmarillo);
            Assert.Equal("60", _vm.EntregasRojo);
            Assert.Equal("80", _vm.EntregasAmarillo);
            Assert.Equal("ALTO RIESGO: 1  |  RIESGO MODERADO: 1  |  AL DÍA: 1  |  SIN DATOS: 0", _vm.ResumenNiveles);
        }

        [Fact]
        public async Task Guardar_ConValoresValidos_LosGuardaYRecalculaALosAlumnos()
        {
            // Arrange: umbrales más permisivos
            await _vm.CargarAsync();
            _vm.AsistenciaRojo = "50";
            _vm.AsistenciaAmarillo = "70";

            // Act
            await _vm.GuardarAsync();

            // Assert
            Assert.Equal(50, _datos.Umbrales.AsistenciaRojo);
            Assert.Equal(70, _datos.Umbrales.AsistenciaAmarillo);
            Assert.Equal("ALTO RIESGO: 0  |  RIESGO MODERADO: 1  |  AL DÍA: 2  |  SIN DATOS: 0", _vm.ResumenNiveles);
            Assert.Equal(new[] { "Éxito: Umbrales guardados. Se recalculó el riesgo de 3 alumnos." }, _dialogo.Avisos);
        }

        [Fact]
        public async Task Guardar_ConUnValorQueNoEsNumero_AvisaYNoGuarda()
        {
            // Arrange
            await _vm.CargarAsync();
            _vm.EntregasRojo = "mucho";

            // Act
            await _vm.GuardarAsync();

            // Assert
            Assert.Equal(new[] { "Atención: Los cuatro umbrales tienen que ser números enteros." }, _dialogo.Avisos);
            Assert.Equal(60, _datos.Umbrales.EntregasRojo);
        }

        [Fact]
        public async Task Guardar_ConUnPorcentajeFueraDeRango_AvisaYNoGuarda()
        {
            // Arrange
            await _vm.CargarAsync();
            _vm.AsistenciaAmarillo = "150";

            // Act
            await _vm.GuardarAsync();

            // Assert
            Assert.Equal(new[] { "Atención: Los umbrales son porcentajes: tienen que estar entre 0 y 100." }, _dialogo.Avisos);
            Assert.Equal(85, _datos.Umbrales.AsistenciaAmarillo);
        }

        [Fact]
        public async Task Guardar_ConAltoRiesgoMayorQueModerado_AvisaYNoGuarda()
        {
            // Arrange
            await _vm.CargarAsync();
            _vm.AsistenciaRojo = "90";

            // Act
            await _vm.GuardarAsync();

            // Assert
            Assert.Equal(new[] { "Atención: En la asistencia, el umbral de alto riesgo tiene que ser menor que el de riesgo moderado." }, _dialogo.Avisos);
            Assert.Equal(75, _datos.Umbrales.AsistenciaRojo);
        }

        [Fact]
        public async Task Guardar_AceptaEspaciosAlrededorDeLosNumeros()
        {
            // Arrange
            await _vm.CargarAsync();
            _vm.AsistenciaRojo = "  50 ";

            // Act
            await _vm.GuardarAsync();

            // Assert
            Assert.Equal(50, _datos.Umbrales.AsistenciaRojo);
        }

        [Fact]
        public async Task Restablecer_VuelveALosValoresPorDefectoYLosGuarda()
        {
            // Arrange: primero se guardan otros umbrales
            await _vm.CargarAsync();
            _vm.AsistenciaRojo = "50";
            _vm.AsistenciaAmarillo = "70";
            await _vm.GuardarAsync();

            // Act
            await _vm.RestablecerAsync();

            // Assert
            Assert.Equal("75", _vm.AsistenciaRojo);
            Assert.Equal(75, _datos.Umbrales.AsistenciaRojo);
            Assert.Equal(85, _datos.Umbrales.AsistenciaAmarillo);
            Assert.Equal("ALTO RIESGO: 1  |  RIESGO MODERADO: 1  |  AL DÍA: 1  |  SIN DATOS: 0", _vm.ResumenNiveles);
        }

        [Fact]
        public async Task Guardar_ConLaBaseCaida_AvisaElErrorYConservaLoEscrito()
        {
            // Arrange
            await _vm.CargarAsync();
            _vm.AsistenciaRojo = "50";
            _datos.FallarAlGuardarUmbrales = true;

            // Act
            await _vm.GuardarAsync();

            // Assert
            Assert.Equal(new[] { "Error: No se pudieron guardar los umbrales. Revisá la conexión con la base de datos." }, _dialogo.Avisos);
            Assert.Equal("50", _vm.AsistenciaRojo);
            Assert.Equal(75, _datos.Umbrales.AsistenciaRojo);
        }
    }
}
using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;
using ProyectoFinal_PPII_G6.ViewModels;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas del flujo de la pantalla del reporte de una comisión, con una base falsa.
    /// </summary>
    public class ReporteViewModelTests
    {
        private readonly DatosEnMemoria _datos = new();
        private readonly ReporteViewModel _vm;

        public ReporteViewModelTests()
        {
            var consulta = new ConsultaSeguimiento(_datos, new SeguimientoService(new RiesgoService()));
            _vm = new ReporteViewModel(_datos, consulta);
        }

        /// <summary>
        /// Arma una comisión con 6 clases y 3 trabajos ya vencidos y los alumnos indicados: "alDia" fueron a las 6 clases
        /// y entregaron los 3 trabajos, "moderados" fueron a 5 clases y entregaron los 3, y "enRiesgo" fueron a 2 clases y
        /// entregaron 1. Con "conRegistros" en false, nadie registró asistencias ni entregas.
        /// </summary>
        private void ArmarComision(int id, string nombre, int alDia = 0, int moderados = 0, int enRiesgo = 0, bool conRegistros = true)
        {
            _datos.Comisiones.Add(new Comision(id, nombre));

            for (var i = 1; i <= 6; i++)
            {
                _datos.Clases.Add(new Clase(id * 100 + i, new DateTime(2026, 9, 1).AddDays(i * 7), null, id));
            }

            for (var i = 1; i <= 3; i++)
            {
                _datos.Trabajos.Add(new TrabajoPractico(id * 100 + i, id, $"TP{i}", new DateTime(2026, 9, 10).AddDays(i * 7)));
            }

            for (var k = 1; k <= alDia + moderados + enRiesgo; k++)
            {
                var alumno = _datos.AgregarAlumno(id * 1000 + k, $"Alumno{k}", nombre, id);
                if (!conRegistros) continue;

                var (presentes, entregados) = k <= alDia ? (6, 3) : k <= alDia + moderados ? (5, 3) : (2, 1);
                for (var c = 1; c <= 6; c++) _datos.Asistencias.Add(new Asistencia(alumno.Id * 10 + c, alumno.Id, id * 100 + c, c <= presentes, false));
                for (var t = 1; t <= 3; t++) _datos.Entregas.Add(new Entrega(alumno.Id * 10 + t, alumno.Id, id * 100 + t, t <= entregados));
            }
        }

        [Fact]
        public async Task Cargar_ElijeLaPrimeraComisionYMuestraElTitularYLasBarras()
        {
            // Arrange: 1°A tiene 4 estudiantes (2 al día, 1 moderado, 1 en alto riesgo)
            ArmarComision(1, "1°A", alDia: 2, moderados: 1, enRiesgo: 1);
            ArmarComision(2, "1°B", conRegistros: false);

            // Act
            await _vm.CargarComisionesAsync();

            // Assert
            Assert.Equal("1°A", _vm.ComisionSeleccionada?.Nombre);
            Assert.Equal("1 de 4 estudiantes en alto riesgo de abandonar", _vm.Titular);
            Assert.Equal("RIESGO MODERADO: 1  |  AL DÍA: 2  |  SIN DATOS: 0", _vm.Detalle);
            Assert.True(_vm.HayGrafico);
            Assert.Equal(string.Empty, _vm.Estado);
            Assert.Equal(new[] { "ALTO RIESGO", "RIESGO MODERADO", "AL DÍA", "SIN DATOS" }, _vm.Filas.Select(f => f.Nivel));
            Assert.Equal(new[] { 1, 1, 2, 0 }, _vm.Filas.Select(f => f.Cantidad));
            Assert.Equal(new[] { 65.0, 65.0, 130.0, 0.0 }, _vm.Filas.Select(f => f.Ancho));
            Assert.Equal("2  (50 %)", _vm.Filas[2].Resumen);
        }

        [Fact]
        public async Task CambiarDeComision_ConEstudiantesSinRegistros_DiceQueNoHayDatosYNoMuestraBarras()
        {
            // Arrange
            ArmarComision(1, "1°A", alDia: 2, enRiesgo: 1);
            ArmarComision(2, "1°B", enRiesgo: 5, conRegistros: false);
            await _vm.CargarComisionesAsync();

            // Act
            _vm.ComisionSeleccionada = _vm.Comisiones.Single(c => c.Nombre == "1°B");

            // Assert
            Assert.Equal("Todavía no hay datos para calcular el riesgo de esta comisión.", _vm.Titular);
            Assert.False(_vm.HayGrafico);
            Assert.Empty(_vm.Filas);
            Assert.Equal(string.Empty, _vm.Detalle);
        }

        [Fact]
        public async Task ComisionSinEstudiantes_DiceQueNoTieneEstudiantes()
        {
            // Arrange
            _datos.Comisiones.Add(new Comision(1, "1°C"));

            // Act
            await _vm.CargarComisionesAsync();

            // Assert
            Assert.Equal("La comisión todavía no tiene estudiantes.", _vm.Titular);
            Assert.False(_vm.HayGrafico);
        }

        [Fact]
        public async Task SinComisiones_InvitaACrearLaPrimera()
        {
            // Act
            await _vm.CargarComisionesAsync();

            // Assert
            Assert.Equal("Todavía no hay comisiones. Creá la primera en Carga de alumnos.", _vm.Estado);
            Assert.Equal(string.Empty, _vm.Titular);
            Assert.False(_vm.HayGrafico);
        }

        [Fact]
        public async Task ConLaBaseCaida_LoDiceEnPantallaEnVezDeMostrarCeroEnRiesgo()
        {
            // Arrange
            ArmarComision(1, "1°A", alDia: 3, enRiesgo: 1);
            _datos.FallarAlConsultar = true;

            // Act
            await _vm.CargarComisionesAsync();

            // Assert
            Assert.Equal("No se pudo cargar el reporte. Revisá la conexión con la base de datos.", _vm.Estado);
            Assert.Equal(string.Empty, _vm.Titular);
            Assert.False(_vm.HayGrafico);
            Assert.Empty(_vm.Filas);
        }

        [Fact]
        public async Task Recargar_ConLasMismasComisiones_ConservaLaElegidaYActualizaElReporte()
        {
            // Arrange
            ArmarComision(1, "1°A", alDia: 2, enRiesgo: 1);
            ArmarComision(2, "1°B", alDia: 1, enRiesgo: 3);
            await _vm.CargarComisionesAsync();
            _vm.ComisionSeleccionada = _vm.Comisiones.Single(c => c.Nombre == "1°B");

            // Act: el tutor vuelve a la pantalla
            await _vm.CargarComisionesAsync();

            // Assert
            Assert.Equal("1°B", _vm.ComisionSeleccionada?.Nombre);
            Assert.Equal("3 de 4 estudiantes en alto riesgo de abandonar", _vm.Titular);
        }
    }
}
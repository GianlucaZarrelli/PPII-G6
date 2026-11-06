using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Presentacion;
using ProyectoFinal_PPII_G6.Services;
using ProyectoFinal_PPII_G6.ViewModels;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas de qué pasa al tocar un alumno en el panel: se anota cuál abrir y se va al Historial.
    /// </summary>
    public class PanelRiesgoNavegacionTests
    {
        private readonly DatosEnMemoria _datos = new();
        private readonly NavegacionFalsa _navegacion = new();
        private readonly SeleccionAlumnoService _seleccion = new();

        /// <summary>
        /// Arma una comisión con dos alumnos y devuelve el panel ya cargado, con el más grave arriba.
        /// </summary>
        private async Task<PanelRiesgoViewModel> CrearPanelCargadoAsync()
        {
            _datos.Comisiones.Add(new Comision(1, "A"));
            _datos.AgregarAlumno(1, "Lucía", "Fernández", 1);
            _datos.AgregarAlumno(2, "Mateo", "Gómez", 1);

            for (var clase = 1; clase <= 5; clase++)
            {
                _datos.Clases.Add(new Clase(clase, new DateTime(2026, 9, 1).AddDays(clase), null, 1));
                _datos.Asistencias.Add(new Asistencia(clase * 10 + 1, 1, clase, true, false));
                _datos.Asistencias.Add(new Asistencia(clase * 10 + 2, 2, clase, clase == 1, false));
            }

            var vm = new PanelRiesgoViewModel(
                _datos, new ConsultaSeguimiento(_datos, new SeguimientoService(new RiesgoService())), _seleccion, _navegacion);
            await vm.CargarAsync();
            return vm;
        }

        [Fact]
        public async Task TocarUnAlumno_AnotaCualAbrirYVaAlHistorial()
        {
            // Arrange
            var vm = await CrearPanelCargadoAsync();
            var mateo = vm.Alumnos.Single(a => a.NombreCompleto.Contains("Mateo"));

            // Act
            mateo.AbrirFichaCommand!.Execute(null);

            // Assert
            Assert.Equal(2, _seleccion.TomarPendiente());
            Assert.Equal(new[] { Rutas.Historial }, _navegacion.Visitadas);
        }

        [Fact]
        public async Task CadaFila_AbreLaFichaDeSuPropioAlumno()
        {
            // Arrange
            var vm = await CrearPanelCargadoAsync();
            var lucia = vm.Alumnos.Single(a => a.NombreCompleto.Contains("Lucía"));

            // Act
            lucia.AbrirFichaCommand!.Execute(null);

            // Assert
            Assert.Equal(1, _seleccion.TomarPendiente());
        }

        [Fact]
        public async Task TocarUnAlumno_NoCambiaDePantallaHastaQueSeToca()
        {
            // Act
            await CrearPanelCargadoAsync();

            // Assert
            Assert.Empty(_navegacion.Visitadas);
            Assert.Null(_seleccion.TomarPendiente());
        }
    }
}
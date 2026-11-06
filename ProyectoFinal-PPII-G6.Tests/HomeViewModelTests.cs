using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Presentacion;
using ProyectoFinal_PPII_G6.Services;
using ProyectoFinal_PPII_G6.ViewModels;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas de la pantalla de Inicio: la navegación, el cambio de tema y las métricas, con servicios falsos.
    /// </summary>
    public class HomeViewModelTests
    {
        private readonly DatosEnMemoria _datos = new();
        private readonly NavegacionFalsa _navegacion = new();
        private readonly TemaFalso _tema = new();

        private HomeViewModel CrearViewModel() =>
            new(new ConsultaSeguimiento(_datos, new SeguimientoService(new RiesgoService())), _navegacion, _tema);

        /// <summary>
        /// Arma una comisión con 5 clases ya dadas y dos alumnos: uno con asistencia perfecta y otro que casi no fue.
        /// </summary>
        private void ArmarComisionConUnAlumnoEnRiesgo()
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
        }

        [Fact]
        public void IrAAlumnos_NavegaALaGestionDeAlumnos()
        {
            // Arrange
            var vm = CrearViewModel();

            // Act
            vm.IrAAlumnosCommand.Execute(null);

            // Assert
            Assert.Equal(new[] { Rutas.Alumnos }, _navegacion.Visitadas);
        }

        [Fact]
        public void IrAHistorial_NavegaAlHistorial()
        {
            // Arrange
            var vm = CrearViewModel();

            // Act
            vm.IrAHistorialCommand.Execute(null);

            // Assert
            Assert.Equal(new[] { Rutas.Historial }, _navegacion.Visitadas);
        }

        [Fact]
        public void Rutas_SonLasDeAppShell()
        {
            // Assert: los textos que usa el código para navegar son las rutas de las pestañas
            Assert.Equal("GestorEstudiantes", Rutas.Alumnos);
            Assert.Equal("HistorialEstudiantes", Rutas.Historial);
        }

        [Fact]
        public void CambiarElInterruptor_CambiaElTemaDeLaAplicacion()
        {
            // Arrange
            var vm = CrearViewModel();

            // Act
            vm.TemaOscuro = true;
            var oscuroDespuesDeActivar = _tema.EsOscuro;
            vm.TemaOscuro = false;

            // Assert
            Assert.True(oscuroDespuesDeActivar);
            Assert.False(_tema.EsOscuro);
        }

        [Fact]
        public async Task AlMostrarse_ElInterruptorRefleja_ElTemaActual_SinVolverAAplicarlo()
        {
            // Arrange: la aplicación ya está en oscuro (por ejemplo, por la configuración del sistema)
            _tema.EsOscuro = true;
            var vm = CrearViewModel();

            // Act
            await vm.CargarMetricasAsync();

            // Assert
            Assert.True(vm.TemaOscuro);
            Assert.True(_tema.EsOscuro);
        }

        [Fact]
        public async Task CargarMetricas_CuentaLosAlumnosYLosQueEstanEnRiesgoAlto()
        {
            // Arrange
            ArmarComisionConUnAlumnoEnRiesgo();
            var vm = CrearViewModel();

            // Act
            await vm.CargarMetricasAsync();

            // Assert
            Assert.Equal(2, vm.TotalEstudiantes);
            Assert.Equal(1, vm.EstudiantesEnRiesgo);
        }
    }
}
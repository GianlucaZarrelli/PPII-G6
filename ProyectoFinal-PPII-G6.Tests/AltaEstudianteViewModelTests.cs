using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;
using ProyectoFinal_PPII_G6.ViewModels;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas del alta manual de alumnos en la pantalla de gestión, con una base y avisos falsos.
    /// Los errores aparecen debajo de cada campo, sin ventana emergente.
    /// </summary>
    public class AltaEstudianteViewModelTests
    {
        private readonly DatosEnMemoria _datos = new();
        private readonly DialogoFalso _dialogo = new();

        /// <summary>
        /// Arma la comisión 1°A, con un alumno ya cargado (DNI 12345678, Pérez, Juan), y deja el ViewModel con ella elegida.
        /// </summary>
        private EstudianteViewModel CrearConUnAlumnoCargado()
        {
            _datos.Comisiones.Add(new Comision(1, "1°A"));
            _datos.Estudiantes.Add(new Estudiante(1, "12345678", "Juan", "Pérez", "juan@mail.com", 1));

            var vm = new EstudianteViewModel(_datos, _dialogo, new RegistrarEstudianteService(_datos));
            vm.ComisionSeleccionada = vm.Comisiones.Single();
            return vm;
        }

        private static void Completar(EstudianteViewModel vm, string dni, string nombre, string apellido, string email)
        {
            vm.Dni = dni;
            vm.Nombre = nombre;
            vm.Apellido = apellido;
            vm.Email = email;
        }

        [Fact]
        public void Guardar_ConDatosValidos_LoAgregaALaNominaYAvisa()
        {
            // Arrange
            var vm = CrearConUnAlumnoCargado();
            Completar(vm, "30.123.456", " Ana ", "Gómez", "ana@mail.com");

            // Act
            vm.GuardarEstudianteCommand.Execute(null);

            // Assert
            Assert.Equal(2, vm.Estudiantes.Count);
            Assert.Contains("30123456", vm.Estudiantes.Select(e => e.Dni));
            Assert.Equal(new[] { "Éxito: Estudiante registrado en la comisión 1°A." }, _dialogo.Avisos);
            Assert.Equal(string.Empty, vm.Dni);
        }

        [Fact]
        public void Guardar_ConUnDniYaCargado_MuestraAQuienCorrespondeDebajoDelCampoYNoGuarda()
        {
            // Arrange
            var vm = CrearConUnAlumnoCargado();
            Completar(vm, "12.345.678", "Ana", "Gómez", "ana@mail.com");

            // Act
            vm.GuardarEstudianteCommand.Execute(null);

            // Assert
            Assert.Equal("El DNI 12345678 ya está cargado para Pérez, Juan.", vm.ErrorDni);
            Assert.Equal(1, vm.Estudiantes.Count);
            Assert.Empty(_dialogo.Avisos);
            Assert.Equal("Ana", vm.Nombre);
        }

        [Fact]
        public void Guardar_ConCamposVacios_MuestraUnErrorDebajoDeCadaUnoYNoGuarda()
        {
            // Arrange
            var vm = CrearConUnAlumnoCargado();

            // Act
            vm.GuardarEstudianteCommand.Execute(null);

            // Assert
            Assert.Equal("El DNI es obligatorio.", vm.ErrorDni);
            Assert.Equal("Ingresá un nombre.", vm.ErrorNombre);
            Assert.Equal("Ingresá un apellido.", vm.ErrorApellido);
            Assert.Equal("Ingresá un email.", vm.ErrorEmail);
            Assert.Equal(1, _datos.Estudiantes.Count);
        }

        [Fact]
        public void Guardar_ConLaBaseCaida_AvisaElErrorYConservaLoEscrito()
        {
            // Arrange
            var vm = CrearConUnAlumnoCargado();
            Completar(vm, "30123456", "Ana", "Gómez", "ana@mail.com");
            _datos.FallarAlGuardarEstudiante = true;

            // Act
            vm.GuardarEstudianteCommand.Execute(null);

            // Assert
            Assert.Equal(new[] { "Error: No se pudo guardar el estudiante. Revisá la conexión con la base de datos." }, _dialogo.Avisos);
            Assert.Equal("Ana", vm.Nombre);
            Assert.Equal(1, _datos.Estudiantes.Count);
        }
    }
}
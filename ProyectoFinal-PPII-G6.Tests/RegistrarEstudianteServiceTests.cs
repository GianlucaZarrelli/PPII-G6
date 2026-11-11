using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas del alta de un estudiante: el DNI se guarda normalizado y no se repite dentro de una comisión.
    /// </summary>
    public class RegistrarEstudianteServiceTests
    {
        private readonly DatosEnMemoria _datos = new();
        private readonly RegistrarEstudianteService _registrar;

        public RegistrarEstudianteServiceTests()
        {
            _datos.Comisiones.Add(new Comision(1, "1°A"));
            _datos.Comisiones.Add(new Comision(2, "1°B"));
            _registrar = new RegistrarEstudianteService(_datos);
        }

        private static Estudiante Alumno(string dni, string nombre, string apellido, int comisionId = 1) =>
            new(0, dni, nombre, apellido, "alumno@mail.com", comisionId);

        [Fact]
        public async Task Registrar_ConUnDniNuevo_LoGuardaSinPuntosNiEspacios()
        {
            // Act
            var rechazo = await _registrar.RegistrarAsync(Alumno(" 12.345.678 ", "Juan", "Pérez"));

            // Assert
            Assert.Null(rechazo);
            Assert.Equal("12345678", Assert.Single(_datos.Estudiantes).Dni);
        }

        [Fact]
        public async Task Registrar_ConUnDniYaCargado_SeRechazaDiciendoAQuienCorresponde()
        {
            // Arrange
            await _registrar.RegistrarAsync(Alumno("12345678", "Juan", "Pérez"));

            // Act
            var rechazo = await _registrar.RegistrarAsync(Alumno("12345678", "Ana", "Gómez"));

            // Assert
            Assert.Equal("El DNI 12345678 ya está cargado para Pérez, Juan.", rechazo);
            Assert.Single(_datos.Estudiantes);
        }

        [Theory]
        [InlineData("12.345.678")]
        [InlineData("12 345 678")]
        [InlineData("  12345678  ")]
        public async Task Registrar_ConElMismoDniEscritoDeOtraForma_TambienSeRechaza(string escrito)
        {
            // Arrange
            await _registrar.RegistrarAsync(Alumno("12345678", "Juan", "Pérez"));

            // Act
            var rechazo = await _registrar.RegistrarAsync(Alumno(escrito, "Ana", "Gómez"));

            // Assert
            Assert.Equal("El DNI 12345678 ya está cargado para Pérez, Juan.", rechazo);
            Assert.Single(_datos.Estudiantes);
        }

        [Fact]
        public async Task Registrar_ConElMismoDniEnOtraComision_LoGuarda()
        {
            // Arrange
            await _registrar.RegistrarAsync(Alumno("12345678", "Juan", "Pérez", comisionId: 1));

            // Act
            var rechazo = await _registrar.RegistrarAsync(Alumno("12345678", "Juan", "Pérez", comisionId: 2));

            // Assert
            Assert.Null(rechazo);
            Assert.Equal(2, _datos.Estudiantes.Count);
        }

        [Theory]
        [InlineData("", "El DNI es obligatorio.")]
        [InlineData("12.34a.678", "El DNI solo puede tener números.")]
        [InlineData("12345", "El DNI tiene que tener entre 6 y 9 dígitos.")]
        public async Task Registrar_ConUnDniInvalido_SeRechazaYNoGuardaNada(string dni, string motivo)
        {
            // Act
            var rechazo = await _registrar.RegistrarAsync(Alumno(dni, "Juan", "Pérez"));

            // Assert
            Assert.Equal(motivo, rechazo);
            Assert.Empty(_datos.Estudiantes);
        }
    }
}
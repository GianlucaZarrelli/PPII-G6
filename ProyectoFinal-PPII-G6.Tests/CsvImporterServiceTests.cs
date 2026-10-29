using System.Text;
using ProyectoFinal_PPII_G6.Services;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas unitarias de la lectura de archivos CSV de alumnos.
    /// </summary>
    public class CsvImporterServiceTests
    {
        private readonly CsvImporterService _importador = new();

        [Fact]
        public async Task ParseCsvAsync_OmiteElEncabezadoYLeeLasFilas()
        {
            // Arrange
            var csv = "Dni;Nombre;Apellido;Email\n30123456;Ana;Perez;ana@demo.test\n30123457;Bruno;Castro;bruno@demo.test";

            // Act
            var alumnos = await Leer(csv);

            // Assert
            Assert.Equal(2, alumnos.Count);
            Assert.Equal("30123456", alumnos[0].Dni);
            Assert.Equal("Bruno", alumnos[1].Nombre);
        }

        [Fact]
        public async Task ParseCsvAsync_AceptaComaComoSeparador()
        {
            // Arrange
            var csv = "Dni,Nombre,Apellido,Email\n30123456,Ana,Perez,ana@demo.test";

            // Act
            var alumnos = await Leer(csv);

            // Assert
            Assert.Equal(1, alumnos.Count);
            Assert.Equal("Perez", alumnos[0].Apellido);
        }

        [Fact]
        public async Task ParseCsvAsync_IgnoraLineasVaciasYFilasConMenosDeTresColumnas()
        {
            // Arrange
            var csv = "Dni;Nombre;Apellido;Email\n\n30123456;Ana;Perez;ana@demo.test\n   \n30123457;Bruno\n30123458;Carla;Mendez;carla@demo.test";

            // Act
            var alumnos = await Leer(csv);

            // Assert: quedan las dos filas completas
            Assert.Equal(2, alumnos.Count);
            Assert.Equal("30123458", alumnos[1].Dni);
        }

        [Fact]
        public async Task ParseCsvAsync_SinColumnaDeEmail_DejaElEmailVacio()
        {
            // Arrange
            var csv = "Dni;Nombre;Apellido\n30123456;Ana;Perez";

            // Act
            var alumnos = await Leer(csv);

            // Assert
            Assert.Equal(string.Empty, alumnos[0].Email);
        }

        [Fact]
        public async Task ParseCsvAsync_RecortaLosEspacios()
        {
            // Arrange
            var csv = "Dni;Nombre;Apellido;Email\n 30123456 ; Ana ; Perez ; ana@demo.test ";

            // Act
            var alumnos = await Leer(csv);

            // Assert
            Assert.Equal("30123456", alumnos[0].Dni);
            Assert.Equal("Ana", alumnos[0].Nombre);
            Assert.Equal("ana@demo.test", alumnos[0].Email);
        }

        [Fact]
        public async Task ParseCsvAsync_AsignaLaComisionRecibida()
        {
            // Arrange
            var csv = "Dni;Nombre;Apellido;Email\n30123456;Ana;Perez;ana@demo.test";

            // Act
            var alumnos = await Leer(csv, comisionId: 7);

            // Assert
            Assert.Equal(7, alumnos[0].ComisionId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("30123456;Ana")]
        public void ParsearLinea_LineaSinDatosSuficientes_RetornaNull(string? linea)
        {
            // Act
            var alumno = CsvImporterService.ParsearLinea(linea, 1);

            // Assert
            Assert.Null(alumno);
        }

        private async Task<List<ProyectoFinal_PPII_G6.Models.Estudiante>> Leer(string csv, int comisionId = 1)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
            return await _importador.ParseCsvAsync(stream, comisionId);
        }
    }
}
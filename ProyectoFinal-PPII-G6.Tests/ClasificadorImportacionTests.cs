using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas unitarias de qué se hace con cada fila de un archivo de importación.
    /// </summary>
    public class ClasificadorImportacionTests
    {
        [Fact]
        public void Clasificar_FilaCompleta_EsNueva()
        {
            // Act
            var resultado = Clasificar(Alumno("30123456", "Ana", "Perez", "ana@demo.test"));

            // Assert
            Assert.Equal(EstadoFila.Nuevo, resultado.Filas[0].Estado);
            Assert.True(resultado.Filas[0].SeImporta);
        }

        [Fact]
        public void Clasificar_DniConPuntos_SeGuardaNormalizado()
        {
            // Act
            var resultado = Clasificar(Alumno("30.123.456", "Ana", "Perez", "ana@demo.test"));

            // Assert
            Assert.Equal("30123456", resultado.Filas[0].Dni);
        }

        [Fact]
        public void Clasificar_DniQueYaEstaEnLaComision_YaExiste()
        {
            // Arrange: la comisión lo tiene guardado con puntos y el archivo lo trae sin ellos
            var existentes = new[] { "30.123.456" };

            // Act
            var resultado = ClasificadorImportacion.Clasificar(
                new[] { Alumno("30123456", "Ana", "Perez", "ana@demo.test") }, existentes);

            // Assert
            Assert.Equal(EstadoFila.YaExiste, resultado.Filas[0].Estado);
            Assert.False(resultado.Filas[0].SeImporta);
        }

        [Fact]
        public void Clasificar_DniRepetidoEnElArchivo_SoloLaPrimeraEsNueva()
        {
            // Act
            var resultado = Clasificar(
                Alumno("30123456", "Ana", "Perez", "ana@demo.test"),
                Alumno("30123456", "Ana", "Perez", "ana@demo.test"));

            // Assert
            Assert.Equal(EstadoFila.Nuevo, resultado.Filas[0].Estado);
            Assert.Equal(EstadoFila.RepetidoEnArchivo, resultado.Filas[1].Estado);
        }

        [Fact]
        public void Clasificar_SinNombre_DatosIncompletos()
        {
            // Act
            var resultado = Clasificar(Alumno("30123456", "", "Perez", "ana@demo.test"));

            // Assert
            Assert.Equal(EstadoFila.DatosIncompletos, resultado.Filas[0].Estado);
        }

        [Fact]
        public void Clasificar_SinEmail_DatosIncompletos()
        {
            // Act: el email es obligatorio, aunque el archivo permita dejar la columna afuera
            var resultado = Clasificar(Alumno("30123456", "Ana", "Perez", ""));

            // Assert
            Assert.Equal(EstadoFila.DatosIncompletos, resultado.Filas[0].Estado);
        }

        [Fact]
        public void Clasificar_DniConLetras_DniInvalido()
        {
            // Act
            var resultado = Clasificar(Alumno("abc", "Ana", "Perez", "ana@demo.test"));

            // Assert
            Assert.Equal(EstadoFila.DniInvalido, resultado.Filas[0].Estado);
        }

        [Fact]
        public void Clasificar_DniMuyCorto_DniInvalido()
        {
            // Act
            var resultado = Clasificar(Alumno("12345", "Ana", "Perez", "ana@demo.test"));

            // Assert
            Assert.Equal(EstadoFila.DniInvalido, resultado.Filas[0].Estado);
        }

        [Fact]
        public void Clasificar_NombreDeMasDeCienCaracteres_DatoDemasiadoLargo()
        {
            // Arrange
            var nombreLargo = new string('a', LimitesAlumno.LargoNombre + 1);

            // Act
            var resultado = Clasificar(Alumno("30123456", nombreLargo, "Perez", "ana@demo.test"));

            // Assert
            Assert.Equal(EstadoFila.DatoDemasiadoLargo, resultado.Filas[0].Estado);
        }

        [Fact]
        public void Clasificar_EmailMalFormado_EmailInvalido()
        {
            // Act
            var resultado = Clasificar(Alumno("30123456", "Ana", "Perez", "ana@demo"));

            // Assert
            Assert.Equal(EstadoFila.EmailInvalido, resultado.Filas[0].Estado);
        }

        [Fact]
        public void Clasificar_DniEmailInvalidos_ReportaPrimeroElDni()
        {
            // Act: la fila tiene dos problemas y se informa uno solo, en el mismo orden que el alta manual
            var resultado = Clasificar(Alumno("abc", "Ana", "Perez", "ana@demo"));

            // Assert
            Assert.Equal(EstadoFila.DniInvalido, resultado.Filas[0].Estado);
        }

        [Fact]
        public void Clasificar_FilaRechazada_NoMarcaElDniComoVisto()
        {
            // Act: la primera fila no tiene nombre; la segunda, con el mismo DNI, está completa
            var resultado = Clasificar(
                Alumno("30123456", "", "Perez", "ana@demo.test"),
                Alumno("30123456", "Ana", "Perez", "ana@demo.test"));

            // Assert: la segunda es nueva y no "repetida", porque la primera no se iba a importar
            Assert.Equal(EstadoFila.DatosIncompletos, resultado.Filas[0].Estado);
            Assert.Equal(EstadoFila.Nuevo, resultado.Filas[1].Estado);
        }

        [Fact]
        public void Clasificar_ArchivoDeDemostracion_DaLosTotalesEsperados()
        {
            // Arrange: las 9 filas de alumnos_demo.csv, con Lucía ya cargada en la comisión
            var filas = new[]
            {
                Alumno("91000001", "Ana", "Ramirez", "ana.ramirez@demo.test"),
                Alumno("91000002", "Bruno", "Castro", "bruno.castro@demo.test"),
                Alumno("91000003", "Carla", "Mendez", "carla.mendez@demo.test"),
                Alumno("91000004", "Diego", "Sosa", "diego.sosa@demo.test"),
                Alumno("91000005", "Elena", "Vega", "elena.vega@demo.test"),
                Alumno("91000006", "Franco", "Luna", "franco.luna@demo.test"),
                Alumno("90000001", "Lucia", "Fernandez", "lucia.fernandez@demo.test"),
                Alumno("91000001", "Ana", "Ramirez", "ana.ramirez@demo.test"),
                Alumno("91000007", "", "Perez", "")
            };

            // Act
            var resultado = ClasificadorImportacion.Clasificar(filas, new[] { "90000001" });

            // Assert: 6 nuevos, 1 que ya existe, 1 repetido y 1 con errores
            Assert.Equal(6, resultado.Nuevos);
            Assert.Equal(1, resultado.YaExisten);
            Assert.Equal(1, resultado.Repetidos);
            Assert.Equal(1, resultado.ConErrores);
            Assert.Equal(6, resultado.Filas.Count(f => f.SeImporta));
        }

        // Helpers para armar los datos de prueba

        private static Estudiante Alumno(string dni, string nombre, string apellido, string email) =>
            new Estudiante { Dni = dni, Nombre = nombre, Apellido = apellido, Email = email };

        private static ResultadoClasificacion Clasificar(params Estudiante[] filas) =>
            ClasificadorImportacion.Clasificar(filas, Array.Empty<string>());
    }
}
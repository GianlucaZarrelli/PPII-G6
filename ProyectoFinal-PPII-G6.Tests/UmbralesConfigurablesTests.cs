using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas de que el riesgo se calcula con los umbrales que se indican, y de que al cambiarlos
    /// se recalcula de inmediato a toda la nómina.
    /// </summary>
    public class UmbralesConfigurablesTests
    {
        private readonly RiesgoService _riesgo = new();

        /// <summary>
        /// Arma la asistencia de un alumno: tantas presentes y tantas ausentes (sin justificar) como se indique.
        /// </summary>
        private static List<Asistencia> Asistencias(int presentes, int ausentes)
        {
            var lista = new List<Asistencia>();

            for (var i = 0; i < presentes + ausentes; i++)
            {
                lista.Add(new Asistencia(i + 1, 1, i + 1, i < presentes, false));
            }

            return lista;
        }

        /// <summary>
        /// Calcula el nivel de un alumno con 10 clases y la cantidad de presentes indicada, y todos los trabajos entregados.
        /// </summary>
        private NivelRiesgoEnum NivelConAsistencia(int presentes, UmbralesRiesgo? umbrales) =>
            _riesgo.CalcularRiesgo(Asistencias(presentes, 10 - presentes), 10, new List<Entrega>(), 0, umbrales).Nivel;

        [Fact]
        public void CalcularRiesgo_SinUmbrales_UsaLosValoresPorDefecto()
        {
            // Act: 80 % de asistencia está entre el 75 y el 85 por defecto
            var sinIndicar = NivelConAsistencia(8, null);
            var indicandoLosDeSiempre = NivelConAsistencia(8, UmbralesRiesgo.PorDefecto());

            // Assert
            Assert.Equal(NivelRiesgoEnum.Amarillo, sinIndicar);
            Assert.Equal(sinIndicar, indicandoLosDeSiempre);
        }

        [Fact]
        public void CalcularRiesgo_ConUmbralesMasEstrictos_ElMismoAlumnoPasaARojo()
        {
            // Act
            var nivel = NivelConAsistencia(8, new UmbralesRiesgo(85, 60, 95, 80));

            // Assert
            Assert.Equal(NivelRiesgoEnum.Rojo, nivel);
        }

        [Fact]
        public void CalcularRiesgo_ConUmbralesMasPermisivos_ElMismoAlumnoPasaAVerde()
        {
            // Act
            var nivel = NivelConAsistencia(8, new UmbralesRiesgo(50, 40, 70, 60));

            // Assert
            Assert.Equal(NivelRiesgoEnum.Verde, nivel);
        }

        [Fact]
        public void CalcularRiesgo_ExactamenteEnElUmbral_NoCaeEnEseNivel()
        {
            // Act: 70 % con el umbral de alto riesgo en 70 no es rojo: "por debajo" no incluye al propio umbral
            var nivel = NivelConAsistencia(7, new UmbralesRiesgo(70, 60, 90, 80));

            // Assert
            Assert.Equal(NivelRiesgoEnum.Amarillo, nivel);
        }

        [Fact]
        public void CalcularRiesgo_LasEntregasUsanSusPropiosUmbrales()
        {
            // Arrange: 2 de 4 trabajos entregados es 50 %
            var entregas = new List<Entrega> { new(1, 1, 1, true), new(2, 1, 2, true), new(3, 1, 3, false), new(4, 1, 4, false) };

            // Act
            var conLosDeSiempre = _riesgo.CalcularRiesgo(new List<Asistencia>(), 0, entregas, 4).Nivel;
            var conOtros = _riesgo.CalcularRiesgo(new List<Asistencia>(), 0, entregas, 4, new UmbralesRiesgo(75, 40, 85, 60)).Nivel;

            // Assert
            Assert.Equal(NivelRiesgoEnum.Rojo, conLosDeSiempre);
            Assert.Equal(NivelRiesgoEnum.Amarillo, conOtros);
        }

        [Fact]
        public void Evaluar_ConOtrosUmbrales_CambiaElNivelDelResumen()
        {
            // Arrange
            var servicio = new SeguimientoService(_riesgo);
            var alumno = new Estudiante(1, "90000001", "Lucía", "Fernández", "", 1) { FechaAlta = new DateTime(2026, 8, 1) };
            var clases = Enumerable.Range(1, 10).Select(i => new Clase(i, new DateTime(2026, 9, 1).AddDays(i), null, 1)).ToList();
            var hoy = new DateTime(2026, 10, 26);

            // Act
            var conLosDeSiempre = servicio.Evaluar(alumno, clases, new List<TrabajoPractico>(), Asistencias(8, 2), new List<Entrega>(), hoy);
            var conOtros = servicio.Evaluar(alumno, clases, new List<TrabajoPractico>(), Asistencias(8, 2), new List<Entrega>(), hoy,
                null, new UmbralesRiesgo(85, 60, 95, 80));

            // Assert
            Assert.Equal(NivelSeguimiento.Amarillo, conLosDeSiempre.Nivel);
            Assert.Equal(NivelSeguimiento.Rojo, conOtros.Nivel);
            Assert.Equal(conLosDeSiempre.PorcentajeAsistencia, conOtros.PorcentajeAsistencia);
        }

        [Fact]
        public async Task ConsultaSeguimiento_AlGuardarOtrosUmbrales_RecalculaDeInmediato()
        {
            // Arrange: un alumno con 80 % de asistencia en una comisión con 5 clases ya dadas
            var datos = new DatosEnMemoria();
            datos.Comisiones.Add(new Comision(1, "A"));
            var alumno = datos.AgregarAlumno(1, "Lucía", "Fernández", 1);
            for (var i = 1; i <= 5; i++)
            {
                datos.Clases.Add(new Clase(i, new DateTime(2026, 9, 1).AddDays(i), null, 1));
                datos.Asistencias.Add(new Asistencia(i, alumno.Id, i, i <= 4, false));
            }
            var consulta = new ConsultaSeguimiento(datos, new SeguimientoService(_riesgo));

            // Act
            var antes = (await consulta.ObtenerComisionAsync(1)).Single().Nivel;
            datos.Umbrales = new UmbralesRiesgo(85, 60, 95, 80);
            var despues = (await consulta.ObtenerComisionAsync(1)).Single().Nivel;
            var enLaFicha = (await consulta.ObtenerFichaAsync(alumno)).Resumen.Nivel;

            // Assert
            Assert.Equal(NivelSeguimiento.Amarillo, antes);
            Assert.Equal(NivelSeguimiento.Rojo, despues);
            Assert.Equal(NivelSeguimiento.Rojo, enLaFicha);
        }
    }
}
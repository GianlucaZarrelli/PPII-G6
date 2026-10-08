using System.Collections.Generic;
using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas unitarias del cálculo del nivel de riesgo académico.
    /// </summary>
    public class RiesgoServiceTests
    {
        private readonly RiesgoService _service;

        public RiesgoServiceTests()
        {
            _service = new RiesgoService();
        }

        [Fact]
        public void CalcularRiesgo_EstudianteCumplidor_RetornaVerde()
        {
            // Arrange: 10/10 asistencias (100%) y 5/5 entregas (100%)
            var asistencias = CrearAsistencias(presentes: 10);
            var entregas = CrearEntregas(entregados: 5);

            // Act
            var resultado = _service.CalcularRiesgo(asistencias, 10, entregas, 5);

            // Assert
            Assert.Equal(NivelRiesgoEnum.Verde, resultado.Nivel);
            Assert.Equal(100, resultado.PorcentajeAsistencia);
            Assert.Equal(100, resultado.PorcentajeEntregas);
        }

        [Fact]
        public void CalcularRiesgo_AsistenciaOEntregasEnRangoMedio_RetornaAmarillo()
        {
            // Arrange: 8/10 asistencias (80% - rango 75% a 84%) y 5/5 entregas (100%)
            var asistencias = CrearAsistencias(presentes: 8);
            var entregas = CrearEntregas(entregados: 5);

            // Act
            var resultado = _service.CalcularRiesgo(asistencias, 10, entregas, 5);

            // Assert
            Assert.Equal(NivelRiesgoEnum.Amarillo, resultado.Nivel);
        }

        [Fact]
        public void CalcularRiesgo_AsistenciaBajaOEntregasBajas_RetornaRojo()
        {
            // Arrange: 7/10 asistencias (70% - menor al 75%)
            var asistencias = CrearAsistencias(presentes: 7);
            var entregas = CrearEntregas(entregados: 5);

            // Act
            var resultado = _service.CalcularRiesgo(asistencias, 10, entregas, 5);

            // Assert
            Assert.Equal(NivelRiesgoEnum.Rojo, resultado.Nivel);
        }

        [Fact]
        public void CalcularRiesgo_SinClasesNiTpsRegistrados_RetornaVerdeY100PorCiento()
        {
            // Arrange & Act (Borde: inicio de cuatrimestre sin datos)
            var resultado = _service.CalcularRiesgo(new List<Asistencia>(), 0, new List<Entrega>(), 0);

            // Assert
            Assert.Equal(NivelRiesgoEnum.Verde, resultado.Nivel);
            Assert.Equal(100, resultado.PorcentajeAsistencia);
            Assert.Equal(100, resultado.PorcentajeEntregas);
        }

        // Helpers auxiliares para generar datos de prueba
        private List<Asistencia> CrearAsistencias(int presentes)
        {
            var lista = new List<Asistencia>();
            for (int i = 0; i < presentes; i++)
                lista.Add(new Asistencia { Presente = true });
            return lista;
        }

        private List<Entrega> CrearEntregas(int entregados)
        {
            var lista = new List<Entrega>();
            for (int i = 0; i < entregados; i++)
                lista.Add(new Entrega { Entregado = true });
            return lista;
        }
    }
}
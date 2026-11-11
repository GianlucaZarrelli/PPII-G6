using Microsoft.EntityFrameworkCore;
using ProyectoFinal_PPII_G6.Infraestructura;
using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas de integración: ponen a prueba el adaptador SqlServerDataService contra una base real (SQLite en memoria),
    /// y no contra el doble de las pruebas unitarias. Cubren lo que el doble no simula: la base rechaza duplicados,
    /// asigna los Id y devuelve las listas en el orden que se le pide.
    /// </summary>
    [Trait("Categoria", "Integracion")]
    public class SqlServerDataServiceIntegracionTests : IDisposable
    {
        private readonly BaseEnMemoria _base = new();
        private readonly SqlServerDataService _datos;

        public SqlServerDataServiceIntegracionTests()
        {
            _datos = new SqlServerDataService(_base);
        }

        public void Dispose() => _base.Dispose();

        private async Task<int> CrearComisionAsync(string nombre) => (await _datos.GetOrCreateComisionAsync(nombre)).Id;

        private Task<int> GuardarEstudianteAsync(int comisionId, string dni, string nombre = "Juan", string apellido = "Pérez") =>
            _datos.SaveEstudianteAsync(new Estudiante(0, dni, nombre, apellido, "alumno@mail.com", comisionId) { FechaAlta = new DateTime(2026, 9, 1) });

        [Fact]
        public async Task Given_UnEstudianteGuardado_When_LoLeoPorComision_Then_VuelveConSusDatosYUnIdAsignadoPorLaBase()
        {
            // Dado un estudiante guardado en una comisión
            var comisionId = await CrearComisionAsync("1°A");
            var id = await GuardarEstudianteAsync(comisionId, "12345678");

            // Cuando se lee la nómina de la comisión
            var leido = Assert.Single(await _datos.GetEstudiantesByComisionIdAsync(comisionId));

            // Entonces vuelve con sus datos y con el Id que le asignó la base
            Assert.True(id > 0);
            Assert.Equal(id, leido.Id);
            Assert.Equal("12345678", leido.Dni);
            Assert.Equal("Pérez", leido.Apellido);
            Assert.Equal(new DateTime(2026, 9, 1), leido.FechaAlta);
        }

        // H1 · CA3 · Identificador repetido, del lado de la base: el doble acepta dos estudiantes con el mismo DNI; la base no
        [Fact]
        public async Task Given_DniYaCargadoEnLaBase_When_GuardarOtroEnLaMismaComision_Then_LaBaseLoRechazaYLaNominaNoCambia()
        {
            // Dado que la comisión ya tiene un estudiante con el DNI 12345678
            var comisionId = await CrearComisionAsync("1°A");
            await GuardarEstudianteAsync(comisionId, "12345678");

            // Cuando se intenta guardar otro con el mismo DNI en la misma comisión
            await Assert.ThrowsAsync<DbUpdateException>(() => GuardarEstudianteAsync(comisionId, "12345678", "Ana", "Gómez"));

            // Entonces la base lo rechaza y la nómina sigue con un solo estudiante
            Assert.Single(await _datos.GetEstudiantesByComisionIdAsync(comisionId));
        }

        [Fact]
        public async Task Given_ElMismoDniEnOtraComision_When_Guardar_Then_LaBaseLoAcepta()
        {
            // Dado un estudiante con el DNI 12345678 en 1°A
            var primera = await CrearComisionAsync("1°A");
            var segunda = await CrearComisionAsync("1°B");
            await GuardarEstudianteAsync(primera, "12345678");

            // Cuando se guarda el mismo DNI en 1°B
            await GuardarEstudianteAsync(segunda, "12345678");

            // Entonces se acepta: el DNI no se repite dentro de una comisión, pero sí puede estar en dos
            Assert.Single(await _datos.GetEstudiantesByComisionIdAsync(primera));
            Assert.Single(await _datos.GetEstudiantesByComisionIdAsync(segunda));
        }

        [Fact]
        public async Task Given_UnLoteConUnDniRepetido_When_GuardarCargaMasiva_Then_NoSeGuardaNingunoDelLote()
        {
            // Dado un lote de tres estudiantes donde el primero y el tercero tienen el mismo DNI
            var comisionId = await CrearComisionAsync("1°A");
            var lote = new List<Estudiante>
            {
                new(0, "11111111", "Ana", "Gómez", "ana@mail.com", comisionId),
                new(0, "22222222", "Luis", "Paz", "luis@mail.com", comisionId),
                new(0, "11111111", "Eva", "Ruiz", "eva@mail.com", comisionId)
            };

            // Cuando se guarda la carga masiva
            await Assert.ThrowsAsync<DbUpdateException>(() => _datos.SaveEstudiantesMassiveAsync(lote));

            // Entonces la base la rechaza y no queda guardado ninguno: se guarda todo o nada
            Assert.Empty(await _datos.GetEstudiantesByComisionIdAsync(comisionId));
        }

        [Fact]
        public async Task Given_UnContactoGuardado_When_LoLeoDeLaBase_Then_ElMedioVuelveIgualYQuedoGuardadoComoTexto()
        {
            // Dado un contacto guardado con el medio "Mensaje"
            var comisionId = await CrearComisionAsync("1°A");
            var estudianteId = await GuardarEstudianteAsync(comisionId, "12345678");
            var fechaHora = new DateTime(2026, 11, 10, 14, 30, 0);
            await _datos.SaveContactoAsync(new Contacto(0, estudianteId, fechaHora, MedioContacto.Mensaje, "No atendió."));

            // Cuando se lo lee de la base
            var leido = Assert.Single(await _datos.GetContactosPorEstudianteAsync(estudianteId));

            // Entonces vuelve igual, con su fecha y hora
            Assert.Equal(MedioContacto.Mensaje, leido.Medio);
            Assert.Equal(fechaHora, leido.FechaHora);
            Assert.Equal("No atendió.", leido.Resultado);

            // Y en la tabla el medio está escrito como texto, que es lo que espera la restricción del script de la base
            using var contexto = _base.CreateDbContext();
            Assert.Equal("Mensaje", await contexto.Database.SqlQueryRaw<string>("SELECT Medio AS Value FROM Contactos").SingleAsync());
        }

        [Fact]
        public async Task Given_ContactosGuardadosEnDesorden_When_ListoLosDeUnEstudiante_Then_VienenDelMasRecienteAlMasAntiguo()
        {
            // Dado tres contactos guardados fuera de orden
            var comisionId = await CrearComisionAsync("1°A");
            var estudianteId = await GuardarEstudianteAsync(comisionId, "12345678");
            foreach (var dia in new[] { 10, 5, 12 })
            {
                await _datos.SaveContactoAsync(new Contacto(0, estudianteId, new DateTime(2026, 11, dia, 9, 0, 0), MedioContacto.Llamada, "Atendió."));
            }

            // Cuando se listan los contactos del estudiante
            var contactos = await _datos.GetContactosPorEstudianteAsync(estudianteId);

            // Entonces vienen del más reciente al más antiguo
            Assert.Equal(new[] { 12, 10, 5 }, contactos.Select(c => c.FechaHora.Day));
        }

        [Fact]
        public async Task Given_ClaseEnUnaFecha_When_GuardarOtraEnLaMismaFechaDeLaComision_Then_LaBaseLaRechaza()
        {
            // Dado una clase cargada el 7/9 en 1°A
            var comisionId = await CrearComisionAsync("1°A");
            var fecha = new DateTime(2026, 9, 7);
            await _datos.SaveClaseAsync(new Clase(0, fecha, "Introducción", comisionId));

            // Cuando se intenta cargar otra clase el mismo día en la misma comisión
            await Assert.ThrowsAsync<DbUpdateException>(() => _datos.SaveClaseAsync(new Clase(0, fecha, "Otra", comisionId)));

            // Entonces la base la rechaza y la comisión sigue con una sola clase
            Assert.Single(await _datos.GetClasesByComisionIdAsync(comisionId));
        }

        [Fact]
        public async Task Given_ClasesGuardadasEnDesorden_When_ListoLasDeLaComision_Then_VienenDeLaMasRecienteALaMasAntigua()
        {
            // Dado tres clases guardadas fuera de orden
            var comisionId = await CrearComisionAsync("1°A");
            foreach (var dia in new[] { 14, 7, 21 })
            {
                await _datos.SaveClaseAsync(new Clase(0, new DateTime(2026, 9, dia), null, comisionId));
            }

            // Cuando se listan las clases de la comisión
            var clases = await _datos.GetClasesByComisionIdAsync(comisionId);

            // Entonces vienen de la más reciente a la más antigua
            Assert.Equal(new[] { 21, 14, 7 }, clases.Select(c => c.Fecha.Day));
        }

        [Fact]
        public async Task Given_EntregasDeDosEstudiantes_When_PidoLasDeUno_Then_VuelvenSoloLasSuyas()
        {
            // Dado un trabajo y las entregas de dos estudiantes
            var comisionId = await CrearComisionAsync("1°A");
            var ana = await GuardarEstudianteAsync(comisionId, "11111111", "Ana", "Gómez");
            var luis = await GuardarEstudianteAsync(comisionId, "22222222", "Luis", "Paz");
            var trabajo = await _datos.SaveTrabajoPracticoAsync(new TrabajoPractico(0, comisionId, "TP1", new DateTime(2026, 9, 30)));
            await _datos.SaveEntregasAsync(new List<Entrega> { new(0, ana, trabajo.Id, true), new(0, luis, trabajo.Id, false) });

            // Cuando se piden las entregas de Ana
            var entregasDeAna = await _datos.GetEntregasPorEstudianteAsync(ana);

            // Entonces vuelve solo la suya
            var entrega = Assert.Single(entregasDeAna);
            Assert.True(entrega.Entregado);
            Assert.Equal(ana, entrega.EstudianteId);
        }

        [Fact]
        public async Task Given_ComisionExistente_When_PidoCrearlaOtraVezConElMismoNombre_Then_DevuelveLaMismaYNoDuplica()
        {
            // Dado una comisión creada
            var primera = await _datos.GetOrCreateComisionAsync("1°A");

            // Cuando se pide crear una con el mismo nombre (con espacios de más)
            var segunda = await _datos.GetOrCreateComisionAsync("  1°A  ");

            // Entonces devuelve la misma y la base sigue teniendo una sola
            Assert.Equal(primera.Id, segunda.Id);
            Assert.Single(await _datos.GetComisionesAsync());
        }

        [Fact]
        public async Task Given_SinUmbralesGuardados_When_LosPido_Then_DevuelveLosValoresPorDefecto()
        {
            // Dado una base sin umbrales guardados, cuando se los pide
            var umbrales = await _datos.GetUmbralesAsync();

            // Entonces devuelve los valores por defecto
            Assert.Equal(75, umbrales.AsistenciaRojo);
            Assert.Equal(60, umbrales.EntregasRojo);
            Assert.Equal(85, umbrales.AsistenciaAmarillo);
            Assert.Equal(80, umbrales.EntregasAmarillo);
        }

        // H7 · Los umbrales son una sola configuración: guardar de nuevo reemplaza a la anterior
        [Fact]
        public async Task Given_UmbralesGuardados_When_LosGuardoDeNuevo_Then_QuedaUnaSolaFilaConLosUltimosValores()
        {
            // Dado unos umbrales guardados
            await _datos.SaveUmbralesAsync(new UmbralesRiesgo(50, 40, 70, 60));

            // Cuando se guardan otros
            await _datos.SaveUmbralesAsync(new UmbralesRiesgo(55, 45, 75, 65));

            // Entonces se leen los últimos y en la tabla hay una sola fila
            var guardados = await _datos.GetUmbralesAsync();
            Assert.Equal(55, guardados.AsistenciaRojo);
            Assert.Equal(45, guardados.EntregasRojo);
            Assert.Equal(75, guardados.AsistenciaAmarillo);
            Assert.Equal(65, guardados.EntregasAmarillo);

            using var contexto = _base.CreateDbContext();
            Assert.Equal(1, await contexto.Umbrales.CountAsync());
        }
    }
}
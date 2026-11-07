using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;
using ProyectoFinal_PPII_G6.ViewModels;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Pruebas de aceptación: cada una traduce criterios de aceptación de una historia a un escenario (Given, When, Then),
    /// y el nombre de la prueba es el criterio. Recorren el caso de uso completo, desde lo que hace el tutor hasta lo
    /// que ve, entrando por el ViewModel de cada pantalla (que es lo que orquesta cada caso de uso, sin XAML) y pasando
    /// por las reglas y la consulta de seguimiento, con la base, los avisos, la navegación y el tema falsos.
    /// </summary>
    [Trait("Categoria", "Aceptacion")]
    public class PruebasDeAceptacionTests
    {
        private const int Lucia = 1;
        private const int Mateo = 2;
        private const int Joaquin = 3;

        private readonly DatosEnMemoria _datos = new();
        private readonly DialogoFalso _dialogo = new();
        private readonly NavegacionFalsa _navegacion = new();
        private readonly TemaFalso _tema = new();
        private readonly ConsultaSeguimiento _consulta;

        /// <summary>
        /// Dado el estado de partida de todos los escenarios: la comisión A con 6 clases y 3 trabajos ya vencidos,
        /// y tres alumnos: Lucía al día (6 de 6 clases, 3 de 3 trabajos), Mateo en riesgo moderado (5 de 6 clases,
        /// 2 de 3 trabajos) y Joaquín en alto riesgo (2 de 6 clases, 1 de 3 trabajos).
        /// </summary>
        public PruebasDeAceptacionTests()
        {
            _consulta = new ConsultaSeguimiento(_datos, new SeguimientoService(new RiesgoService()));

            _datos.Comisiones.Add(new Comision(1, "A"));
            _datos.AgregarAlumno(Lucia, "Lucía", "Fernández", 1);
            _datos.AgregarAlumno(Mateo, "Mateo", "Gómez", 1);
            _datos.AgregarAlumno(Joaquin, "Joaquín", "Pérez", 1);

            CargarClasesYTrabajos();
            CargarAsistencias(Lucia, presentes: 6);
            CargarAsistencias(Mateo, presentes: 5);
            CargarAsistencias(Joaquin, presentes: 2);
            CargarEntregas(Lucia, entregados: 3);
            CargarEntregas(Mateo, entregados: 2);
            CargarEntregas(Joaquin, entregados: 1);
        }

        private void CargarClasesYTrabajos()
        {
            for (var i = 1; i <= 6; i++)
            {
                _datos.Clases.Add(new Clase(i, new DateTime(2026, 9, 1).AddDays(i * 7), null, 1));
            }

            for (var i = 1; i <= 3; i++)
            {
                _datos.Trabajos.Add(new TrabajoPractico(i, 1, $"TP{i}", new DateTime(2026, 9, 10).AddDays(i * 7)));
            }
        }

        private void CargarAsistencias(int alumno, int presentes)
        {
            for (var clase = 1; clase <= 6; clase++)
            {
                _datos.Asistencias.Add(new Asistencia(alumno * 100 + clase, alumno, clase, clase <= presentes, false));
            }
        }

        private void CargarEntregas(int alumno, int entregados)
        {
            for (var tp = 1; tp <= 3; tp++)
            {
                _datos.Entregas.Add(new Entrega(alumno * 100 + tp, alumno, tp, tp <= entregados));
            }
        }

        private PanelRiesgoViewModel CrearPanel() =>
            new(_datos, _consulta, new SeleccionAlumnoService(), _navegacion);

        private async Task<PanelRiesgoViewModel> AbrirPanelAsync()
        {
            var panel = CrearPanel();
            await panel.CargarAsync();
            return panel;
        }

        // H8 · Criterios 1 a 4: lista de la comisión ordenada por riesgo, con indicador y dato que lo justifica
        [Fact]
        public async Task Given_ComisionConUnAlumnoAlDiaUnoModeradoYUnoEnRiesgoAlto_When_ElTutorAbreElPanel_Then_VeAPrimeroAlDeMayorRiesgoConLosNumerosQueLoJustifican()
        {
            // Dado la comisión A con un alumno al día, uno en riesgo moderado y uno en alto riesgo
            // Cuando el tutor abre el panel de riesgo de la comisión
            var panel = await AbrirPanelAsync();

            // Entonces ve primero al de alto riesgo, después al moderado y al final al que está al día
            Assert.Equal(new[] { "Joaquín Pérez", "Mateo Gómez", "Lucía Fernández" }, panel.Alumnos.Select(a => a.NombreCompleto));
            Assert.Equal(new[] { "ALTO RIESGO", "RIESGO MODERADO", "AL DÍA" }, panel.Alumnos.Select(a => a.Nivel));

            // Y cada fila muestra los números que justifican su nivel
            Assert.Equal("Asistencia 2/6 (33 %)  |  Entregas 1/3 (33 %)", panel.Alumnos[0].Detalle);
            Assert.Equal("Asistencia 5/6 (83 %)  |  Entregas 2/3 (67 %)", panel.Alumnos[1].Detalle);
        }

        // H10 · Criterio 3 (se guarda vinculado al alumno) y H11 · Criterios 1 y 2 (indicador y último contacto en el panel)
        [Fact]
        public async Task Given_AlumnoEnRiesgoAltoSinContactar_When_ElTutorRegistraUnaLlamada_Then_SigueEnRiesgoAltoPeroFiguraComoContactado()
        {
            // Dado un alumno en alto riesgo al que nadie contactó
            var antes = await AbrirPanelAsync();
            var joaquinAntes = antes.Alumnos.Single(a => a.EstudianteId == Joaquin);
            Assert.Equal("Sin contactar", joaquinAntes.ContactoTexto);

            // Cuando el tutor registra una llamada a ese alumno y vuelve al panel
            await RegistrarUnaLlamadaAsync(Joaquin);
            var despues = await AbrirPanelAsync();
            var joaquinDespues = despues.Alumnos.Single(a => a.EstudianteId == Joaquin);

            // Entonces el alumno sigue en alto riesgo, y su fila muestra que fue contactado
            Assert.Equal("ALTO RIESGO", joaquinDespues.Nivel);
            Assert.StartsWith("Último contacto: ", joaquinDespues.ContactoTexto);
            Assert.EndsWith("(1 contacto)", joaquinDespues.ContactoTexto);
            Assert.Contains("Éxito: Contacto registrado.", _dialogo.Avisos);
        }

        private async Task RegistrarUnaLlamadaAsync(int alumnoId)
        {
            var contactos = new ContactosViewModel(_datos, _dialogo);
            await contactos.CargarComisionesAsync();

            contactos.EstudianteSeleccionado = contactos.ListaEstudiantes.Single(e => e.Id == alumnoId);
            contactos.Fecha = DateTime.Today;
            contactos.Hora = TimeSpan.Zero;
            contactos.MedioSeleccionado = contactos.Medios.First(m => m.Valor == MedioContacto.Llamada);
            contactos.Resultado = "Atendió y se comprometió a entregar.";

            await contactos.GuardarAsync();
        }

        // H7 · Criterio 3: al guardar los nuevos umbrales se recalcula el riesgo de todos los estudiantes de inmediato
        [Fact]
        public async Task Given_AlumnoEnRiesgoModerado_When_ElTutorEndureceLosUmbralesYGuarda_Then_ElPanelYElInicioMuestranLosNivelesNuevos()
        {
            // Dado un alumno en riesgo moderado (Mateo, 83 % de asistencia) y un solo alumno en alto riesgo en Inicio
            var inicio = new HomeViewModel(_consulta, _navegacion, _tema);
            await inicio.CargarMetricasAsync();
            Assert.Equal(1, inicio.EstudiantesEnRiesgo);
            var panelAntes = await AbrirPanelAsync();
            Assert.Equal("RIESGO MODERADO", panelAntes.Alumnos.Single(a => a.EstudianteId == Mateo).Nivel);

            // Cuando el tutor sube los umbrales de asistencia en la pantalla Umbrales y los guarda
            var umbrales = new UmbralesViewModel(_datos, _consulta, _dialogo);
            await umbrales.CargarAsync();
            umbrales.AsistenciaRojo = "85";
            umbrales.AsistenciaAmarillo = "95";
            await umbrales.GuardarAsync();

            // Entonces la pantalla avisa cuántos alumnos se reevaluaron
            Assert.Contains("Éxito: Umbrales guardados. Se recalculó el riesgo de 3 alumnos.", _dialogo.Avisos);

            // Y, sin reiniciar, el panel muestra a Mateo en alto riesgo e Inicio cuenta un alumno más en rojo
            var panelDespues = await AbrirPanelAsync();
            await inicio.CargarMetricasAsync();
            Assert.Equal("ALTO RIESGO", panelDespues.Alumnos.Single(a => a.EstudianteId == Mateo).Nivel);
            Assert.Equal(2, inicio.EstudiantesEnRiesgo);
        }

        // H2 · Criterios 1 y 5 (cargar fechas de clases y consultar el calendario) y regla del ADR 7: las clases futuras no cuentan
        [Fact]
        public async Task Given_ElPanelConLosNivelesDeHoy_When_ElTutorCargaClasesFuturasEnElCalendario_Then_NingunAlumnoCambiaDeNivel()
        {
            // Dado el panel con los niveles de hoy
            var ultimoDia = new DateTime(DateTime.Today.Year, 12, 31);
            if (DateTime.Today >= ultimoDia.AddDays(-2)) return; // hace falta que existan fechas futuras en el año en curso
            var antes = (await AbrirPanelAsync()).Alumnos.ToDictionary(a => a.EstudianteId, a => a.Nivel);

            // Cuando el tutor carga en el calendario tres clases de fechas futuras
            var calendario = new CalendarioViewModel(_datos, _dialogo);
            await calendario.CargarComisionesAsync();
            foreach (var dias in new[] { 0, 1, 2 })
            {
                calendario.FechaClase = ultimoDia.AddDays(-dias);
                await calendario.AgregarClaseAsync();
            }

            // Entonces el calendario muestra las 9 clases y los 3 trabajos, ordenados por fecha
            Assert.Equal(12, calendario.Filas.Count);
            Assert.Equal(calendario.Filas.Select(f => f.Fecha).OrderBy(f => DateTime.ParseExact(f, "dd/MM/yyyy", null)), calendario.Filas.Select(f => f.Fecha));

            // Y ningún alumno cambia de nivel: las clases futuras todavía no cuentan
            var despues = (await AbrirPanelAsync()).Alumnos.ToDictionary(a => a.EstudianteId, a => a.Nivel);
            Assert.Equal(antes, despues);
        }
    }
}
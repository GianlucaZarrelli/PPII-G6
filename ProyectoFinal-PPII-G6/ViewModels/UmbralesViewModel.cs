using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Presentacion;
using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.ViewModels
{
    /// <summary>
    /// ViewModel de la pantalla de umbrales de riesgo: el tutor ve los cuatro porcentajes con los que se decide el nivel
    /// de cada alumno, los cambia, y al guardar se recalcula de inmediato el riesgo de toda la nómina.
    /// </summary>
    public class UmbralesViewModel : BindableObject
    {
        private readonly IDataService _dataService;
        private readonly ConsultaSeguimiento _consulta;
        private readonly IDialogoService _dialogo;

        private string _asistenciaRojo = string.Empty;

        /// <summary>
        /// Porcentaje de asistencia por debajo del cual el riesgo es alto, tal como se escribe en la pantalla.
        /// </summary>
        public string AsistenciaRojo
        {
            get => _asistenciaRojo;
            set { _asistenciaRojo = value ?? string.Empty; OnPropertyChanged(); }
        }

        private string _asistenciaAmarillo = string.Empty;

        /// <summary>
        /// Porcentaje de asistencia por debajo del cual el riesgo es moderado, tal como se escribe en la pantalla.
        /// </summary>
        public string AsistenciaAmarillo
        {
            get => _asistenciaAmarillo;
            set { _asistenciaAmarillo = value ?? string.Empty; OnPropertyChanged(); }
        }

        private string _entregasRojo = string.Empty;

        /// <summary>
        /// Porcentaje de entregas por debajo del cual el riesgo es alto, tal como se escribe en la pantalla.
        /// </summary>
        public string EntregasRojo
        {
            get => _entregasRojo;
            set { _entregasRojo = value ?? string.Empty; OnPropertyChanged(); }
        }

        private string _entregasAmarillo = string.Empty;

        /// <summary>
        /// Porcentaje de entregas por debajo del cual el riesgo es moderado, tal como se escribe en la pantalla.
        /// </summary>
        public string EntregasAmarillo
        {
            get => _entregasAmarillo;
            set { _entregasAmarillo = value ?? string.Empty; OnPropertyChanged(); }
        }

        private string _resumenNiveles = string.Empty;

        /// <summary>
        /// Cuántos alumnos hay en cada nivel con los umbrales guardados, por ejemplo "ALTO RIESGO: 4  |  AL DÍA: 5".
        /// </summary>
        public string ResumenNiveles
        {
            get => _resumenNiveles;
            set { _resumenNiveles = value; OnPropertyChanged(); }
        }

        private string _estado = string.Empty;

        /// <summary>
        /// Mensaje de la pantalla cuando algo no se pudo cargar.
        /// </summary>
        public string Estado
        {
            get => _estado;
            set { _estado = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Comando para guardar los umbrales escritos.
        /// </summary>
        public Command GuardarCommand { get; }

        /// <summary>
        /// Comando para volver a los umbrales por defecto.
        /// </summary>
        public Command RestablecerCommand { get; }

        /// <summary>
        /// Crea el ViewModel con el acceso a datos, la consulta que evalúa a los alumnos y el servicio de avisos.
        /// </summary>
        public UmbralesViewModel(IDataService dataService, ConsultaSeguimiento consulta, IDialogoService dialogo)
        {
            _dataService = dataService;
            _consulta = consulta;
            _dialogo = dialogo;

            GuardarCommand = new Command(async () => await GuardarAsync());
            RestablecerCommand = new Command(async () => await RestablecerAsync());
        }

        /// <summary>
        /// Muestra los umbrales guardados y cuántos alumnos hay en cada nivel con ellos.
        /// </summary>
        public async Task CargarAsync()
        {
            try
            {
                MostrarUmbrales(await _dataService.GetUmbralesAsync());
                await MostrarResumenAsync();
                Estado = string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                Estado = "No se pudieron cargar los umbrales.";
            }
        }

        /// <summary>
        /// Valida los cuatro valores escritos y, si están bien, los guarda y recalcula el riesgo de todos los alumnos.
        /// Es pública para poder probarla sin la pantalla.
        /// </summary>
        public async Task GuardarAsync()
        {
            var umbrales = LeerUmbrales();
            var error = umbrales == null
                ? "Los cuatro umbrales tienen que ser números enteros."
                : ValidacionUmbrales.ObtenerError(umbrales);

            if (error != null)
                await _dialogo.AvisarAsync("Atención", error);
            else
                await GuardarUmbralesAsync(umbrales!);
        }

        /// <summary>
        /// Vuelve a poner los umbrales por defecto y los guarda, con el mismo recálculo que cualquier otro guardado.
        /// Es pública para poder probarla sin la pantalla.
        /// </summary>
        public async Task RestablecerAsync()
        {
            MostrarUmbrales(UmbralesRiesgo.PorDefecto());
            await GuardarAsync();
        }

        /// <summary>
        /// Guarda los umbrales, recalcula el resumen y avisa cuántos alumnos se reevaluaron.
        /// Si la base falla, avisa y conserva lo escrito.
        /// </summary>
        private async Task GuardarUmbralesAsync(UmbralesRiesgo umbrales)
        {
            try
            {
                await _dataService.SaveUmbralesAsync(umbrales);
                var alumnos = await MostrarResumenAsync();

                await _dialogo.AvisarAsync("Éxito",
                    $"Umbrales guardados. Se recalculó el riesgo de {alumnos} {(alumnos == 1 ? "alumno" : "alumnos")}.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                await _dialogo.AvisarAsync("Error", "No se pudieron guardar los umbrales. Revisá la conexión con la base de datos.");
            }
        }

        /// <summary>
        /// Evalúa a todos los alumnos con los umbrales guardados y muestra cuántos hay en cada nivel.
        /// Devuelve la cantidad de alumnos evaluados.
        /// </summary>
        private async Task<int> MostrarResumenAsync()
        {
            var resumenes = await _consulta.ObtenerTodasAsync();
            var niveles = new[] { NivelSeguimiento.Rojo, NivelSeguimiento.Amarillo, NivelSeguimiento.Verde, NivelSeguimiento.SinDatos };

            ResumenNiveles = string.Join("  |  ",
                niveles.Select(n => $"{PresentacionNivel.Texto(n)}: {resumenes.Count(r => r.Nivel == n)}"));

            return resumenes.Count;
        }

        /// <summary>
        /// Escribe los cuatro valores en los campos de la pantalla.
        /// </summary>
        private void MostrarUmbrales(UmbralesRiesgo umbrales)
        {
            AsistenciaRojo = umbrales.AsistenciaRojo.ToString(CultureInfo.InvariantCulture);
            AsistenciaAmarillo = umbrales.AsistenciaAmarillo.ToString(CultureInfo.InvariantCulture);
            EntregasRojo = umbrales.EntregasRojo.ToString(CultureInfo.InvariantCulture);
            EntregasAmarillo = umbrales.EntregasAmarillo.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Lee los cuatro campos como números enteros, o devuelve null si alguno no lo es.
        /// </summary>
        private UmbralesRiesgo? LeerUmbrales() =>
            TryEntero(AsistenciaRojo, out var asistenciaRojo)
            && TryEntero(EntregasRojo, out var entregasRojo)
            && TryEntero(AsistenciaAmarillo, out var asistenciaAmarillo)
            && TryEntero(EntregasAmarillo, out var entregasAmarillo)
                ? new UmbralesRiesgo(asistenciaRojo, entregasRojo, asistenciaAmarillo, entregasAmarillo)
                : null;

        /// <summary>
        /// Convierte un texto en un número entero, sin tener en cuenta los espacios de los extremos.
        /// </summary>
        private static bool TryEntero(string? texto, out int valor) =>
            int.TryParse((texto ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out valor);
    }
}
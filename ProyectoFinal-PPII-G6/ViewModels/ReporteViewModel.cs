using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Presentacion;
using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.ViewModels
{
    /// <summary>
    /// Una barra del gráfico del reporte: cuántos estudiantes hay en un nivel.
    /// </summary>
    public class FilaGrafico
    {
        /// <summary>
        /// Nombre del nivel, por ejemplo "ALTO RIESGO".
        /// </summary>
        public string Nivel { get; init; } = string.Empty;

        /// <summary>
        /// Cantidad de estudiantes en ese nivel.
        /// </summary>
        public int Cantidad { get; init; }

        /// <summary>
        /// Cantidad y porcentaje, por ejemplo "7  (23 %)".
        /// </summary>
        public string Resumen { get; init; } = string.Empty;

        /// <summary>
        /// Ancho de la barra, proporcional a la cantidad.
        /// </summary>
        public double Ancho { get; init; }

        /// <summary>
        /// Color del nivel, en hexadecimal.
        /// </summary>
        public string Color { get; init; } = "#757575";
    }

    /// <summary>
    /// ViewModel del reporte de una comisión (H12): se elige una comisión y se ve cuántos de sus estudiantes están en
    /// riesgo de abandonar, con una barra por nivel. Usa la misma evaluación que el panel, así que no puede discrepar.
    /// </summary>
    public class ReporteViewModel : BindableObject
    {
        /// <summary>
        /// Ancho de la barra que corresponde al total de estudiantes de la comisión.
        /// </summary>
        public const double AnchoMaximoBarra = 260;

        private readonly IDataService _dataService;
        private readonly ConsultaSeguimiento _consulta;

        /// <summary>
        /// Comisiones disponibles para elegir.
        /// </summary>
        public ObservableCollection<Comision> Comisiones { get; } = new();

        /// <summary>
        /// Una barra por nivel de la comisión elegida. Está vacía si no hay números que mostrar.
        /// </summary>
        public ObservableCollection<FilaGrafico> Filas { get; } = new();

        private Comision? _comisionSeleccionada;

        /// <summary>
        /// Comisión elegida; al cambiarla se carga su reporte.
        /// </summary>
        public Comision? ComisionSeleccionada
        {
            get => _comisionSeleccionada;
            set
            {
                _comisionSeleccionada = value;
                OnPropertyChanged();
                _ = CargarReporteAsync();
            }
        }

        private string _titular = string.Empty;

        /// <summary>
        /// Frase principal, por ejemplo "7 de 30 estudiantes en alto riesgo de abandonar".
        /// </summary>
        public string Titular
        {
            get => _titular;
            set { _titular = value; OnPropertyChanged(); }
        }

        private string _detalle = string.Empty;

        /// <summary>
        /// Los otros niveles, en una línea.
        /// </summary>
        public string Detalle
        {
            get => _detalle;
            set { _detalle = value; OnPropertyChanged(); }
        }

        private bool _hayGrafico;

        /// <summary>
        /// Indica si hay números que mostrar en el gráfico.
        /// </summary>
        public bool HayGrafico
        {
            get => _hayGrafico;
            set { _hayGrafico = value; OnPropertyChanged(); }
        }

        private string _estado = "Elegí una comisión para ver su reporte.";

        /// <summary>
        /// Indica qué falta elegir o qué salió mal al cargar. Está vacío cuando el reporte se muestra bien.
        /// </summary>
        public string Estado
        {
            get => _estado;
            set { _estado = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Crea el ViewModel con el acceso a datos (para las comisiones) y la consulta que evalúa a los estudiantes.
        /// </summary>
        public ReporteViewModel(IDataService dataService, ConsultaSeguimiento consulta)
        {
            _dataService = dataService;
            _consulta = consulta;
        }

        /// <summary>
        /// Recarga las comisiones. Si no cambiaron, solo refresca el reporte y conserva la comisión elegida.
        /// </summary>
        public async Task CargarComisionesAsync()
        {
            try
            {
                var lista = await _dataService.GetComisionesAsync();

                if (MismasComisiones(lista))
                    await CargarReporteAsync();
                else
                    RepoblarComisiones(lista);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                MostrarError("No se pudieron cargar las comisiones. Revisá la conexión con la base de datos.");
            }
        }

        /// <summary>
        /// Indica si la lista recibida tiene las mismas comisiones que ya están en el selector.
        /// </summary>
        private bool MismasComisiones(IEnumerable<Comision> lista) =>
            Comisiones.Select(c => c.Id).OrderBy(id => id)
                .SequenceEqual(lista.Select(c => c.Id).OrderBy(id => id));

        /// <summary>
        /// Reemplaza el selector por la lista recibida y deja elegida la anterior (o la primera).
        /// Al cambiar la elección, el setter carga su reporte (o explica qué falta).
        /// </summary>
        private void RepoblarComisiones(IEnumerable<Comision> lista)
        {
            var idPrevio = ComisionSeleccionada?.Id;
            Comisiones.Clear();

            foreach (var c in lista.OrderBy(c => c.Nombre))
            {
                Comisiones.Add(c);
            }

            ComisionSeleccionada = Comisiones.FirstOrDefault(c => c.Id == idPrevio) ?? Comisiones.FirstOrDefault();
        }

        /// <summary>
        /// Carga el reporte de la comisión elegida, o explica qué falta: crear la primera comisión o elegir una.
        /// </summary>
        private async Task CargarReporteAsync()
        {
            var comision = ComisionSeleccionada;

            if (comision == null)
                MostrarError(Comisiones.Count == 0
                    ? "Todavía no hay comisiones. Creá la primera en Carga de alumnos."
                    : "Elegí una comisión para ver su reporte.");
            else
                await TraerReporteAsync(comision);
        }

        /// <summary>
        /// Pide el reporte de la comisión y lo muestra. Si mientras consultaba cambió la comisión elegida, descarta el
        /// resultado porque ya no sirve. Si la consulta falla, lo dice en pantalla y no muestra números.
        /// </summary>
        private async Task TraerReporteAsync(Comision comision)
        {
            try
            {
                var reporte = await _consulta.ObtenerReporteAsync(comision.Id);
                if (comision != ComisionSeleccionada) return;

                MostrarReporte(reporte);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                MostrarError("No se pudo cargar el reporte. Revisá la conexión con la base de datos.");
            }
        }

        /// <summary>
        /// Muestra la frase principal y, si hay datos, la línea con los otros niveles y las cuatro barras.
        /// </summary>
        private void MostrarReporte(ReporteComision reporte)
        {
            Titular = PresentacionReporte.Titular(reporte);
            Detalle = PresentacionReporte.Detalle(reporte);
            HayGrafico = reporte.Estado == EstadoReporte.ConDatos;
            Estado = string.Empty;

            Filas.Clear();
            if (!HayGrafico) return;

            foreach (var fila in ArmarFilas(reporte))
            {
                Filas.Add(fila);
            }
        }

        /// <summary>
        /// Vacía el reporte y muestra el motivo.
        /// </summary>
        private void MostrarError(string motivo)
        {
            Titular = string.Empty;
            Detalle = string.Empty;
            HayGrafico = false;
            Filas.Clear();
            Estado = motivo;
        }

        /// <summary>
        /// Una barra por nivel, del más grave al menos grave.
        /// </summary>
        private static IEnumerable<FilaGrafico> ArmarFilas(ReporteComision reporte) => new[]
        {
            Fila(NivelSeguimiento.Rojo, reporte.AltoRiesgo, reporte.Total),
            Fila(NivelSeguimiento.Amarillo, reporte.RiesgoModerado, reporte.Total),
            Fila(NivelSeguimiento.Verde, reporte.AlDia, reporte.Total),
            Fila(NivelSeguimiento.SinDatos, reporte.EstudiantesSinDatos, reporte.Total)
        };

        /// <summary>
        /// Arma la barra de un nivel con su nombre, su color, su cantidad y su ancho.
        /// </summary>
        private static FilaGrafico Fila(NivelSeguimiento nivel, int cantidad, int total) => new()
        {
            Nivel = PresentacionNivel.Texto(nivel),
            Cantidad = cantidad,
            Resumen = $"{cantidad}  ({PresentacionReporte.Porcentaje(cantidad, total)})",
            Ancho = PresentacionReporte.Ancho(cantidad, total, AnchoMaximoBarra),
            Color = PresentacionNivel.Color(nivel)
        };
    }
}
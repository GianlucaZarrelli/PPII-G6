using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.Presentacion;
using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.ViewModels
{
    /// <summary>
    /// ViewModel del menú principal. Muestra el total de alumnos y cuántos están en riesgo alto, cambia el tema
    /// claro u oscuro y navega a los módulos.
    /// </summary>
    public class HomeViewModel : BindableObject
    {
        private readonly ConsultaSeguimiento _consulta;
        private readonly INavegacion _navegacion;
        private readonly ITema _tema;

        private int _totalEstudiantes;

        /// <summary>
        /// Total de alumnos de todas las comisiones.
        /// </summary>
        public int TotalEstudiantes
        {
            get => _totalEstudiantes;
            set { _totalEstudiantes = value; OnPropertyChanged(); }
        }

        private int _estudiantesEnRiesgo;

        /// <summary>
        /// Cantidad de alumnos en riesgo alto (nivel rojo).
        /// </summary>
        public int EstudiantesEnRiesgo
        {
            get => _estudiantesEnRiesgo;
            set { _estudiantesEnRiesgo = value; OnPropertyChanged(); }
        }

        private bool _temaOscuro;

        /// <summary>
        /// Es true si se muestra el tema oscuro. Al cambiarla desde el interruptor, se cambia el tema de la aplicación.
        /// </summary>
        public bool TemaOscuro
        {
            get => _temaOscuro;
            set
            {
                if (_temaOscuro == value) return;

                _temaOscuro = value;
                _tema.EsOscuro = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Comando para ir a la gestión de alumnos.
        /// </summary>
        public Command IrAAlumnosCommand { get; }

        /// <summary>
        /// Comando para ir al historial de asistencia.
        /// </summary>
        public Command IrAHistorialCommand { get; }

        /// <summary>
        /// Crea el ViewModel con la consulta que evalúa a los alumnos, la navegación y el tema de la aplicación.
        /// </summary>
        public HomeViewModel(ConsultaSeguimiento consulta, INavegacion navegacion, ITema tema)
        {
            _consulta = consulta;
            _navegacion = navegacion;
            _tema = tema;

            IrAAlumnosCommand = new Command(async () => await _navegacion.IrAAsync(Rutas.Alumnos));
            IrAHistorialCommand = new Command(async () => await _navegacion.IrAAsync(Rutas.Historial));
        }

        /// <summary>
        /// Sincroniza el interruptor con el tema actual, evalúa a todos los alumnos con el mismo criterio que el panel
        /// y la ficha, y cuenta los que están en rojo.
        /// </summary>
        public async Task CargarMetricasAsync()
        {
            SincronizarTema();

            try
            {
                var resumenes = await _consulta.ObtenerTodasAsync();

                TotalEstudiantes = resumenes.Count;
                EstudiantesEnRiesgo = resumenes.Count(r => r.Nivel == NivelSeguimiento.Rojo);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
            }
        }

        /// <summary>
        /// Muestra en el interruptor el tema que está activo, sin volver a aplicarlo.
        /// </summary>
        private void SincronizarTema()
        {
            _temaOscuro = _tema.EsOscuro;
            OnPropertyChanged(nameof(TemaOscuro));
        }
    }
}
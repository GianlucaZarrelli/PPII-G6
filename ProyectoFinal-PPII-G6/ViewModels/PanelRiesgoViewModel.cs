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
    /// Una fila del panel: un alumno con su nivel de riesgo y el dato que lo justifica.
    /// </summary>
    public class AlumnoRiesgoItem
    {
        /// <summary>
        /// Id del alumno, para abrir su ficha al tocarlo.
        /// </summary>
        public int EstudianteId { get; set; }

        /// <summary>
        /// Nombre y apellido del alumno.
        /// </summary>
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Texto del nivel de riesgo.
        /// </summary>
        public string Nivel { get; set; } = string.Empty;

        /// <summary>
        /// Color del nivel, en hexadecimal.
        /// </summary>
        public string ColorNivel { get; set; } = "#757575";

        /// <summary>
        /// Los números que justifican el nivel, por ejemplo "Asistencia 4/6 (67 %)".
        /// </summary>
        public string Detalle { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel del panel de seguimiento: muestra a los alumnos de una comisión ordenados por riesgo.
    /// </summary>
    public class PanelRiesgoViewModel : BindableObject
    {
        private readonly IDataService _dataService;
        private readonly ConsultaSeguimiento _consulta;

        /// <summary>
        /// Comisiones disponibles para elegir.
        /// </summary>
        public ObservableCollection<Comision> Comisiones { get; } = new();

        /// <summary>
        /// Alumnos de la comisión elegida, ya ordenados por riesgo.
        /// </summary>
        public ObservableCollection<AlumnoRiesgoItem> Alumnos { get; } = new();

        private Comision? _comisionSeleccionada;

        /// <summary>
        /// Comisión elegida; al cambiarla se recalcula el panel.
        /// </summary>
        public Comision? ComisionSeleccionada
        {
            get => _comisionSeleccionada;
            set
            {
                _comisionSeleccionada = value;
                OnPropertyChanged();
                _ = CargarAlumnosAsync();
            }
        }

        /// <summary>
        /// Crea el ViewModel con el acceso a datos y la consulta que evalúa a los alumnos.
        /// </summary>
        public PanelRiesgoViewModel(IDataService dataService, ConsultaSeguimiento consulta)
        {
            _dataService = dataService;
            _consulta = consulta;
        }

        /// <summary>
        /// Recarga las comisiones, conserva la selección anterior (o elige la primera) y recalcula el panel.
        /// </summary>
        public async Task CargarAsync()
        {
            try
            {
                var idPrevio = ComisionSeleccionada?.Id;
                var lista = await _dataService.GetComisionesAsync();

                RepoblarComisiones(lista);
                ComisionSeleccionada = Comisiones.FirstOrDefault(c => c.Id == idPrevio)
                                       ?? Comisiones.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
            }
        }

        /// <summary>
        /// Reemplaza el selector de comisiones por la lista recibida, ordenada por nombre.
        /// </summary>
        private void RepoblarComisiones(IEnumerable<Comision> lista)
        {
            Comisiones.Clear();
            foreach (var c in lista.OrderBy(c => c.Nombre))
            {
                Comisiones.Add(c);
            }
        }

        /// <summary>
        /// Pide los alumnos de la comisión elegida, ya evaluados y ordenados, y los muestra.
        /// </summary>
        private async Task CargarAlumnosAsync()
        {
            var comision = ComisionSeleccionada;
            Alumnos.Clear();
            if (comision == null) return;

            try
            {
                var resumenes = await _consulta.ObtenerComisionAsync(comision.Id);

                // Si mientras consultaba el usuario eligió otra comisión, este resultado ya no sirve
                if (comision != ComisionSeleccionada) return;

                MostrarAlumnos(resumenes);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
            }
        }

        /// <summary>
        /// Reemplaza la lista del panel por los resúmenes recibidos.
        /// </summary>
        private void MostrarAlumnos(IEnumerable<ResumenAlumno> resumenes)
        {
            Alumnos.Clear();
            foreach (var resumen in resumenes)
            {
                Alumnos.Add(CrearItem(resumen));
            }
        }

        /// <summary>
        /// Traduce un resumen a la fila que muestra el panel.
        /// </summary>
        private static AlumnoRiesgoItem CrearItem(ResumenAlumno resumen) => new()
        {
            EstudianteId = resumen.Estudiante.Id,
            NombreCompleto = resumen.Estudiante.NombreCompleto,
            Nivel = PresentacionNivel.Texto(resumen.Nivel),
            ColorNivel = PresentacionNivel.Color(resumen.Nivel),
            Detalle = $"{TextosSeguimiento.AsistenciaCorta(resumen)}  |  {TextosSeguimiento.EntregasCorta(resumen)}"
        };
    }
}
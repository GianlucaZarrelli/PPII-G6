using ProyectoFinal_PPII_G6.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProyectoFinal_PPII_G6.ViewModels
{
    /// <summary>
    /// ViewModel que gestiona la lógica de presentación para la consulta detallada
    /// del historial de asistencias e indicador de riesgo de un estudiante (Historia H4).
    /// </summary>
    public class DetalleEstudianteViewModel : INotifyPropertyChanged
    {
        private Estudiante _estudiante;
        private double _porcentajeAusencias;
        private string _nivelRiesgoTexto;

        /// <summary>
        /// Obtiene o establece la entidad <see cref="Estudiante"/> seleccionada para la consulta.
        /// </summary>
        public Estudiante Estudiante
        {
            get => _estudiante;
            set { _estudiante = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Obtiene o establece el porcentaje acumulado de ausencias calculadas.
        /// </summary>
        public double PorcentajeAusencias
        {
            get => _porcentajeAusencias;
            set { _porcentajeAusencias = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Obtiene o establece el texto descriptivo del nivel de alerta o riesgo académico.
        /// </summary>
        public string NivelRiesgoTexto
        {
            get => _nivelRiesgoTexto;
            set { _nivelRiesgoTexto = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Colección observable con el desglose del historial de asistencias por clase del estudiante.
        /// </summary>
        public ObservableCollection<Asistencia> HistorialAsistencias { get; set; }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="DetalleEstudianteViewModel"/>
        /// y carga los datos de prueba iniciales.
        /// </summary>
        public DetalleEstudianteViewModel()
        {
            HistorialAsistencias = new ObservableCollection<Asistencia>();
            CargarDatosSimulados();
        }

        /// <summary>
        /// Carga registros de prueba en la colección para permitir la previsualización
        /// e integración visual con la interfaz (Mock Data).
        /// </summary>
        private void CargarDatosSimulados()
        {
            Estudiante = new Estudiante(1, "Juan", "Pérez", "juan.perez@email.com");

            HistorialAsistencias.Add(new Asistencia(1, 1, 1, true));
            HistorialAsistencias.Add(new Asistencia(2, 1, 2, false));
            HistorialAsistencias.Add(new Asistencia(3, 1, 3, true));
            HistorialAsistencias.Add(new Asistencia(4, 1, 4, false));

            PorcentajeAusencias = 50.0;
            NivelRiesgoTexto = "Alto (Alerta Temprana)";
        }

        /// <summary>
        /// Evento que se dispara cuando cambia el valor de una propiedad.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifica a la interfaz gráfica que una propiedad ha actualizado su valor.
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad (infección automática).</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

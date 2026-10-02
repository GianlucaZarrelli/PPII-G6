using ProyectoFinal_PPII_G6.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ProyectoFinal_PPII_G6.ViewModels
{
    /// <summary>
    /// ViewModel encargado de coordinar la lógica de presentación para la gestión
    /// y alta de estudiantes (Historia de Usuario H2).
    /// </summary>
    public class EstudianteViewModel : INotifyPropertyChanged
    {
        private string _dni;
        private string _nombre;
        private string _apellido;

        /// <summary>
        /// Colección observable de estudiantes enlazada directamente con la vista para reflejar cambios en tiempo real.
        /// </summary>
        public ObservableCollection<Estudiante> Estudiantes { get; set; }

        /// <summary>
        /// Obtiene o establece el DNI ingresado en el formulario de alta.
        /// </summary>
        public string Dni
        {
            get => _dni;
            set { _dni = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Obtiene o establece el Nombre ingresado en el formulario de alta.
        /// </summary>
        public string Nombre
        {
            get => _nombre;
            set { _nombre = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Obtiene o establece el Apellido ingresado en el formulario de alta.
        /// </summary>
        public string Apellido
        {
            get => _apellido;
            set { _apellido = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Comando para procesar y guardar un nuevo estudiante desde el formulario.
        /// </summary>
        public ICommand GuardarEstudianteCommand { get; }

        /// <summary>
        /// Comando para eliminar un estudiante seleccionado de la lista.
        /// </summary>
        public ICommand EliminarEstudianteCommand { get; }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="EstudianteViewModel"/>,
        /// preparando la colección y asignando los comandos.
        /// </summary>
        public EstudianteViewModel()
        {
            Estudiantes = new ObservableCollection<Estudiante>();

            GuardarEstudianteCommand = new Command(GuardarEstudiante);
            EliminarEstudianteCommand = new Command<Estudiante>(EliminarEstudiante);
        }

        /// <summary>
        /// Valida los datos ingresados, crea la nueva entidad <see cref="Estudiante"/>,
        /// la agrega a la colección y limpia las entradas del formulario.
        /// </summary>
        private void GuardarEstudiante()
        {
            if (string.IsNullOrWhiteSpace(Dni) || string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(Apellido))
                return;

            var nuevo = new Estudiante(Estudiantes.Count + 1, Dni, Nombre, Apellido);
            Estudiantes.Add(nuevo);

            // Limpiar campos del formulario tras agregar el registro
            Dni = string.Empty;
            Nombre = string.Empty;
            Apellido = string.Empty;
        }

        /// <summary>
        /// Remueve de la nómina al estudiante pasado como parámetro.
        /// </summary>
        /// <param name="estudiante">Instancia del estudiante a eliminar.</param>
        private void EliminarEstudiante(Estudiante estudiante)
        {
            if (estudiante != null && Estudiantes.Contains(estudiante))
            {
                Estudiantes.Remove(estudiante);
            }
        }

        /// <summary>
        /// Ocurre cuando cambia el valor de una propiedad.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifica a la interfaz de usuario (XAML) que el valor de una propiedad ha cambiado.
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad modificada (detectado automáticamente).</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

using ProyectoFinal_PPII_G6.Views;

namespace ProyectoFinal_PPII_G6
{
    /// <summary>
    /// Contenedor de navegación de la aplicación (barra de pestañas).
    /// </summary>
    public partial class AppShell : Shell
    {
        /// <summary>
        /// Inicializa el Shell y registra las rutas de navegación internas.
        /// </summary>
        public AppShell()
        {
            InitializeComponent();

            // Registro de rutas internas para navegación con Shell.Current.GoToAsync(...)
            Routing.RegisterRoute(nameof(DetalleEstudiantePage), typeof(DetalleEstudiantePage));
            Routing.RegisterRoute(nameof(CargaMasivaPage), typeof(CargaMasivaPage));
            Routing.RegisterRoute(nameof(EstudiantePage), typeof(EstudiantePage));
        }
    }
}
using ProyectoFinal_PPII_G6.Views;

namespace ProyectoFinal_PPII_G6
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registro de rutas internas para navegación con Shell.Current.GoToAsync(...)
            Routing.RegisterRoute(nameof(DetalleEstudiantePage), typeof(DetalleEstudiantePage));
            Routing.RegisterRoute(nameof(CargaMasivaPage), typeof(CargaMasivaPage));
            Routing.RegisterRoute(nameof(EstudiantesPage), typeof(EstudiantesPage));
        }
    }
}
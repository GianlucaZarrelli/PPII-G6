using Microsoft.Extensions.DependencyInjection;

namespace ProyectoFinal_PPII_G6
{
    /// <summary>
    /// Clase principal de la aplicación.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Inicializa la aplicación y sus recursos.
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Crea la ventana principal con la navegación por pestañas.
        /// </summary>
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}

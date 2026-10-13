using Foundation;

namespace ProyectoFinal_PPII_G6
{
    /// <summary>
    /// Delegado de aplicación de iOS que inicia la app MAUI.
    /// </summary>
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        /// <summary>
        /// Crea la aplicación MAUI usando la configuración de MauiProgram.
        /// </summary>
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}

using Android.App;
using Android.Runtime;

namespace ProyectoFinal_PPII_G6
{
    /// <summary>
    /// Clase de aplicación de Android que inicia la app MAUI.
    /// </summary>
    [Application]
    public class MainApplication : MauiApplication
    {
        /// <summary>
        /// Constructor requerido por el runtime de Android.
        /// </summary>
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        /// <summary>
        /// Crea la aplicación MAUI usando la configuración de MauiProgram.
        /// </summary>
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}

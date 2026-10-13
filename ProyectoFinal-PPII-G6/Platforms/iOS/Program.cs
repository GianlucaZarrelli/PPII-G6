using ObjCRuntime;
using UIKit;

namespace ProyectoFinal_PPII_G6
{
    /// <summary>
    /// Punto de entrada de la aplicación en iOS.
    /// </summary>
    public class Program
    {
        // This is the main entry point of the application.
        /// <summary>
        /// Inicia la aplicación usando AppDelegate como delegado.
        /// </summary>
        static void Main(string[] args)
        {
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            UIApplication.Main(args, null, typeof(AppDelegate));
        }
    }
}

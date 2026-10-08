using Microsoft.Extensions.Logging;
using ProyectoFinal_PPII_G6.Services;
using ProyectoFinal_PPII_G6.ViewModels;
using ProyectoFinal_PPII_G6.Views;

namespace ProyectoFinal_PPII_G6
{
    /// <summary>
    /// Configuración inicial de la aplicación.
    /// </summary>
    public static class MauiProgram
    {
        /// <summary>
        /// Crea la aplicación y registra fuentes, servicios, ViewModels y vistas.
        /// </summary>
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Servicios de Persistencia y Lógica de Negocio
            builder.Services.AddSingleton<IDataService, SqlServerDataService>();
            builder.Services.AddSingleton<RiesgoService>();

            // ViewModels
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<EstudianteViewModel>();
            builder.Services.AddTransient<DetalleEstudianteViewModel>();

            // Vistas
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<EstudiantePage>();
            builder.Services.AddTransient<DetalleEstudiantePage>();

            return builder.Build();
        }
    }
}

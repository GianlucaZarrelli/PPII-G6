using Microsoft.Extensions.Logging;
using ProyectoFinal_PPII_G6.ViewModels;
using ProyectoFinal_PPII_G6.Views;


namespace ProyectoFinal_PPII_G6
{
    public static class MauiProgram
    {
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
            // ViewModels
            builder.Services.AddTransient<EstudianteViewModel>();
            builder.Services.AddTransient<DetalleEstudianteViewModel>();

            //Views
            builder.Services.AddTransient<EstudiantePage>();
            builder.Services.AddTransient<DetalleEstudiantePage>();

            return builder.Build();
        }
    }
}

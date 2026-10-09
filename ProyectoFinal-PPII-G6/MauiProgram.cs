using Microsoft.Extensions.Logging;
using ProyectoFinal_PPII_G6.Services;
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

            // 1. Persistencia y servicios de dominio
            // SqlServerDataService abre un AppDbContext por operación, por eso no se registra el contexto.
            builder.Services.AddSingleton<IDataService, SqlServerDataService>();
            builder.Services.AddSingleton<CsvImporterService>();
            builder.Services.AddSingleton<RiesgoService>();

            // 2. ViewModels
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<EstudianteViewModel>();
            builder.Services.AddTransient<DetalleEstudianteViewModel>();
            builder.Services.AddTransient<CargaMasivaViewModel>();

            // 3. Vistas (las que usa AppShell.xaml)
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<EstudiantePage>();
            builder.Services.AddTransient<DetalleEstudiantePage>();
            builder.Services.AddTransient<CargaMasivaPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
using Microsoft.Extensions.Logging;
using ProyectoFinal_PPII_G6.Services;
using ProyectoFinal_PPII_G6.ViewModels;
using ProyectoFinal_PPII_G6.Views;
using ProyectoFinal_PPII_G6.Presentacion;

namespace ProyectoFinal_PPII_G6
{
    /// <summary>
    /// Punto de configuración de la aplicación MAUI.
    /// </summary>
    public static class MauiProgram
    {
        /// <summary>
        /// Construye la aplicación: registra fuentes, servicios, ViewModels y vistas en el contenedor de dependencias.
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

            // 1. Persistencia y servicios de dominio
            builder.Services.AddSingleton<IDataService, SqlServerDataService>();
            builder.Services.AddSingleton<CsvImporterService>();
            builder.Services.AddSingleton<RiesgoService>();
            builder.Services.AddSingleton<SeleccionAlumnoService>();
            builder.Services.AddSingleton<SeguimientoService>();
            builder.Services.AddSingleton<ConsultaSeguimiento>();
            builder.Services.AddSingleton<IDialogoService, DialogoService>();

            // 2. ViewModels
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<EstudianteViewModel>();
            builder.Services.AddTransient<DetalleEstudianteViewModel>();
            builder.Services.AddTransient<CargaMasivaViewModel>();
            builder.Services.AddTransient<AsistenciaFechaViewModel>();
            builder.Services.AddTransient<PanelRiesgoViewModel>();
            builder.Services.AddTransient<EntregasViewModel>();

            // 3. Vistas (las que usa AppShell.xaml)
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<EstudiantePage>();
            builder.Services.AddTransient<DetalleEstudiantePage>();
            builder.Services.AddTransient<CargaMasivaPage>();
            builder.Services.AddTransient<AsistenciaFechaPage>();
            builder.Services.AddTransient<PanelRiesgoPage>();
            builder.Services.AddTransient<EntregasPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
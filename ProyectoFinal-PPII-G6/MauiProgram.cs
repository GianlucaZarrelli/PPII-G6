using Microsoft.Extensions.Logging;
using ProyectoFinal_PPII_G6.Presentacion;
using ProyectoFinal_PPII_G6.Services;
using ProyectoFinal_PPII_G6.ViewModels;
using ProyectoFinal_PPII_G6.Views;

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

            RegistrarServicios(builder.Services);
            RegistrarViewModels(builder.Services);
            RegistrarPaginas(builder.Services);

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        /// <summary>
        /// Registra la persistencia y los servicios de dominio. Todos son únicos: se crean una vez y se comparten.
        /// </summary>
        private static void RegistrarServicios(IServiceCollection services)
        {
            services.AddSingleton<IDataService, SqlServerDataService>();
            services.AddSingleton<CsvImporterService>();
            services.AddSingleton<RiesgoService>();
            services.AddSingleton<SeleccionAlumnoService>();
            services.AddSingleton<SeguimientoService>();
            services.AddSingleton<ConsultaSeguimiento>();
            services.AddSingleton<IDialogoService, DialogoService>();
        }

        /// <summary>
        /// Registra los ViewModels. Son transitorios: cada pantalla recibe el suyo.
        /// </summary>
        private static void RegistrarViewModels(IServiceCollection services)
        {
            services.AddTransient<HomeViewModel>();
            services.AddTransient<EstudianteViewModel>();
            services.AddTransient<DetalleEstudianteViewModel>();
            services.AddTransient<CargaMasivaViewModel>();
            services.AddTransient<AsistenciaFechaViewModel>();
            services.AddTransient<PanelRiesgoViewModel>();
            services.AddTransient<EntregasViewModel>();
        }

        /// <summary>
        /// Registra las vistas que usa AppShell.xaml.
        /// </summary>
        private static void RegistrarPaginas(IServiceCollection services)
        {
            services.AddTransient<HomePage>();
            services.AddTransient<EstudiantePage>();
            services.AddTransient<DetalleEstudiantePage>();
            services.AddTransient<CargaMasivaPage>();
            services.AddTransient<AsistenciaFechaPage>();
            services.AddTransient<PanelRiesgoPage>();
            services.AddTransient<EntregasPage>();
        }
    }
}
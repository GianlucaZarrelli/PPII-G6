using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.Presentacion
{
    /// <summary>
    /// Lee y cambia el tema claro u oscuro de la aplicación MAUI. Es la única clase que conoce cómo se aplica.
    /// </summary>
    public class TemaApp : ITema
    {
        /// <summary>
        /// Lee el tema que se está mostrando (incluso si el usuario no eligió ninguno) y, al asignarla, fija el tema elegido.
        /// </summary>
        public bool EsOscuro
        {
            get => Application.Current?.RequestedTheme == AppTheme.Dark;
            set
            {
                if (Application.Current != null)
                {
                    Application.Current.UserAppTheme = value ? AppTheme.Dark : AppTheme.Light;
                }
            }
        }
    }
}
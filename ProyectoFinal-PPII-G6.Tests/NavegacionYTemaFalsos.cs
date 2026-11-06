using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Navegación falsa: en vez de cambiar de pantalla, anota a qué rutas se pidió ir.
    /// </summary>
    public class NavegacionFalsa : INavegacion
    {
        /// <summary>
        /// Rutas a las que se pidió ir, en orden.
        /// </summary>
        public List<string> Visitadas { get; } = new();

        public Task IrAAsync(string ruta)
        {
            Visitadas.Add(ruta);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Tema falso: guarda en una propiedad si el tema es oscuro, sin tocar la aplicación.
    /// </summary>
    public class TemaFalso : ITema
    {
        public bool EsOscuro { get; set; }
    }
}
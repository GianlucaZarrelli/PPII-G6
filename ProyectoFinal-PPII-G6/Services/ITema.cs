namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Permite que un ViewModel lea y cambie el tema claro u oscuro sin conocer cómo se aplica.
    /// </summary>
    public interface ITema
    {
        /// <summary>
        /// Es true si la aplicación se muestra con el tema oscuro. Al asignarla, se cambia el tema de la aplicación.
        /// </summary>
        bool EsOscuro { get; set; }
    }
}
using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.Presentacion
{
    /// <summary>
    /// Texto y color con los que se muestra cada nivel. Es el único lugar donde están escritos.
    /// </summary>
    public static class PresentacionNivel
    {
        /// <summary>
        /// Texto que se muestra para el nivel.
        /// </summary>
        public static string Texto(NivelSeguimiento nivel) => nivel switch
        {
            NivelSeguimiento.Rojo => "ALTO RIESGO",
            NivelSeguimiento.Amarillo => "RIESGO MODERADO",
            NivelSeguimiento.Verde => "AL DÍA",
            _ => "SIN DATOS"
        };

        /// <summary>
        /// Color (en hexadecimal) con el que se muestra el nivel.
        /// </summary>
        public static string Color(NivelSeguimiento nivel) => nivel switch
        {
            NivelSeguimiento.Rojo => "#D32F2F",
            NivelSeguimiento.Amarillo => "#F57C00",
            NivelSeguimiento.Verde => "#388E3C",
            _ => "#757575"
        };
    }
}
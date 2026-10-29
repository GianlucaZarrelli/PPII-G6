using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.Presentacion
{
    /// <summary>
    /// Texto y color con los que se muestra cada estado de una fila de la importación.
    /// Es el único lugar donde están escritos.
    /// </summary>
    public static class PresentacionEstadoFila
    {
        /// <summary>
        /// Texto que se muestra para el estado.
        /// </summary>
        public static string Texto(EstadoFila estado) => estado switch
        {
            EstadoFila.Nuevo => "Nuevo",
            EstadoFila.YaExiste => "Ya existe en la comisión",
            EstadoFila.RepetidoEnArchivo => "DNI repetido en el archivo",
            EstadoFila.DatosIncompletos => "Datos incompletos",
            EstadoFila.DniInvalido => "DNI inválido",
            EstadoFila.DatoDemasiadoLargo => "Dato demasiado largo",
            _ => "Email inválido"
        };

        /// <summary>
        /// Color (en hexadecimal) con el que se muestra el estado: verde si se importa, gris si ya existe,
        /// naranja si está repetido y rojo si tiene un error.
        /// </summary>
        public static string Color(EstadoFila estado) => estado switch
        {
            EstadoFila.Nuevo => "#388E3C",
            EstadoFila.YaExiste => "#757575",
            EstadoFila.RepetidoEnArchivo => "#F57C00",
            _ => "#D32F2F"
        };
    }
}
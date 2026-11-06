using System;
using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.Presentacion
{
    /// <summary>
    /// Textos y colores con los que se muestra el contacto con un alumno. Es el único lugar donde están escritos.
    /// </summary>
    public static class PresentacionContacto
    {
        /// <summary>
        /// Fecha y hora de un contacto, como se muestran en toda la aplicación.
        /// </summary>
        public static string FechaHora(DateTime fechaHora) => fechaHora.ToString("dd/MM/yyyy HH:mm");

        /// <summary>
        /// Resume el contacto con el alumno: la fecha y hora del último y cuántos hay, o "Sin contactar".
        /// </summary>
        public static string Texto(ResumenAlumno resumen) =>
            resumen.UltimoContacto is DateTime ultimo
                ? $"Último contacto: {FechaHora(ultimo)} ({Cantidad(resumen.CantidadContactos)})"
                : "Sin contactar";

        /// <summary>
        /// Color del resumen: verde si ya lo contactaron; naranja si está en riesgo y todavía nadie lo contactó
        /// (es el pendiente que hay que atender); gris en el resto.
        /// </summary>
        public static string Color(ResumenAlumno resumen) => (resumen.UltimoContacto, resumen.Nivel) switch
        {
            (not null, _) => "#388E3C",
            (null, NivelSeguimiento.Rojo or NivelSeguimiento.Amarillo) => "#F57C00",
            _ => "#757575"
        };

        /// <summary>
        /// "1 contacto" o "N contactos".
        /// </summary>
        private static string Cantidad(int cantidad) => cantidad == 1 ? "1 contacto" : $"{cantidad} contactos";
    }
}
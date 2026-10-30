using ProyectoFinal_PPII_G6.Models;

namespace ProyectoFinal_PPII_G6.Presentacion
{
    /// <summary>
    /// Texto con el que se muestra cada medio de contacto. Es el único lugar donde está escrito.
    /// </summary>
    public static class PresentacionMedio
    {
        /// <summary>
        /// Texto que se muestra para el medio.
        /// </summary>
        public static string Texto(MedioContacto medio) => medio switch
        {
            MedioContacto.Llamada => "Llamada",
            MedioContacto.Mail => "Mail",
            MedioContacto.Presencial => "Presencial",
            _ => "Mensaje (WhatsApp)"
        };
    }
}
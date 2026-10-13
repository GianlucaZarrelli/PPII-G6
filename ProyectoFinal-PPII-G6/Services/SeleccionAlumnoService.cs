namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Recuerda qué alumno hay que mostrar en el Historial cuando se llega desde otra pantalla (por ejemplo, el Panel).
    /// </summary>
    public class SeleccionAlumnoService
    {
        private int? _estudianteIdPendiente;

        /// <summary>
        /// Anota el alumno que el Historial tiene que abrir la próxima vez que se muestre.
        /// </summary>
        public void Pedir(int estudianteId)
        {
            _estudianteIdPendiente = estudianteId;
        }

        /// <summary>
        /// Devuelve el alumno pendiente (si hay uno) y lo borra, para que se use una sola vez.
        /// </summary>
        public int? TomarPendiente()
        {
            var id = _estudianteIdPendiente;
            _estudianteIdPendiente = null;
            return id;
        }
    }
}
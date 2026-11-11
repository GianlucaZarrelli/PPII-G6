using System.Linq;
using System.Threading.Tasks;
using ProyectoFinal_PPII_G6.Models;

namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Alta de un estudiante en una comisión. La regla de que un DNI no se repite dentro de la comisión vive acá,
    /// en el dominio: no depende de la pantalla ni solo del índice único de la base de datos, así que quien carga
    /// recibe "este DNI ya está cargado para Pérez, Juan" y no un error de la base.
    /// El formato de los demás campos lo valida cada regla (nombre, apellido y email) y lo muestra la pantalla.
    /// </summary>
    public class RegistrarEstudianteService
    {
        private readonly IDataService _dataService;

        /// <summary>
        /// Crea el servicio con el acceso a datos donde se guarda la nómina.
        /// </summary>
        public RegistrarEstudianteService(IDataService dataService)
        {
            _dataService = dataService;
        }

        /// <summary>
        /// Guarda al estudiante si su DNI es válido y no está cargado en su comisión. Devuelve null si lo guardó, o el
        /// motivo del rechazo (y en ese caso no guarda nada). El DNI se guarda sin puntos ni espacios, de modo que
        /// "12.345.678" y "12345678" sean el mismo.
        /// </summary>
        public async Task<string?> RegistrarAsync(Estudiante estudiante)
        {
            var dni = ValidacionDni.Normalizar(estudiante.Dni);
            var rechazo = ValidacionDni.ObtenerError(dni) ?? await ErrorDeDuplicadoAsync(dni, estudiante.ComisionId);

            if (rechazo != null)
                return rechazo;

            estudiante.Dni = dni;
            await _dataService.SaveEstudianteAsync(estudiante);
            return null;
        }

        /// <summary>
        /// Avisa a qué estudiante de la comisión pertenece el DNI si ya está cargado, o devuelve null si está libre.
        /// Consulta la nómina guardada y no la que muestra la pantalla, que puede estar desactualizada.
        /// </summary>
        private async Task<string?> ErrorDeDuplicadoAsync(string dni, int comisionId)
        {
            var nomina = await _dataService.GetEstudiantesByComisionIdAsync(comisionId);
            var existente = nomina.FirstOrDefault(e => ValidacionDni.Normalizar(e.Dni) == dni);

            return existente == null ? null : $"El DNI {dni} ya está cargado para {existente.Apellido}, {existente.Nombre}.";
        }
    }
}
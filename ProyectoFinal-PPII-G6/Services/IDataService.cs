using System.Collections.Generic;
using System.Threading.Tasks;
using ProyectoFinal_PPII_G6.Models;

namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Define las operaciones de acceso a datos de estudiantes, clases y asistencias.
    /// </summary>
    public interface IDataService
    {
        // Estudiantes

        /// <summary>
        /// Obtiene todos los estudiantes.
        /// </summary>
        Task<List<Estudiante>> GetEstudiantesAsync();

        /// <summary>
        /// Busca un estudiante por su Id. Devuelve null si no existe.
        /// </summary>
        Task<Estudiante?> GetEstudianteByIdAsync(int id);

        /// <summary>
        /// Guarda un estudiante (lo crea si el Id es 0, si no lo actualiza) y devuelve su Id.
        /// </summary>
        Task<int> SaveEstudianteAsync(Estudiante estudiante);

        /// <summary>
        /// Elimina un estudiante por su Id y devuelve la cantidad de filas eliminadas.
        /// </summary>
        Task<int> DeleteEstudianteAsync(int id);

        // Clases

        /// <summary>
        /// Obtiene todas las clases, de la más reciente a la más antigua.
        /// </summary>
        Task<List<Clase>> GetClasesAsync();

        /// <summary>
        /// Guarda una clase (la crea si el Id es 0, si no la actualiza) y devuelve su Id.
        /// </summary>
        Task<int> SaveClaseAsync(Clase clase);

        // Asistencias

        /// <summary>
        /// Obtiene las asistencias de un estudiante.
        /// </summary>
        Task<List<Asistencia>> GetAsistenciasPorEstudianteAsync(int estudianteId);

        /// <summary>
        /// Obtiene todas las asistencias en una sola consulta.
        /// </summary>
        Task<List<Asistencia>> GetTodasLasAsistenciasAsync();

        /// <summary>
        /// Guarda una asistencia (la crea si el Id es 0, si no la actualiza) y devuelve su Id.
        /// </summary>
        Task<int> SaveAsistenciaAsync(Asistencia asistencia);
    }
}

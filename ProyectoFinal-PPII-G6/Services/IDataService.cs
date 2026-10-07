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

        Task<List<Estudiante>> GetEstudiantesByComisionIdAsync(int comisionId);

        /// <summary>
        /// Guarda un estudiante (lo crea si el Id es 0, si no lo actualiza) y devuelve su Id.
        /// </summary>
        Task<int> SaveEstudianteAsync(Estudiante estudiante);

        Task SaveEstudiantesMassiveAsync(List<Estudiante> estudiantes);

        // Clases

        /// <summary>
        /// Obtiene todas las clases, de la más reciente a la más antigua.
        /// </summary>
        Task<List<Clase>> GetClasesAsync();

        /// <summary>
        /// Guarda una clase (la crea si el Id es 0, si no la actualiza) y devuelve su Id.
        /// </summary>
        Task<int> SaveClaseAsync(Clase clase);

        Task<List<Clase>> GetClasesByComisionIdAsync(int comisionId);

        Task<int> SaveAsistenciasAsync(List<Asistencia> asistencias);

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

        Task<List<Comision>> GetComisionesAsync();
        Task<Comision?> GetComisionByIdAsync(int id);

        // Trabajos Practicos

        Task<List<TrabajoPractico>> GetTrabajosPracticosByComisionIdAsync(int comisionId);

        Task<TrabajoPractico> SaveTrabajoPracticoAsync(TrabajoPractico tp);

        Task<List<Entrega>> GetEntregasPorEstudianteAsync(int estudianteId);

        Task SaveEntregasAsync(List<Entrega> entregas);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using ProyectoFinal_PPII_G6.Models;

namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Define las operaciones de acceso a datos de estudiantes, comisiones, clases, asistencias, trabajos prácticos y entregas.
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
        /// Obtiene los estudiantes de una comisión.
        /// </summary>
        Task<List<Estudiante>> GetEstudiantesByComisionIdAsync(int comisionId);

        /// <summary>
        /// Guarda un estudiante (lo crea si el Id es 0, si no lo actualiza) y devuelve su Id.
        /// </summary>
        Task<int> SaveEstudianteAsync(Estudiante estudiante);

        /// <summary>
        /// Agrega en una sola operación una lista de estudiantes nuevos.
        /// </summary>
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

        /// <summary>
        /// Obtiene las clases de una comisión, de la más reciente a la más antigua.
        /// </summary>
        Task<List<Clase>> GetClasesByComisionIdAsync(int comisionId);

        /// <summary>
        /// Guarda varias asistencias (crea las de Id 0 y actualiza el resto) y devuelve la cantidad procesada.
        /// </summary>
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

        /// <summary>
        /// Obtiene todas las comisiones.
        /// </summary>
        Task<List<Comision>> GetComisionesAsync();
        /// <summary>
        /// Busca una comisión por su Id. Devuelve null si no existe.
        /// </summary>
        Task<Comision?> GetComisionByIdAsync(int id);

        /// <summary>
        /// Devuelve la comisión con el nombre indicado, creándola si no existe.
        /// </summary>
        Task<Comision> GetOrCreateComisionAsync(string nombre);

        // Trabajos Practicos

        /// <summary>
        /// Obtiene los trabajos prácticos de una comisión.
        /// </summary>
        Task<List<TrabajoPractico>> GetTrabajosPracticosByComisionIdAsync(int comisionId);

        /// <summary>
        /// Guarda un trabajo práctico (lo crea si el Id es 0, si no lo actualiza) y lo devuelve.
        /// </summary>
        Task<TrabajoPractico> SaveTrabajoPracticoAsync(TrabajoPractico tp);

        /// <summary>
        /// Obtiene las entregas de un estudiante.
        /// </summary>
        Task<List<Entrega>> GetEntregasPorEstudianteAsync(int estudianteId);

        /// <summary>
        /// Guarda varias entregas (crea las de Id 0 y actualiza el resto).
        /// </summary>
        Task SaveEntregasAsync(List<Entrega> entregas);


    }
}

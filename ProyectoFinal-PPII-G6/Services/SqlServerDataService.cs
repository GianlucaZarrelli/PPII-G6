using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_PPII_G6.Models;

namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Acceso a datos sobre SQL Server usando Entity Framework.
    /// </summary>
    public class SqlServerDataService : IDataService
    {
        /// <summary>
        /// Crea un contexto de base de datos nuevo para cada operación.
        /// </summary>
        private AppDbContext CreateContext() => new AppDbContext();

        /// <summary>
        /// Obtiene todos los estudiantes.
        /// </summary>
        public async Task<List<Estudiante>> GetEstudiantesAsync()
        {
            using var context = CreateContext();
            return await context.Estudiantes.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Busca un estudiante por su Id.
        /// </summary>
        public async Task<Estudiante?> GetEstudianteByIdAsync(int id)
        {
            using var context = CreateContext();
            return await context.Estudiantes.FindAsync(id);
        }

        /// <summary>
        /// Crea o actualiza un estudiante y devuelve su Id.
        /// </summary>
        public async Task<int> SaveEstudianteAsync(Estudiante estudiante)
        {
            using var context = CreateContext();

            if (estudiante.Id == 0)
                await context.Estudiantes.AddAsync(estudiante);
            else
                context.Estudiantes.Update(estudiante);

            await context.SaveChangesAsync();
            return estudiante.Id; // EF Core completa el Id autogenerado por SQL Server tras SaveChanges
        }

        /// <summary>
        /// Elimina un estudiante por su Id.
        /// </summary>
        public async Task<int> DeleteEstudianteAsync(int id)
        {
            using var context = CreateContext();
            var estudiante = await context.Estudiantes.FindAsync(id);
            if (estudiante != null)
            {
                context.Estudiantes.Remove(estudiante);
                return await context.SaveChangesAsync();
            }
            return 0;
        }

        /// <summary>
        /// Obtiene todas las clases ordenadas por fecha descendente.
        /// </summary>
        public async Task<List<Clase>> GetClasesAsync()
        {
            using var context = CreateContext();
            return await context.Clases.AsNoTracking().OrderByDescending(c => c.Fecha).ToListAsync();
        }

        /// <summary>
        /// Crea o actualiza una clase y devuelve su Id.
        /// </summary>
        public async Task<int> SaveClaseAsync(Clase clase)
        {
            using var context = CreateContext();

            if (clase.Id == 0)
                await context.Clases.AddAsync(clase);
            else
                context.Clases.Update(clase);

            await context.SaveChangesAsync();
            return clase.Id;
        }

        /// <summary>
        /// Obtiene las asistencias de un estudiante.
        /// </summary>
        public async Task<List<Asistencia>> GetAsistenciasPorEstudianteAsync(int estudianteId)
        {
            using var context = CreateContext();
            return await context.Asistencias
                .AsNoTracking()
                .Where(a => a.EstudianteId == estudianteId)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene todas las asistencias.
        /// </summary>
        public async Task<List<Asistencia>> GetTodasLasAsistenciasAsync()
        {
            using var context = CreateContext();
            return await context.Asistencias.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Crea o actualiza una asistencia y devuelve su Id.
        /// </summary>
        public async Task<int> SaveAsistenciaAsync(Asistencia asistencia)
        {
            using var context = CreateContext();

            if (asistencia.Id == 0)
                await context.Asistencias.AddAsync(asistencia);
            else
                context.Asistencias.Update(asistencia);

            await context.SaveChangesAsync();
            return asistencia.Id;
        }
    }
}

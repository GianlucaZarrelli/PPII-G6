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

        public async Task<List<Comision>> GetComisionesAsync()
        {
            using var context = CreateContext();
            return await context.Comisiones.AsNoTracking().ToListAsync();
        }

        public async Task<Comision?> GetComisionByIdAsync(int id)
        {
            using var context = CreateContext();
            return await context.Comisiones.FindAsync(id);

        }

        public async Task<List<Estudiante>> GetEstudiantesByComisionIdAsync(int comisionId)
        {
            using var context = CreateContext();
            return await context.Estudiantes
                .AsNoTracking()
                .Where(e => e.ComisionId == comisionId)
                .ToListAsync();
        }

        public async Task SaveEstudiantesMassiveAsync(List<Estudiante> estudiantes)
        {
            using var context = CreateContext();
            await context.Estudiantes.AddRangeAsync(estudiantes);
            await context.SaveChangesAsync();
        }

        public async Task<List<Clase>> GetClasesByComisionIdAsync(int comisionId)
        {
            using var context = CreateContext();
            return await context.Clases
                .AsNoTracking()
                .Where(c => c.ComisionId == comisionId)
                .OrderByDescending(c => c.Fecha)
                .ToListAsync();
        }

        public async Task<int> SaveAsistenciasAsync(List<Asistencia> asistencias)
        {
            using var context = CreateContext();
            foreach (var asistencia in asistencias)
            {
                if (asistencia.Id == 0)
                    await context.Asistencias.AddAsync(asistencia);
                else
                    context.Asistencias.Update(asistencia);
            }
            await context.SaveChangesAsync();
            return asistencias.Count;
        }

        public async Task<List<TrabajoPractico>> GetTrabajosPracticosByComisionIdAsync(int comisionId)
        {
            using var context = CreateContext();
            return await context.TrabajosPracticos
                .AsNoTracking()
                .Where(tp => tp.ComisionId == comisionId)
                .ToListAsync();
        }

        public async Task<TrabajoPractico> SaveTrabajoPracticoAsync(TrabajoPractico tp)
        {
            using var context = CreateContext();
            if (tp.Id == 0)
                await context.TrabajosPracticos.AddAsync(tp);
            else
                context.TrabajosPracticos.Update(tp);
            await context.SaveChangesAsync();
            return tp;
        }

        public async Task SaveEntregasAsync(List<Entrega> entregas)
        {
            using var context = CreateContext();
            foreach (var entrega in entregas)
            {
                if (entrega.Id == 0)
                    await context.Entregas.AddAsync(entrega);
                else
                    context.Entregas.Update(entrega);
            }

            await context.SaveChangesAsync();
        }

        public async Task<List<Entrega>> GetEntregasPorEstudianteAsync(int estudianteId)
        {
            using var context = CreateContext();
            return await context.Entregas
                .AsNoTracking()
                .Where(e => e.EstudianteId == estudianteId)
                .ToListAsync();
        }
    }
}
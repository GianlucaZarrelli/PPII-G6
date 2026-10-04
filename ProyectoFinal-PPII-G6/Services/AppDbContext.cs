using Microsoft.EntityFrameworkCore;
using ProyectoFinal_PPII_G6.Models;

namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Contexto de Entity Framework para la base de datos SistemaAlertaTemprana.
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Tabla de estudiantes.
        /// </summary>
        public DbSet<Estudiante> Estudiantes { get; set; }

        /// <summary>
        /// Tabla de clases.
        /// </summary>
        public DbSet<Clase> Clases { get; set; }

        /// <summary>
        /// Tabla de asistencias.
        /// </summary>
        public DbSet<Asistencia> Asistencias { get; set; }

        /// <summary>
        /// Configura la conexión a SQL Server.
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=SistemaAlertaTemprana;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }

        /// <summary>
        /// Define las tablas, claves y restricciones de cada entidad.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Estudiante>(entity =>
            {
                entity.ToTable("Estudiantes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Dni).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(100);
            });

            modelBuilder.Entity<Clase>(entity =>
            {
                entity.ToTable("Clases");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Fecha).IsRequired();
                entity.Ignore(c => c.Descripcion); // Se ignora porque la tabla Clases no tiene columna Descripcion
            });

            modelBuilder.Entity<Asistencia>(entity =>
            {
                entity.ToTable("Asistencias");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.EstudianteId).IsRequired();
                entity.Property(a => a.ClaseId).IsRequired();
                entity.Property(a => a.Presente).IsRequired();
            });
        }
    }
}

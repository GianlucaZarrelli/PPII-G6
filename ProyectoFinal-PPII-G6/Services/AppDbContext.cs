using Microsoft.EntityFrameworkCore;
using ProyectoFinal_PPII_G6.Models;

namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Contexto de Entity Framework para la base de datos SistemaAlertaTemprana.
    /// </summary>
    public class AppDbContext : DbContext
    {

        public DbSet<Comision> Comisiones { get; set; }

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

        public DbSet<TrabajoPractico> TrabajosPracticos { get; set; }

        public DbSet<Entrega> Entregas { get; set; }

        /// <summary>
        /// Configura la conexión a SQL Server.
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = Environment.GetEnvironmentVariable("PPII_DB") ?? "Server=.\\SQLEXPRESS;Database=SistemaAlertaTemprana;Trusted_Connection=True;TrustServerCertificate=True;";

                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        /// <summary>
        /// Define las tablas, claves y restricciones de cada entidad.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Comision>(entity =>
            {
                entity.ToTable("Comisiones");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
            });

            modelBuilder.Entity<Estudiante>(entity =>
            {
                entity.ToTable("Estudiantes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Dni).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.ComisionId).IsRequired();
            });

            modelBuilder.Entity<Clase>(entity =>
            {
                entity.ToTable("Clases");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Fecha).IsRequired();
                entity.Property(c => c.Descripcion).HasMaxLength(200);
                entity.Property(c => c.ComisionId).IsRequired();
            });

            modelBuilder.Entity<Asistencia>(entity =>
            {
                entity.ToTable("Asistencias");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.EstudianteId).IsRequired();
                entity.Property(a => a.ClaseId).IsRequired();
                entity.Property(a => a.Presente).IsRequired();
                entity.Property(a => a.Justificada).IsRequired();
            });

            modelBuilder.Entity<TrabajoPractico>(entity =>
            {
                entity.ToTable("TrabajosPracticos");
                entity.HasKey(tp => tp.Id);
                entity.Property(tp => tp.Titulo).IsRequired().HasMaxLength(150);
                entity.Property(tp => tp.FechaEntrega).IsRequired();
                entity.Property(tp => tp.ComisionId).IsRequired();
            });

            modelBuilder.Entity<Entrega>(entity =>
            {
                entity.ToTable("Entregas");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EstudianteId).IsRequired();
                entity.Property(e => e.TrabajoPracticoId).IsRequired();
                entity.Property(e => e.Entregado).IsRequired();
            });
        }
    }
}

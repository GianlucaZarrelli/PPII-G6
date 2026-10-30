using Microsoft.EntityFrameworkCore;
using ProyectoFinal_PPII_G6.Models;

namespace ProyectoFinal_PPII_G6.Infraestructura
{
    /// <summary>
    /// Contexto de Entity Framework para la base de datos SistemaAlertaTemprana.
    /// </summary>
    public class AppDbContext : DbContext
    {

        /// <summary>
        /// Tabla de comisiones.
        /// </summary>
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

        /// <summary>
        /// Tabla de trabajos prácticos.
        /// </summary>
        public DbSet<TrabajoPractico> TrabajosPracticos { get; set; }

        /// <summary>
        /// Tabla de entregas de trabajos prácticos.
        /// </summary>
        public DbSet<Entrega> Entregas { get; set; }
        
        /// <summary>
        /// Tabla de contactos (intervenciones) con los estudiantes.
        /// </summary>
        public DbSet<Contacto> Contactos { get; set; }

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

            ConfigurarComision(modelBuilder);
            ConfigurarEstudiante(modelBuilder);
            ConfigurarClase(modelBuilder);
            ConfigurarAsistencia(modelBuilder);
            ConfigurarTrabajoPractico(modelBuilder);
            ConfigurarEntrega(modelBuilder);
            ConfigurarContacto(modelBuilder);
        }

        /// <summary>
        /// Tabla Comisiones.
        /// </summary>
        private static void ConfigurarComision(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comision>(entity =>
            {
                entity.ToTable("Comisiones");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
            });
        }

        /// <summary>
        /// Tabla Estudiantes.
        /// </summary>
        private static void ConfigurarEstudiante(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Estudiante>(entity =>
            {
                entity.ToTable("Estudiantes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Dni).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.FechaAlta).IsRequired();
                entity.Property(e => e.ComisionId).IsRequired();
            });
        }

        /// <summary>
        /// Tabla Clases.
        /// </summary>
        private static void ConfigurarClase(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Clase>(entity =>
            {
                entity.ToTable("Clases");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Fecha).IsRequired();
                entity.Property(c => c.Descripcion).HasMaxLength(200);
                entity.Property(c => c.ComisionId).IsRequired();
            });
        }

        /// <summary>
        /// Tabla Asistencias.
        /// </summary>
        private static void ConfigurarAsistencia(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Asistencia>(entity =>
            {
                entity.ToTable("Asistencias");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.EstudianteId).IsRequired();
                entity.Property(a => a.ClaseId).IsRequired();
                entity.Property(a => a.Presente).IsRequired();
                entity.Property(a => a.Justificada).IsRequired();
            });
        }

        /// <summary>
        /// Tabla TrabajosPracticos.
        /// </summary>
        private static void ConfigurarTrabajoPractico(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TrabajoPractico>(entity =>
            {
                entity.ToTable("TrabajosPracticos");
                entity.HasKey(tp => tp.Id);
                entity.Property(tp => tp.Titulo).IsRequired().HasMaxLength(150);
                entity.Property(tp => tp.FechaEntrega).IsRequired();
                entity.Property(tp => tp.ComisionId).IsRequired();
            });
        }

        /// <summary>
        /// Tabla Entregas.
        /// </summary>
        private static void ConfigurarEntrega(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entrega>(entity =>
            {
                entity.ToTable("Entregas");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EstudianteId).IsRequired();
                entity.Property(e => e.TrabajoPracticoId).IsRequired();
                entity.Property(e => e.Entregado).IsRequired();
            });
        }
        
        
        /// <summary>
        /// Tabla Contactos. El medio se guarda con el nombre del valor (Llamada, Mail...) para que se lea en la base.
        /// </summary>
        private static void ConfigurarContacto(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Contacto>(entity =>
            {
                entity.ToTable("Contactos");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.EstudianteId).IsRequired();
                entity.Property(c => c.FechaHora).IsRequired();
                entity.Property(c => c.Medio).IsRequired().HasConversion<string>().HasMaxLength(20);
                entity.Property(c => c.Resultado).IsRequired().HasMaxLength(500);
            });
        }
    }
}
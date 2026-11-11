using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_PPII_G6.Infraestructura;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Una base SQLite real que vive en memoria mientras la conexión esté abierta. Cada prueba arranca con una base
    /// nueva y vacía, armada desde el modelo de AppDbContext. Hace de fábrica de contextos, así que el adaptador
    /// no se entera de que está en una prueba.
    /// </summary>
    internal sealed class BaseEnMemoria : IDbContextFactory<AppDbContext>, IDisposable
    {
        private readonly SqliteConnection _conexion;
        private readonly DbContextOptions<AppDbContext> _opciones;

        public BaseEnMemoria()
        {
            _conexion = new SqliteConnection("Filename=:memory:");
            _conexion.Open();
            _opciones = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conexion).Options;

            using var contexto = CreateDbContext();
            contexto.Database.EnsureCreated();
        }

        public AppDbContext CreateDbContext() => new(_opciones);

        public void Dispose() => _conexion.Dispose();
    }
}
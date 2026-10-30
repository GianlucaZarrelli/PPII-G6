using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Acceso a datos falso que guarda todo en listas, para probar los ViewModels sin una base de datos.
    /// Solo implementa lo que las pruebas necesitan; el resto avisa si alguien lo usa sin querer.
    /// </summary>
    public class DatosEnMemoria : IDataService
    {
        /// <summary>
        /// Comisiones que "existen" en la base falsa.
        /// </summary>
        public List<Comision> Comisiones { get; } = new();

        /// <summary>
        /// Alumnos que "existen" en la base falsa.
        /// </summary>
        public List<Estudiante> Estudiantes { get; } = new();

        /// <summary>
        /// Clases de todas las comisiones de la base falsa.
        /// </summary>
        public List<Clase> Clases { get; } = new();

        /// <summary>
        /// Trabajos prácticos de todas las comisiones de la base falsa.
        /// </summary>
        public List<TrabajoPractico> Trabajos { get; } = new();

        /// <summary>
        /// Asistencias registradas en la base falsa.
        /// </summary>
        public List<Asistencia> Asistencias { get; } = new();

        /// <summary>
        /// Entregas registradas en la base falsa.
        /// </summary>
        public List<Entrega> Entregas { get; } = new();

        /// <summary>
        /// Contactos guardados en la base falsa.
        /// </summary>
        public List<Contacto> Contactos { get; } = new();

        /// <summary>
        /// Si es true, guardar un contacto falla, como si la base estuviera caída.
        /// </summary>
        public bool FallarAlGuardarContacto { get; set; }

        /// <summary>
        /// Agrega un alumno dado de alta antes de las clases, para que todas le cuenten.
        /// </summary>
        public Estudiante AgregarAlumno(int id, string nombre, string apellido, int comisionId)
        {
            var alumno = new Estudiante(id, $"9000000{id}", nombre, apellido, "", comisionId)
            {
                FechaAlta = new DateTime(2026, 8, 1)
            };

            Estudiantes.Add(alumno);
            return alumno;
        }

        public Task<List<Comision>> GetComisionesAsync() => Task.FromResult(Comisiones.ToList());

        public Task<Comision?> GetComisionByIdAsync(int id) => Task.FromResult(Comisiones.FirstOrDefault(c => c.Id == id));

        public Task<Estudiante?> GetEstudianteByIdAsync(int id) => Task.FromResult(Estudiantes.FirstOrDefault(e => e.Id == id));

        public Task<List<Estudiante>> GetEstudiantesByComisionIdAsync(int comisionId) =>
            Task.FromResult(Estudiantes.Where(e => e.ComisionId == comisionId).ToList());

        public Task<List<Clase>> GetClasesByComisionIdAsync(int comisionId) =>
            Task.FromResult(Clases.Where(c => c.ComisionId == comisionId).OrderByDescending(c => c.Fecha).ToList());

        public Task<List<TrabajoPractico>> GetTrabajosPracticosByComisionIdAsync(int comisionId) =>
            Task.FromResult(Trabajos.Where(t => t.ComisionId == comisionId).ToList());

        public Task<List<Asistencia>> GetTodasLasAsistenciasAsync() => Task.FromResult(Asistencias.ToList());

        public Task<List<Asistencia>> GetAsistenciasPorEstudianteAsync(int estudianteId) =>
            Task.FromResult(Asistencias.Where(a => a.EstudianteId == estudianteId).ToList());

        public Task<List<Entrega>> GetEntregasPorEstudianteAsync(int estudianteId) =>
            Task.FromResult(Entregas.Where(e => e.EstudianteId == estudianteId).ToList());

        public Task<int> SaveContactoAsync(Contacto contacto)
        {
            if (FallarAlGuardarContacto)
                throw new InvalidOperationException("La base de datos no responde.");

            contacto.Id = Contactos.Count + 1;
            Contactos.Add(contacto);
            return Task.FromResult(contacto.Id);
        }

        public Task<List<Contacto>> GetContactosPorEstudianteAsync(int estudianteId) =>
            Task.FromResult(Contactos
                .Where(c => c.EstudianteId == estudianteId)
                .OrderByDescending(c => c.FechaHora)
                .ToList());

        public Task<List<Contacto>> GetTodosLosContactosAsync() =>
            Task.FromResult(Contactos.OrderByDescending(c => c.FechaHora).ToList());

        // Lo que sigue no se usa en estas pruebas.
        public Task<List<Estudiante>> GetEstudiantesAsync() => throw new NotImplementedException();
        public Task<int> SaveEstudianteAsync(Estudiante estudiante) => throw new NotImplementedException();
        public Task SaveEstudiantesMassiveAsync(List<Estudiante> estudiantes) => throw new NotImplementedException();
        public Task<List<Clase>> GetClasesAsync() => throw new NotImplementedException();
        public Task<int> SaveClaseAsync(Clase clase) => throw new NotImplementedException();
        public Task<int> SaveAsistenciasAsync(List<Asistencia> asistencias) => throw new NotImplementedException();
        public Task<int> SaveAsistenciaAsync(Asistencia asistencia) => throw new NotImplementedException();
        public Task<Comision> GetOrCreateComisionAsync(string nombre) => throw new NotImplementedException();
        public Task<TrabajoPractico> SaveTrabajoPracticoAsync(TrabajoPractico tp) => throw new NotImplementedException();
        public Task SaveEntregasAsync(List<Entrega> entregas) => throw new NotImplementedException();
    }

    /// <summary>
    /// Servicio de avisos falso: en vez de abrir una ventana, anota los mensajes para poder revisarlos.
    /// </summary>
    public class DialogoFalso : IDialogoService
    {
        /// <summary>
        /// Avisos mostrados, en orden, como "Título: mensaje".
        /// </summary>
        public List<string> Avisos { get; } = new();

        public Task AvisarAsync(string titulo, string mensaje)
        {
            Avisos.Add($"{titulo}: {mensaje}");
            return Task.CompletedTask;
        }
    }
}
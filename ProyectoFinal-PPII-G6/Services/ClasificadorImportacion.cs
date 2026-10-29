using System;
using System.Collections.Generic;
using System.Linq;
using ProyectoFinal_PPII_G6.Models;

namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Qué se va a hacer con cada fila de un archivo de importación.
    /// </summary>
    public enum EstadoFila
    {
        Nuevo,
        YaExiste,
        RepetidoEnArchivo,
        DatosIncompletos,
        DniInvalido,
        DatoDemasiadoLargo,
        EmailInvalido
    }

    /// <summary>
    /// Una fila del archivo ya limpia (sin espacios de más y con el DNI normalizado) y lo que se hará con ella.
    /// </summary>
    public sealed record FilaClasificada(string Dni, string Nombre, string Apellido, string Email, EstadoFila Estado)
    {
        /// <summary>
        /// Solo se guardan las filas nuevas y válidas.
        /// </summary>
        public bool SeImporta => Estado == EstadoFila.Nuevo;
    }

    /// <summary>
    /// Las filas clasificadas de un archivo, con los totales que se muestran en pantalla.
    /// </summary>
    public sealed class ResultadoClasificacion
    {
        /// <summary>
        /// Crea el resultado a partir de las filas ya clasificadas.
        /// </summary>
        public ResultadoClasificacion(IReadOnlyList<FilaClasificada> filas)
        {
            Filas = filas;
        }

        /// <summary>
        /// Las filas del archivo, en el mismo orden.
        /// </summary>
        public IReadOnlyList<FilaClasificada> Filas { get; }

        /// <summary>
        /// Filas que se van a importar.
        /// </summary>
        public int Nuevos => Contar(EstadoFila.Nuevo);

        /// <summary>
        /// Filas cuyo DNI ya está en la comisión.
        /// </summary>
        public int YaExisten => Contar(EstadoFila.YaExiste);

        /// <summary>
        /// Filas cuyo DNI ya apareció antes en el mismo archivo.
        /// </summary>
        public int Repetidos => Contar(EstadoFila.RepetidoEnArchivo);

        /// <summary>
        /// Filas con datos faltantes, mal formados o demasiado largos.
        /// </summary>
        public int ConErrores => Filas.Count(f =>
            f.Estado is EstadoFila.DatosIncompletos or EstadoFila.DniInvalido
                or EstadoFila.DatoDemasiadoLargo or EstadoFila.EmailInvalido);

        private int Contar(EstadoFila estado) => Filas.Count(f => f.Estado == estado);
    }

    /// <summary>
    /// Decide qué se hace con cada fila de un archivo de importación. No toca la base ni la pantalla:
    /// recibe lo leído y los DNI que la comisión ya tiene, y por eso se prueba sola.
    /// </summary>
    public static class ClasificadorImportacion
    {
        /// <summary>
        /// Clasifica cada fila del archivo, en orden. Una fila rechazada no cuenta como "ya vista":
        /// si después viene otra con el mismo DNI y datos correctos, esa sí es nueva.
        /// </summary>
        public static ResultadoClasificacion Clasificar(IEnumerable<Estudiante> leidos, IEnumerable<string> dnisExistentes)
        {
            var existentes = dnisExistentes.Select(ValidacionDni.Normalizar).ToHashSet();
            var vistos = new HashSet<string>();
            var filas = new List<FilaClasificada>();

            foreach (var alumno in leidos)
            {
                var fila = Limpiar(alumno);
                filas.Add(fila with { Estado = ElegirEstado(fila, existentes, vistos) });
            }

            return new ResultadoClasificacion(filas);
        }

        /// <summary>
        /// Deja los datos de la fila sin espacios de más y con el DNI normalizado. El estado es provisorio.
        /// </summary>
        private static FilaClasificada Limpiar(Estudiante alumno) => new(
            ValidacionDni.Normalizar(alumno.Dni),
            (alumno.Nombre ?? string.Empty).Trim(),
            (alumno.Apellido ?? string.Empty).Trim(),
            (alumno.Email ?? string.Empty).Trim(),
            EstadoFila.Nuevo);

        /// <summary>
        /// Primero se miran los datos de la fila sola; recién si están bien se compara con la comisión y con el resto del archivo.
        /// </summary>
        private static EstadoFila ElegirEstado(FilaClasificada fila, HashSet<string> existentes, HashSet<string> vistos)
        {
            var estado = EstadoPorDatos(fila);
            if (estado != EstadoFila.Nuevo) return estado;

            if (existentes.Contains(fila.Dni)) return EstadoFila.YaExiste;

            return vistos.Add(fila.Dni) ? EstadoFila.Nuevo : EstadoFila.RepetidoEnArchivo;
        }

        /// <summary>
        /// Valida los datos de la fila sin mirar a las demás. El orden es el mismo que en el alta manual.
        /// </summary>
        private static EstadoFila EstadoPorDatos(FilaClasificada fila)
        {
            if (fila.Dni.Length == 0 || fila.Nombre.Length == 0 || fila.Apellido.Length == 0 || fila.Email.Length == 0)
                return EstadoFila.DatosIncompletos;

            if (ValidacionDni.ObtenerError(fila.Dni) != null)
                return EstadoFila.DniInvalido;

            if (fila.Nombre.Length > LimitesAlumno.LargoNombre || fila.Apellido.Length > LimitesAlumno.LargoApellido)
                return EstadoFila.DatoDemasiadoLargo;

            return ValidacionEmail.EsValido(fila.Email) ? EstadoFila.Nuevo : EstadoFila.EmailInvalido;
        }
    }
}
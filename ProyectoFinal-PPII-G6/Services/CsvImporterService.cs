using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ProyectoFinal_PPII_G6.Models;

namespace ProyectoFinal_PPII_G6.Services
{
    /// <summary>
    /// Convierte archivos CSV en listas de estudiantes para la carga masiva.
    /// </summary>
    public class CsvImporterService
    {
        /// <summary>
        /// Lee un CSV (omitiendo la cabecera) con columnas DNI, nombre, apellido y email opcional, separadas por ';' o ',', y devuelve los estudiantes asignados a la comisión indicada. Ignora líneas vacías o con menos de tres columnas.
        /// Lee hasta que ReadLineAsync devuelve null: consultar EndOfStream bloquearía el hilo dentro de un método asíncrono.
        /// </summary>
        public async Task<List<Estudiante>> ParseCsvAsync(Stream csvStream, int comisionId)
        {
            var estudiantes = new List<Estudiante>();

            using var reader = new StreamReader(csvStream);
            await reader.ReadLineAsync(); // la primera línea es el encabezado

            string? linea;
            while ((linea = await reader.ReadLineAsync()) != null)
            {
                var alumno = ParsearLinea(linea, comisionId);

                if (alumno != null)
                    estudiantes.Add(alumno);
            }

            return estudiantes;
        }

        /// <summary>
        /// Convierte una línea del archivo en un estudiante. Devuelve null si la línea está vacía
        /// o tiene menos de tres columnas (DNI, nombre y apellido); el email es opcional.
        /// </summary>
        public static Estudiante? ParsearLinea(string? linea, int comisionId)
        {
            if (string.IsNullOrWhiteSpace(linea)) return null;

            var valores = linea.Split(';', ',');
            if (valores.Length < 3) return null;

            return new Estudiante
            {
                Dni = valores[0].Trim(),
                Nombre = valores[1].Trim(),
                Apellido = valores[2].Trim(),
                Email = valores.Length > 3 ? valores[3].Trim() : string.Empty,
                ComisionId = comisionId
            };
        }
    }
}
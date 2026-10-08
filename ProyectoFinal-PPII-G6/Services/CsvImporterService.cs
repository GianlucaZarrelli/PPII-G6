using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ProyectoFinal_PPII_G6.Models;

namespace ProyectoFinal_PPII_G6.Services
{
    public class CsvImporterService
    {
        public async Task<List<Estudiante>> ParseCsvAsync(Stream csvStream, int comisionId)
        {
            var estudiantes = new List<Estudiante>();

            using var reader = new StreamReader(csvStream);

            await reader.ReadLineAsync();

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(line)) continue;

                var values = line.Split(';', ',');

                if (values.Length < 3) continue;
                
                var alumno = new Estudiante
                {
                    Dni = values[0].Trim(),
                    Nombre = values[1].Trim(),
                    Apellido = values[2].Trim(),
                    Email = values.Length > 3 ? values[3].Trim() : string.Empty,
                    ComisionId = comisionId
                };

                estudiantes.Add(alumno);

            }

            return estudiantes;
        }
    }
}

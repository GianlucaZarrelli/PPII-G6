using System.Reflection;
using ProyectoFinal_PPII_G6.Infraestructura;
using ProyectoFinal_PPII_G6.Models;
using Xunit;

namespace ProyectoFinal_PPII_G6.Tests
{
    /// <summary>
    /// Vigila la regla de dependencias: el dominio (Models) y la lógica (Services) no pueden depender
    /// de Entity Framework ni de MAUI. Esas dependencias viven en Infraestructura y en Presentacion.
    /// </summary>
    public class DependenciasDelDominioTests
    {
        private static readonly string[] EspaciosProhibidos = { "Microsoft.EntityFrameworkCore", "Microsoft.Maui" };
        private static readonly Assembly Aplicacion = typeof(Estudiante).Assembly;

        [Theory]
        [InlineData("ProyectoFinal_PPII_G6.Models")]
        [InlineData("ProyectoFinal_PPII_G6.Services")]
        public void LaCapa_NoDependeDeEntityFrameworkNiDeMaui(string espacio)
        {
            // Arrange
            var tipos = ObtenerTipos(espacio);

            // Act: se revisan los tipos que cada clase usa en sus campos, propiedades, métodos y constructores
            var infracciones = tipos
                .SelectMany(tipo => ObtenerTiposUsados(tipo).Where(EsProhibido).Select(usado => $"{tipo.Name} usa {usado.FullName}"))
                .Distinct()
                .ToList();

            // Assert: tiene que haber tipos que revisar (si no, la prueba no probaría nada) y ninguna infracción
            Assert.True(tipos.Count > 0, $"No se encontraron tipos en {espacio}");
            Assert.Empty(infracciones);
        }

        [Fact]
        public void ElDetector_ReconoceUnaDependenciaDeEntityFramework()
        {
            // Act: AppDbContext sí depende de EF, así que el detector tiene que verlo
            var prohibidos = ObtenerTiposUsados(typeof(AppDbContext)).Where(EsProhibido).ToList();

            // Assert: protege contra una prueba que siempre pase porque el detector dejó de funcionar
            Assert.NotEmpty(prohibidos);
        }

        private static List<Type> ObtenerTipos(string espacio)
        {
            Type?[] todos;
            try
            {
                todos = Aplicacion.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                todos = ex.Types;
            }

            return todos.OfType<Type>().Where(t => t.Namespace == espacio).ToList();
        }

        private static IEnumerable<Type> ObtenerTiposUsados(Type tipo)
        {
            const BindingFlags todos = BindingFlags.Instance | BindingFlags.Static
                | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            var usados = new List<Type?> { tipo.BaseType };
            usados.AddRange(tipo.GetInterfaces());
            usados.AddRange(tipo.GetFields(todos).Select(f => f.FieldType));
            usados.AddRange(tipo.GetProperties(todos).Select(p => p.PropertyType));
            usados.AddRange(tipo.GetMethods(todos).SelectMany(m => m.GetParameters().Select(p => p.ParameterType).Append(m.ReturnType)));
            usados.AddRange(tipo.GetConstructors(todos).SelectMany(c => c.GetParameters().Select(p => p.ParameterType)));

            return usados.OfType<Type>().SelectMany(Aplanar);
        }

        /// <summary>
        /// Devuelve el tipo y, si es genérico o un arreglo, también los tipos que lleva adentro (List&lt;Estudiante&gt; incluye Estudiante).
        /// </summary>
        private static IEnumerable<Type> Aplanar(Type tipo)
        {
            yield return tipo;

            var interiores = tipo.HasElementType ? new[] { tipo.GetElementType()! } : tipo.GetGenericArguments();

            foreach (var interior in interiores)
            {
                foreach (var t in Aplanar(interior))
                    yield return t;
            }
        }

        private static bool EsProhibido(Type tipo) =>
            EspaciosProhibidos.Any(prohibido => tipo.Namespace?.StartsWith(prohibido) == true);
    }
}
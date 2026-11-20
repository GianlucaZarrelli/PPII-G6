using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Presentacion;
using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.ViewModels
{
    /// <summary>
    /// Una fila del panel: un alumno con su nivel de riesgo y el dato que lo justifica.
    /// </summary>
    public class AlumnoRiesgoItem
    {
        /// <summary>
        /// Id del alumno, para abrir su ficha al tocarlo.
        /// </summary>
        public int EstudianteId { get; set; }

        /// <summary>
        /// Nombre y apellido del alumno.
        /// </summary>
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Texto del nivel de riesgo.
        /// </summary>
        public string Nivel { get; set; } = string.Empty;

        /// <summary>
        /// Color del nivel, en hexadecimal.
        /// </summary>
        public string ColorNivel { get; set; } = "#757575";

        /// <summary>
        /// Fondo suave del chip de nivel.
        /// </summary>
        public string ColorFondoNivel { get; set; } = "#F5F5F5";

        /// <summary>
        /// Los números que justifican el nivel, en una sola línea (compatibilidad con pruebas).
        /// </summary>
        public string Detalle { get; set; } = string.Empty;

        /// <summary>
        /// Inasistencias en formato corto para la grilla.
        /// </summary>
        public string AsistenciaTexto { get; set; } = string.Empty;

        /// <summary>
        /// Entregas en formato corto para la grilla.
        /// </summary>
        public string EntregasTexto { get; set; } = string.Empty;

        /// <summary>
        /// Racha de ausencias para la grilla.
        /// </summary>
        public string RachaTexto { get; set; } = string.Empty;

        /// <summary>
        /// Color del texto de la racha.
        /// </summary>
        public string ColorRacha { get; set; } = "#757575";

        /// <summary>
        /// Fondo del chip de racha.
        /// </summary>
        public string ColorFondoRacha { get; set; } = "#F5F5F5";

        /// <summary>
        /// Resumen del contacto con el alumno: la fecha y hora del último, o "Sin contactar".
        /// </summary>
        public string ContactoTexto { get; set; } = string.Empty;

        /// <summary>
        /// Color del resumen del contacto, en hexadecimal.
        /// </summary>
        public string ContactoColor { get; set; } = "#757575";

        /// <summary>
        /// Comando que abre la ficha de este alumno al tocar la fila.
        /// </summary>
        public Command? AbrirFichaCommand { get; set; }
    }

    /// <summary>
    /// ViewModel del panel de seguimiento: muestra a los alumnos de una comisión ordenados por riesgo.
    /// </summary>
    public class PanelRiesgoViewModel : BindableObject
    {
        private readonly IDataService _dataService;
        private readonly ConsultaSeguimiento _consulta;
        private readonly SeleccionAlumnoService _seleccion;
        private readonly INavegacion _navegacion;

        /// <summary>
        /// Comisiones disponibles para elegir.
        /// </summary>
        public ObservableCollection<Comision> Comisiones { get; } = new();

        /// <summary>
        /// Alumnos de la comisión elegida, ya ordenados por riesgo.
        /// </summary>
        public ObservableCollection<AlumnoRiesgoItem> Alumnos { get; } = new();

        private Comision? _comisionSeleccionada;

        /// <summary>
        /// Comisión elegida; al cambiarla se recalcula el panel.
        /// </summary>
        public Comision? ComisionSeleccionada
        {
            get => _comisionSeleccionada;
            set
            {
                _comisionSeleccionada = value;
                OnPropertyChanged();
                _ = CargarAlumnosAsync();
            }
        }

        private int _cantAlto;
        private int _cantModerado;
        private int _cantAlDia;
        private int _cantSinDatos;

        /// <summary>Cantidad de alumnos en alto riesgo.</summary>
        public int CantAlto
        {
            get => _cantAlto;
            private set { _cantAlto = value; OnPropertyChanged(); }
        }

        /// <summary>Cantidad de alumnos en riesgo moderado.</summary>
        public int CantModerado
        {
            get => _cantModerado;
            private set { _cantModerado = value; OnPropertyChanged(); }
        }

        /// <summary>Cantidad de alumnos al día.</summary>
        public int CantAlDia
        {
            get => _cantAlDia;
            private set { _cantAlDia = value; OnPropertyChanged(); }
        }

        /// <summary>Cantidad de alumnos sin datos.</summary>
        public int CantSinDatos
        {
            get => _cantSinDatos;
            private set { _cantSinDatos = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Crea el ViewModel con el acceso a datos, la consulta que evalúa a los alumnos, el aviso de qué alumno abrir
        /// y la navegación.
        /// </summary>
        public PanelRiesgoViewModel(
            IDataService dataService, ConsultaSeguimiento consulta, SeleccionAlumnoService seleccion, INavegacion navegacion)
        {
            _dataService = dataService;
            _consulta = consulta;
            _seleccion = seleccion;
            _navegacion = navegacion;
        }

        /// <summary>
        /// Recarga las comisiones, conserva la selección anterior (o elige la primera) y recalcula el panel.
        /// </summary>
        public async Task CargarAsync()
        {
            try
            {
                var idPrevio = ComisionSeleccionada?.Id;
                var lista = await _dataService.GetComisionesAsync();

                RepoblarComisiones(lista);
                ComisionSeleccionada = Comisiones.FirstOrDefault(c => c.Id == idPrevio)
                                       ?? Comisiones.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
            }
        }

        /// <summary>
        /// Reemplaza el selector de comisiones por la lista recibida, ordenada por nombre.
        /// </summary>
        private void RepoblarComisiones(IEnumerable<Comision> lista)
        {
            Comisiones.Clear();
            foreach (var c in lista.OrderBy(c => c.Nombre))
            {
                Comisiones.Add(c);
            }
        }

        /// <summary>
        /// Pide los alumnos de la comisión elegida, ya evaluados y ordenados, y los muestra.
        /// </summary>
        private async Task CargarAlumnosAsync()
        {
            var comision = ComisionSeleccionada;
            Alumnos.Clear();
            ActualizarContadores(Array.Empty<ResumenAlumno>());
            if (comision == null) return;

            try
            {
                var resumenes = await _consulta.ObtenerComisionAsync(comision.Id);

                // Si mientras consultaba el usuario eligió otra comisión, este resultado ya no sirve
                if (comision != ComisionSeleccionada) return;

                MostrarAlumnos(resumenes);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
            }
        }

        /// <summary>
        /// Reemplaza la lista del panel por los resúmenes recibidos.
        /// </summary>
        private void MostrarAlumnos(IEnumerable<ResumenAlumno> resumenes)
        {
            var lista = resumenes.ToList();
            Alumnos.Clear();
            foreach (var resumen in lista)
            {
                Alumnos.Add(CrearItem(resumen));
            }

            ActualizarContadores(lista);
        }

        private void ActualizarContadores(IReadOnlyList<ResumenAlumno> resumenes)
        {
            CantAlto = resumenes.Count(r => r.Nivel == NivelSeguimiento.Rojo);
            CantModerado = resumenes.Count(r => r.Nivel == NivelSeguimiento.Amarillo);
            CantAlDia = resumenes.Count(r => r.Nivel == NivelSeguimiento.Verde);
            CantSinDatos = resumenes.Count(r => r.Nivel == NivelSeguimiento.SinDatos);
        }

        /// <summary>
        /// Anota qué alumno tiene que mostrar el Historial y va a esa pantalla.
        /// </summary>
        private async Task AbrirFichaAsync(int estudianteId)
        {
            _seleccion.Pedir(estudianteId);
            await _navegacion.IrAAsync(Rutas.Historial);
        }

        /// <summary>
        /// Traduce un resumen a la fila que muestra el panel.
        /// </summary>
        private AlumnoRiesgoItem CrearItem(ResumenAlumno resumen)
        {
            var (rachaTexto, colorRacha, fondoRacha) = PresentarRacha(resumen.Racha);

            return new AlumnoRiesgoItem
            {
                EstudianteId = resumen.Estudiante.Id,
                NombreCompleto = resumen.Estudiante.NombreCompleto,
                Nivel = PresentacionNivel.Texto(resumen.Nivel),
                ColorNivel = PresentacionNivel.Color(resumen.Nivel),
                ColorFondoNivel = FondoNivel(resumen.Nivel),
                AsistenciaTexto = TextosSeguimiento.AsistenciaGrilla(resumen),
                EntregasTexto = TextosSeguimiento.EntregasGrilla(resumen),
                RachaTexto = rachaTexto,
                ColorRacha = colorRacha,
                ColorFondoRacha = fondoRacha,
                Detalle = ArmarDetalle(resumen),
                ContactoTexto = PresentacionContacto.Texto(resumen),
                ContactoColor = PresentacionContacto.Color(resumen),
                AbrirFichaCommand = new Command(async () => await AbrirFichaAsync(resumen.Estudiante.Id))
            };
        }

        /// <summary>
        /// Junta el dato que justifica el riesgo con la racha de ausencias seguidas.
        /// </summary>
        private static string ArmarDetalle(ResumenAlumno resumen) =>
            $"{TextosSeguimiento.AsistenciaCorta(resumen)}  |  {TextosSeguimiento.EntregasCorta(resumen)}  |  {TextosSeguimiento.RachaCorta(resumen)}";

        private static string FondoNivel(NivelSeguimiento nivel) => nivel switch
        {
            NivelSeguimiento.Rojo => "#FFEBEE",
            NivelSeguimiento.Amarillo => "#FFF3E0",
            NivelSeguimiento.Verde => "#E8F5E9",
            _ => "#ECEFF1"
        };

        /// <summary>
        /// Textos cortos y parejos para el chip de racha del panel.
        /// </summary>
        private static (string Texto, string Color, string Fondo) PresentarRacha(ResultadoRacha racha) => racha.Estado switch
        {
            EstadoRacha.ConRacha => (
                racha.Cantidad == 1 ? "1 seguida" : $"{racha.Cantidad} seguidas",
                "#B71C1C",
                "#FFCDD2"),
            EstadoRacha.SinRacha => ("Sin racha", "#2E7D32", "#C8E6C9"),
            _ => ("Sin datos", "#546E7A", "#CFD8DC")
        };
    }
}

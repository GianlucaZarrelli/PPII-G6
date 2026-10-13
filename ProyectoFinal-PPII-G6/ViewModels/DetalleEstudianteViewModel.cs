using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using ProyectoFinal_PPII_G6.Models;
using ProyectoFinal_PPII_G6.Services;

namespace ProyectoFinal_PPII_G6.ViewModels
{
    /// <summary>
    /// Una fila del historial de asistencia: una clase y el estado del alumno en ella.
    /// </summary>
    public class HistorialAsistenciaItem
    {
        /// <summary>
        /// Fecha de la clase con formato dd/MM/yyyy.
        /// </summary>
        public string Fecha { get; set; } = string.Empty;
        /// <summary>
        /// Texto del estado de asistencia (presente, ausente, justificada o sin registro).
        /// </summary>
        public string Estado { get; set; } = string.Empty;
        /// <summary>
        /// Color hexadecimal con el que se muestra el estado.
        /// </summary>
        public string Color { get; set; } = "#757575";
    }

    /// <summary>
    /// Una fila del historial de entregas: un trabajo práctico y si el alumno lo entregó.
    /// </summary>
    public class HistorialEntregaItem
    {
        /// <summary>
        /// Título del trabajo práctico.
        /// </summary>
        public string Titulo { get; set; } = string.Empty;
        /// <summary>
        /// Texto con la fecha de entrega del trabajo, listo para mostrar.
        /// </summary>
        public string FechaEntrega { get; set; } = string.Empty;
        /// <summary>
        /// Texto del estado de la entrega ("Entregado" o "No entregado").
        /// </summary>
        public string Estado { get; set; } = string.Empty;
        /// <summary>
        /// Color hexadecimal con el que se muestra el estado.
        /// </summary>
        public string Color { get; set; } = "#757575";
    }

    /// <summary>
    /// ViewModel de la ficha del alumno: historial de asistencia y entregas, y nivel de riesgo (solo lectura).
    /// </summary>
    public class DetalleEstudianteViewModel : BindableObject
    {
        private readonly IDataService _dataService;
        private readonly RiesgoService _riesgoService;

        /// <summary>
        /// Mientras es true, los setters de las selecciones no disparan cargas (se usa al repoblar las listas).
        /// </summary>
        private bool _actualizando;

        /// <summary>
        /// Comisiones disponibles para elegir, ordenadas por nombre.
        /// </summary>
        public ObservableCollection<Comision> Comisiones { get; } = new();
        /// <summary>
        /// Alumnos de la comisión elegida, ordenados por apellido y nombre.
        /// </summary>
        public ObservableCollection<Estudiante> ListaEstudiantes { get; } = new();
        /// <summary>
        /// Historial de asistencia del alumno elegido, una fila por clase de su comisión.
        /// </summary>
        public ObservableCollection<HistorialAsistenciaItem> HistorialAsistencias { get; } = new();
        /// <summary>
        /// Historial de entregas del alumno elegido, una fila por trabajo práctico de su comisión.
        /// </summary>
        public ObservableCollection<HistorialEntregaItem> HistorialEntregas { get; } = new();

        private Comision? _comisionSeleccionada;

        /// <summary>
        /// Comisión elegida; al cambiar recarga la lista de alumnos (salvo durante una actualización interna).
        /// </summary>
        public Comision? ComisionSeleccionada
        {
            get => _comisionSeleccionada;
            set
            {
                _comisionSeleccionada = value;
                OnPropertyChanged();

                if (_actualizando) return;
                _ = CargarEstudiantesAsync();
            }
        }

        private Estudiante? _estudianteSeleccionado;

        /// <summary>
        /// Alumno elegido; al cambiar carga su ficha o la limpia si es null (salvo durante una actualización interna).
        /// </summary>
        public Estudiante? EstudianteSeleccionado
        {
            get => _estudianteSeleccionado;
            set
            {
                _estudianteSeleccionado = value;
                OnPropertyChanged();

                if (_actualizando) return;

                if (value != null)
                    _ = CargarDetalleAsync(value);
                else
                    LimpiarDetalle();
            }
        }

        private string _comisionNombre = string.Empty;

        /// <summary>
        /// Nombre de la comisión del alumno elegido.
        /// </summary>
        public string ComisionNombre
        {
            get => _comisionNombre;
            set { _comisionNombre = value; OnPropertyChanged(); }
        }

        private string _nivelRiesgoTexto = "Seleccione un alumno";

        /// <summary>
        /// Texto del nivel de riesgo del alumno; también se usa para mensajes de estado o error.
        /// </summary>
        public string NivelRiesgoTexto
        {
            get => _nivelRiesgoTexto;
            set { _nivelRiesgoTexto = value; OnPropertyChanged(); }
        }

        private string _colorRiesgo = "#757575";

        /// <summary>
        /// Color hexadecimal asociado al nivel de riesgo.
        /// </summary>
        public string ColorRiesgo
        {
            get => _colorRiesgo;
            set { _colorRiesgo = value; OnPropertyChanged(); }
        }

        private string _detalleAsistencia = string.Empty;

        /// <summary>
        /// Resumen de asistencia del alumno (presentes, justificadas y porcentaje).
        /// </summary>
        public string DetalleAsistencia
        {
            get => _detalleAsistencia;
            set { _detalleAsistencia = value; OnPropertyChanged(); }
        }

        private string _detalleEntregas = string.Empty;

        /// <summary>
        /// Resumen de entregas del alumno (entregados sobre total y porcentaje).
        /// </summary>
        public string DetalleEntregas
        {
            get => _detalleEntregas;
            set { _detalleEntregas = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Crea el ViewModel con el servicio de datos y el servicio de cálculo de riesgo.
        /// </summary>
        public DetalleEstudianteViewModel(IDataService dataService, RiesgoService riesgoService)
        {
            _dataService = dataService;
            _riesgoService = riesgoService;
        }

        /// <summary>
        /// Recarga las comisiones y los alumnos, y conserva la comisión y el alumno elegidos
        /// (para que la ficha se refresque al volver a la pestaña).
        /// </summary>
        public async Task CargarListaEstudiantesAsync()
        {
            try
            {
                var idComision = ComisionSeleccionada?.Id;
                var idEstudiante = EstudianteSeleccionado?.Id;

                var comisiones = await _dataService.GetComisionesAsync();

                _actualizando = true;
                try
                {
                    Comisiones.Clear();
                    foreach (var c in comisiones.OrderBy(c => c.Nombre))
                    {
                        Comisiones.Add(c);
                    }

                    ComisionSeleccionada = Comisiones.FirstOrDefault(c => c.Id == idComision)
                                           ?? Comisiones.FirstOrDefault();
                }
                finally
                {
                    _actualizando = false;
                }

                await CargarEstudiantesAsync(idEstudiante);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                NivelRiesgoTexto = "No se pudieron cargar las comisiones";
            }
        }

        /// <summary>
        /// Carga los alumnos de la comisión elegida. Si se pasa un Id y sigue en la lista, lo deja seleccionado.
        /// </summary>
        private async Task CargarEstudiantesAsync(int? seleccionarId = null)
        {
            var comision = ComisionSeleccionada;

            _actualizando = true;
            try
            {
                ListaEstudiantes.Clear();
                EstudianteSeleccionado = null;
            }
            finally
            {
                _actualizando = false;
            }

            LimpiarDetalle();

            if (comision == null) return;

            try
            {
                var estudiantes = await _dataService.GetEstudiantesByComisionIdAsync(comision.Id);

                // Si mientras consultaba cambió la comisión, este resultado ya no sirve
                if (comision != ComisionSeleccionada) return;

                _actualizando = true;
                try
                {
                    foreach (var est in estudiantes.OrderBy(e => e.Apellido).ThenBy(e => e.Nombre))
                    {
                        ListaEstudiantes.Add(est);
                    }

                    EstudianteSeleccionado = ListaEstudiantes.FirstOrDefault(e => e.Id == seleccionarId);
                }
                finally
                {
                    _actualizando = false;
                }

                if (ListaEstudiantes.Count == 0)
                {
                    NivelRiesgoTexto = "La comisión no tiene alumnos cargados";
                    return;
                }

                if (EstudianteSeleccionado != null)
                {
                    await CargarDetalleAsync(EstudianteSeleccionado);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                NivelRiesgoTexto = "No se pudo cargar la lista de alumnos";
            }
        }

        /// <summary>
        /// Vacía los historiales y resúmenes y vuelve el riesgo a su estado inicial.
        /// </summary>
        private void LimpiarDetalle()
        {
            HistorialAsistencias.Clear();
            HistorialEntregas.Clear();
            ComisionNombre = string.Empty;
            DetalleAsistencia = string.Empty;
            DetalleEntregas = string.Empty;
            NivelRiesgoTexto = "Seleccione un alumno";
            ColorRiesgo = "#757575";
        }

        /// <summary>
        /// Carga el historial del alumno y calcula su riesgo con las clases y trabajos de SU comisión.
        /// </summary>
        public async Task CargarDetalleAsync(Estudiante estudiante)
        {
            try
            {
                var comision = await _dataService.GetComisionByIdAsync(estudiante.ComisionId);
                var clases = await _dataService.GetClasesByComisionIdAsync(estudiante.ComisionId);
                var asistenciasAlumno = await _dataService.GetAsistenciasPorEstudianteAsync(estudiante.Id);
                var tps = await _dataService.GetTrabajosPracticosByComisionIdAsync(estudiante.ComisionId);
                var entregasAlumno = await _dataService.GetEntregasPorEstudianteAsync(estudiante.Id);

                // Si mientras consultaba el usuario eligió otro alumno, este resultado ya no sirve
                if (EstudianteSeleccionado?.Id != estudiante.Id) return;

                // Solo cuenta lo que pertenece a la comisión del alumno
                var idsClases = clases.Select(c => c.Id).ToHashSet();
                var idsTps = tps.Select(t => t.Id).ToHashSet();
                var asistencias = asistenciasAlumno.Where(a => idsClases.Contains(a.ClaseId)).ToList();
                var entregas = entregasAlumno.Where(e => idsTps.Contains(e.TrabajoPracticoId)).ToList();

                ComisionNombre = comision?.Nombre ?? "-";

                HistorialAsistencias.Clear();
                foreach (var clase in clases) // ya vienen de la más reciente a la más antigua
                {
                    var asis = asistencias.FirstOrDefault(a => a.ClaseId == clase.Id);
                    HistorialAsistencias.Add(CrearItemAsistencia(clase, asis));
                }

                HistorialEntregas.Clear();
                foreach (var tp in tps.OrderBy(t => t.FechaEntrega))
                {
                    var entregado = entregas.Any(e => e.TrabajoPracticoId == tp.Id && e.Entregado);
                    HistorialEntregas.Add(new HistorialEntregaItem
                    {
                        Titulo = tp.Titulo,
                        FechaEntrega = $"Fecha de entrega: {tp.FechaEntrega:dd/MM/yyyy}",
                        Estado = entregado ? "Entregado" : "No entregado",
                        Color = entregado ? "#388E3C" : "#D32F2F"
                    });
                }

                CalcularNivelRiesgo(asistencias, clases.Count, entregas, tps.Count);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                NivelRiesgoTexto = "No se pudo cargar el historial";
                ColorRiesgo = "#757575";
            }
        }

        /// <summary>
        /// Arma la fila del historial para una clase según la asistencia del alumno (sin registro cuenta como ausente).
        /// </summary>
        private static HistorialAsistenciaItem CrearItemAsistencia(Clase clase, Asistencia? asis)
        {
            var item = new HistorialAsistenciaItem { Fecha = clase.Fecha.ToString("dd/MM/yyyy") };

            if (asis == null)
            {
                item.Estado = "Sin registro (cuenta como ausente)";
                item.Color = "#F57C00";
            }
            else if (asis.Presente)
            {
                item.Estado = "Presente";
                item.Color = "#388E3C";
            }
            else if (asis.Justificada)
            {
                item.Estado = "Ausente (justificada)";
                item.Color = "#F57C00";
            }
            else
            {
                item.Estado = "Ausente";
                item.Color = "#D32F2F";
            }

            return item;
        }

        /// <summary>
        /// Calcula el riesgo con <see cref="RiesgoService"/> y actualiza los textos de resumen, el nivel y su color.
        /// </summary>
        private void CalcularNivelRiesgo(List<Asistencia> asistencias, int totalClases, List<Entrega> entregas, int totalTps)
        {
            if (totalClases == 0 && totalTps == 0)
            {
                NivelRiesgoTexto = "Sin clases ni trabajos registrados";
                ColorRiesgo = "#757575";
                DetalleAsistencia = string.Empty;
                DetalleEntregas = string.Empty;
                return;
            }

            var resultado = _riesgoService.CalcularRiesgo(asistencias, totalClases, entregas, totalTps);

            int presentes = asistencias.Count(a => a.Presente);
            int justificadas = asistencias.Count(a => !a.Presente && a.Justificada);
            int entregados = entregas.Count(e => e.Entregado);

            DetalleAsistencia = totalClases > 0
                ? (justificadas > 0
                    ? $"Asistencia: {presentes} presentes + {justificadas} justificadas (valen la mitad) de {totalClases} clases ({resultado.PorcentajeAsistencia:F0}%)"
                    : $"Asistencia: {presentes} de {totalClases} clases ({resultado.PorcentajeAsistencia:F0}%)")
                : "Asistencia: sin clases registradas";

            DetalleEntregas = totalTps > 0
                ? $"Entregas: {entregados} de {totalTps} trabajos ({resultado.PorcentajeEntregas:F0}%)"
                : "Entregas: sin trabajos cargados";

            switch (resultado.Nivel)
            {
                case NivelRiesgoEnum.Rojo:
                    NivelRiesgoTexto = "ALTO RIESGO";
                    ColorRiesgo = "#D32F2F";
                    break;
                case NivelRiesgoEnum.Amarillo:
                    NivelRiesgoTexto = "RIESGO MODERADO";
                    ColorRiesgo = "#F57C00";
                    break;
                default:
                    NivelRiesgoTexto = "BAJO RIESGO / AL DÍA";
                    ColorRiesgo = "#388E3C";
                    break;
            }
        }
    }
}
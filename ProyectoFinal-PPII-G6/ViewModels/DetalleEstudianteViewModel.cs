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
        public string Fecha { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Color { get; set; } = "#757575";
    }

    /// <summary>
    /// Una fila del historial de entregas: un trabajo práctico y si el alumno lo entregó.
    /// </summary>
    public class HistorialEntregaItem
    {
        public string Titulo { get; set; } = string.Empty;
        public string FechaEntrega { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Color { get; set; } = "#757575";
    }

    /// <summary>
    /// ViewModel de la ficha del alumno: historial de asistencia y entregas, y nivel de riesgo (solo lectura).
    /// </summary>
    public class DetalleEstudianteViewModel : BindableObject
    {
        private readonly IDataService _dataService;
        private readonly RiesgoService _riesgoService;

        public ObservableCollection<Estudiante> ListaEstudiantes { get; } = new();
        public ObservableCollection<HistorialAsistenciaItem> HistorialAsistencias { get; } = new();
        public ObservableCollection<HistorialEntregaItem> HistorialEntregas { get; } = new();

        private Estudiante? _estudianteSeleccionado;

        public Estudiante? EstudianteSeleccionado
        {
            get => _estudianteSeleccionado;
            set
            {
                _estudianteSeleccionado = value;
                OnPropertyChanged();

                if (value != null)
                    _ = CargarDetalleAsync(value);
                else
                    LimpiarDetalle();
            }
        }

        private string _comisionNombre = string.Empty;

        public string ComisionNombre
        {
            get => _comisionNombre;
            set { _comisionNombre = value; OnPropertyChanged(); }
        }

        private string _nivelRiesgoTexto = "Seleccione un alumno";

        public string NivelRiesgoTexto
        {
            get => _nivelRiesgoTexto;
            set { _nivelRiesgoTexto = value; OnPropertyChanged(); }
        }

        private string _colorRiesgo = "#757575";

        public string ColorRiesgo
        {
            get => _colorRiesgo;
            set { _colorRiesgo = value; OnPropertyChanged(); }
        }

        private string _detalleAsistencia = string.Empty;

        public string DetalleAsistencia
        {
            get => _detalleAsistencia;
            set { _detalleAsistencia = value; OnPropertyChanged(); }
        }

        private string _detalleEntregas = string.Empty;

        public string DetalleEntregas
        {
            get => _detalleEntregas;
            set { _detalleEntregas = value; OnPropertyChanged(); }
        }

        public DetalleEstudianteViewModel(IDataService dataService, RiesgoService riesgoService)
        {
            _dataService = dataService;
            _riesgoService = riesgoService;
        }

        /// <summary>
        /// Recarga la lista de alumnos y conserva el seleccionado (para que se refresque al volver a la pestaña).
        /// </summary>
        public async Task CargarListaEstudiantesAsync()
        {
            try
            {
                var idPrevio = EstudianteSeleccionado?.Id;
                var estudiantes = await _dataService.GetEstudiantesAsync();

                ListaEstudiantes.Clear();
                foreach (var est in estudiantes.OrderBy(e => e.Apellido).ThenBy(e => e.Nombre))
                {
                    ListaEstudiantes.Add(est);
                }

                EstudianteSeleccionado = ListaEstudiantes.FirstOrDefault(e => e.Id == idPrevio);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                NivelRiesgoTexto = "No se pudo cargar la lista de alumnos";
            }
        }

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
            int entregados = entregas.Count(e => e.Entregado);

            DetalleAsistencia = totalClases > 0
                ? $"Asistencia: {presentes} de {totalClases} clases ({resultado.PorcentajeAsistencia:F0}%)"
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
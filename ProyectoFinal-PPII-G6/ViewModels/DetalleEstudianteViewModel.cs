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
    /// Una fila del historial de asistencia: una clase y el estado del alumno en ella.
    /// </summary>
    public class HistorialAsistenciaItem
    {
        /// <summary>
        /// Fecha de la clase.
        /// </summary>
        public string Fecha { get; set; } = string.Empty;

        /// <summary>
        /// Tema o descripción de la clase.
        /// </summary>
        public string Tema { get; set; } = string.Empty;

        /// <summary>
        /// Estado del alumno en esa clase.
        /// </summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>
        /// Color del estado, en hexadecimal.
        /// </summary>
        public string Color { get; set; } = "#757575";
    }

    /// <summary>
    /// Una fila del historial de entregas: un trabajo práctico y si el alumno lo entregó.
    /// </summary>
    public class HistorialEntregaItem
    {
        /// <summary>
        /// Título del trabajo.
        /// </summary>
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de entrega, en texto.
        /// </summary>
        public string FechaEntrega { get; set; } = string.Empty;

        /// <summary>
        /// Estado del alumno en ese trabajo.
        /// </summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>
        /// Color del estado, en hexadecimal.
        /// </summary>
        public string Color { get; set; } = "#757575";
    }

    /// <summary>
    /// ViewModel de la ficha del alumno: historial de asistencia y entregas, y nivel de riesgo (solo lectura).
    /// </summary>
    public class DetalleEstudianteViewModel : BindableObject
    {
        private readonly IDataService _dataService;
        private readonly ConsultaSeguimiento _consulta;
        private readonly SeleccionAlumnoService _seleccion;

        /// <summary>
        /// Mientras es true, los setters de las selecciones no disparan cargas (se usa al repoblar las listas).
        /// </summary>
        private bool _actualizando;

        /// <summary>
        /// Comisiones disponibles para elegir.
        /// </summary>
        public ObservableCollection<Comision> Comisiones { get; } = new();

        /// <summary>
        /// Alumnos de la comisión elegida.
        /// </summary>
        public ObservableCollection<Estudiante> ListaEstudiantes { get; } = new();

        /// <summary>
        /// Historial de clases del alumno elegido.
        /// </summary>
        public ObservableCollection<HistorialAsistenciaItem> HistorialAsistencias { get; } = new();

        /// <summary>
        /// Historial de trabajos del alumno elegido.
        /// </summary>
        public ObservableCollection<HistorialEntregaItem> HistorialEntregas { get; } = new();

        private Comision? _comisionSeleccionada;

        /// <summary>
        /// Comisión elegida; al cambiarla se recarga la lista de alumnos.
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
        /// Alumno elegido; al cambiarlo se carga su ficha.
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
        /// Texto del nivel de riesgo del alumno elegido.
        /// </summary>
        public string NivelRiesgoTexto
        {
            get => _nivelRiesgoTexto;
            set { _nivelRiesgoTexto = value; OnPropertyChanged(); }
        }

        private string _colorRiesgo = "#757575";

        /// <summary>
        /// Color del nivel de riesgo, en hexadecimal.
        /// </summary>
        public string ColorRiesgo
        {
            get => _colorRiesgo;
            set { _colorRiesgo = value; OnPropertyChanged(); }
        }

        private string _detalleAsistencia = string.Empty;

        /// <summary>
        /// Explicación de cómo se calculó el porcentaje de asistencia.
        /// </summary>
        public string DetalleAsistencia
        {
            get => _detalleAsistencia;
            set { _detalleAsistencia = value; OnPropertyChanged(); }
        }

        private string _detalleEntregas = string.Empty;

        /// <summary>
        /// Explicación de cómo se calculó el porcentaje de entregas.
        /// </summary>
        public string DetalleEntregas
        {
            get => _detalleEntregas;
            set { _detalleEntregas = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Crea el ViewModel con el acceso a datos, la consulta de seguimiento y el servicio que recuerda qué alumno abrir.
        /// </summary>
        public DetalleEstudianteViewModel(
            IDataService dataService,
            ConsultaSeguimiento consulta,
            SeleccionAlumnoService seleccion)
        {
            _dataService = dataService;
            _consulta = consulta;
            _seleccion = seleccion;
        }

        /// <summary>
        /// Recarga las comisiones y los alumnos, y conserva lo elegido (para que la ficha se refresque al volver
        /// a la pestaña). Si se llegó desde otra pantalla, abre el alumno que se pidió.
        /// </summary>
        public async Task CargarListaEstudiantesAsync()
        {
            try
            {
                var (idComision, idEstudiante) = await ResolverSeleccionInicialAsync();
                await RepoblarComisionesAsync(idComision);
                await CargarEstudiantesAsync(idEstudiante);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                NivelRiesgoTexto = "No se pudieron cargar las comisiones";
            }
        }

        /// <summary>
        /// Decide qué comisión y qué alumno mostrar: el pedido desde otra pantalla, o lo que ya estaba elegido.
        /// </summary>
        private async Task<(int? IdComision, int? IdEstudiante)> ResolverSeleccionInicialAsync()
        {
            var pendiente = _seleccion.TomarPendiente();
            if (pendiente != null)
            {
                var alumno = await _dataService.GetEstudianteByIdAsync(pendiente.Value);
                if (alumno != null) return (alumno.ComisionId, alumno.Id);
            }

            return (ComisionSeleccionada?.Id, EstudianteSeleccionado?.Id);
        }

        /// <summary>
        /// Vuelve a cargar el selector de comisiones y deja elegida la indicada (o la primera).
        /// </summary>
        private async Task RepoblarComisionesAsync(int? idComision)
        {
            var comisiones = await _dataService.GetComisionesAsync();

            EjecutarSinCargas(() =>
            {
                Comisiones.Clear();
                foreach (var c in comisiones.OrderBy(c => c.Nombre))
                {
                    Comisiones.Add(c);
                }

                ComisionSeleccionada = Comisiones.FirstOrDefault(c => c.Id == idComision)
                                       ?? Comisiones.FirstOrDefault();
            });
        }

        /// <summary>
        /// Carga los alumnos de la comisión elegida. Si se pasa un Id y sigue en la lista, lo deja seleccionado.
        /// Si mientras consultaba cambió la comisión, descarta el resultado porque ya no sirve.
        /// </summary>
        private async Task CargarEstudiantesAsync(int? seleccionarId = null)
        {
            var comision = ComisionSeleccionada;
            VaciarAlumnos();
            if (comision == null) return;

            try
            {
                var estudiantes = await _dataService.GetEstudiantesByComisionIdAsync(comision.Id);
                if (comision != ComisionSeleccionada) return;

                MostrarAlumnos(estudiantes, seleccionarId);
                await MostrarDetalleDelSeleccionadoAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                NivelRiesgoTexto = "No se pudo cargar la lista de alumnos";
            }
        }

        /// <summary>
        /// Vacía la lista de alumnos y la ficha.
        /// </summary>
        private void VaciarAlumnos()
        {
            EjecutarSinCargas(() =>
            {
                ListaEstudiantes.Clear();
                EstudianteSeleccionado = null;
            });

            LimpiarDetalle();
        }

        /// <summary>
        /// Llena la lista de alumnos ordenada por apellido y deja elegido el indicado, si está.
        /// </summary>
        private void MostrarAlumnos(IEnumerable<Estudiante> estudiantes, int? seleccionarId)
        {
            EjecutarSinCargas(() =>
            {
                foreach (var est in estudiantes.OrderBy(e => e.Apellido).ThenBy(e => e.Nombre))
                {
                    ListaEstudiantes.Add(est);
                }

                EstudianteSeleccionado = ListaEstudiantes.FirstOrDefault(e => e.Id == seleccionarId);
            });
        }

        /// <summary>
        /// Si hay un alumno elegido carga su ficha; si la comisión no tiene alumnos, lo avisa.
        /// </summary>
        private async Task MostrarDetalleDelSeleccionadoAsync()
        {
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

        /// <summary>
        /// Ejecuta un cambio de listas con los selectores "mudos": evita que cada cambio dispare una carga.
        /// </summary>
        private void EjecutarSinCargas(Action cambio)
        {
            _actualizando = true;
            try
            {
                cambio();
            }
            finally
            {
                _actualizando = false;
            }
        }

        /// <summary>
        /// Deja la ficha vacía, como antes de elegir un alumno.
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
        /// Pide la ficha del alumno y la muestra: datos, historial de clases y de trabajos, y nivel.
        /// Si mientras consultaba el usuario eligió otro alumno, descarta el resultado porque ya no sirve.
        /// </summary>
        public async Task CargarDetalleAsync(Estudiante estudiante)
        {
            try
            {
                var ficha = await _consulta.ObtenerFichaAsync(estudiante);
                if (EstudianteSeleccionado?.Id != estudiante.Id) return;

                ComisionNombre = ficha.ComisionNombre;
                MostrarHistorialAsistencia(ficha);
                MostrarHistorialEntregas(ficha);
                MostrarNivel(ficha.Resumen);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                NivelRiesgoTexto = "No se pudo cargar el historial";
                ColorRiesgo = "#757575";
            }
        }

        /// <summary>
        /// Arma el historial de clases, de la más reciente a la más antigua, marcando las que cuentan.
        /// </summary>
        private void MostrarHistorialAsistencia(FichaAlumno ficha)
        {
            HistorialAsistencias.Clear();

            foreach (var clase in ficha.Clases)
            {
                var asistencia = ficha.Asistencias.FirstOrDefault(a => a.ClaseId == clase.Id);
                var cuenta = ficha.Resumen.IdsClasesContadas.Contains(clase.Id);
                var (estado, color) = EstadoAsistencia(asistencia, cuenta);

                HistorialAsistencias.Add(new HistorialAsistenciaItem
                {
                    Fecha = clase.Fecha.ToString("dd/MM/yyyy"),
                    Tema = string.IsNullOrWhiteSpace(clase.Descripcion) ? "Sin descripción" : clase.Descripcion,
                    Estado = estado,
                    Color = color
                });
            }
        }

        /// <summary>
        /// Arma el historial de trabajos por fecha de entrega, marcando los que todavía no vencieron.
        /// </summary>
        private void MostrarHistorialEntregas(FichaAlumno ficha)
        {
            HistorialEntregas.Clear();

            foreach (var trabajo in ficha.Trabajos.OrderBy(t => t.FechaEntrega))
            {
                var entregado = ficha.Entregas.Any(e => e.TrabajoPracticoId == trabajo.Id && e.Entregado);
                var cuenta = ficha.Resumen.IdsTrabajosContados.Contains(trabajo.Id);
                var (estado, color) = EstadoEntrega(entregado, cuenta);

                HistorialEntregas.Add(new HistorialEntregaItem
                {
                    Titulo = trabajo.Titulo,
                    FechaEntrega = $"Fecha de entrega: {trabajo.FechaEntrega:dd/MM/yyyy}",
                    Estado = estado,
                    Color = color
                });
            }
        }

        /// <summary>
        /// Muestra el nivel del alumno y los textos que explican sus porcentajes.
        /// </summary>
        private void MostrarNivel(ResumenAlumno resumen)
        {
            NivelRiesgoTexto = PresentacionNivel.Texto(resumen.Nivel);
            ColorRiesgo = PresentacionNivel.Color(resumen.Nivel);
            DetalleAsistencia = TextosSeguimiento.AsistenciaLarga(resumen);
            DetalleEntregas = TextosSeguimiento.EntregasLarga(resumen);
        }

        /// <summary>
        /// Estado y color de una clase para el alumno. Una clase que no cuenta es anterior al alta y no tiene registro.
        /// </summary>
        private static (string Texto, string Color) EstadoAsistencia(Asistencia? asistencia, bool cuenta) =>
            (asistencia, cuenta) switch
            {
                (null, false) => ("Anterior al alta (no cuenta)", "#757575"),
                (null, true) => ("Sin registro (cuenta como ausente)", "#F57C00"),
                ({ Presente: true }, _) => ("Presente", "#388E3C"),
                ({ Justificada: true }, _) => ("Ausente (justificada)", "#F57C00"),
                _ => ("Ausente", "#D32F2F")
            };

        /// <summary>
        /// Estado y color de un trabajo para el alumno. Un trabajo que no cuenta es uno que todavía no venció.
        /// </summary>
        private static (string Texto, string Color) EstadoEntrega(bool entregado, bool cuenta) =>
            (entregado, cuenta) switch
            {
                (true, _) => ("Entregado", "#388E3C"),
                (false, true) => ("No entregado", "#D32F2F"),
                _ => ("Pendiente (aún no vence)", "#757575")
            };
    }
}
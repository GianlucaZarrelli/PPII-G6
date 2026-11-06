// Datos de la demo de la guía: comisiones A, B y C.
// 6 clases y 3 trabajos por comisión. Los porcentajes coinciden con la tabla.

const CSV_EJEMPLO = [
  "Dni;Nombre;Apellido;Email",
  "40100004;Lucía;Fernández;lucia.fernandez@demo.edu",
  "40400001;Ana;García;ana.garcia@demo.edu",
  "40400002;Pedro;López;pedro.lopez@demo.edu",
  "40400003;María;Suárez;maria.suarez@demo.edu",
  "40400004;Juan;Castro;juan.castro@demo.edu",
  "40400005;Elena;Vega;elena.vega@demo.edu",
  "40400006;Diego;Romero;diego.romero@demo.edu",
  "40400002;Pedro;López;pedro.otro@demo.edu",
  "40400007;;SinNombre;error@demo.edu",
].join("\n");

function crearDatosDemo() {
  const comisiones = [
    { id: 1, nombre: "A" },
    { id: 2, nombre: "B" },
    { id: 3, nombre: "C" },
  ];

  const estudiantes = [
    { id: 1, dni: "40100001", nombre: "Joaquín", apellido: "Pérez", email: "joaquin.perez@demo.edu", comisionId: 1 },
    { id: 2, dni: "40100002", nombre: "Valentina", apellido: "Díaz", email: "valentina.diaz@demo.edu", comisionId: 1 },
    { id: 3, dni: "40100003", nombre: "Mateo", apellido: "Gómez", email: "mateo.gomez@demo.edu", comisionId: 1 },
    { id: 4, dni: "40100004", nombre: "Lucía", apellido: "Fernández", email: "lucia.fernandez@demo.edu", comisionId: 1 },
    { id: 5, dni: "40100005", nombre: "Sofía", apellido: "Benítez", email: "sofia.benitez@demo.edu", comisionId: 1 },
    { id: 6, dni: "40200001", nombre: "Santiago", apellido: "Ruiz", email: "santiago.ruiz@demo.edu", comisionId: 2 },
    { id: 7, dni: "40200002", nombre: "Camila", apellido: "Torres", email: "camila.torres@demo.edu", comisionId: 2 },
    { id: 8, dni: "40200003", nombre: "Martina", apellido: "López", email: "martina.lopez@demo.edu", comisionId: 2 },
    { id: 9, dni: "40200004", nombre: "Tomás", apellido: "Silva", email: "tomas.silva@demo.edu", comisionId: 2 },
    { id: 10, dni: "40300001", nombre: "Facundo", apellido: "Medina", email: "facundo.medina@demo.edu", comisionId: 3 },
    { id: 11, dni: "40300002", nombre: "Julieta", apellido: "Herrera", email: "julieta.herrera@demo.edu", comisionId: 3 },
    { id: 12, dni: "40300003", nombre: "Nicolás", apellido: "Acosta", email: "nicolas.acosta@demo.edu", comisionId: 3 },
  ];

  const fechas = ["2026-09-04", "2026-09-11", "2026-09-18", "2026-09-25", "2026-10-02", "2026-10-09"];
  const temas = ["Presentación", "Variables", "Condicionales", "Funciones", "Arreglos", "Repaso"];
  const clases = [];
  for (const comision of comisiones) {
    fechas.forEach((fecha, indice) => {
      clases.push({
        id: comision.id * 100 + indice + 1,
        fecha,
        comisionId: comision.id,
        descripcion: temas[indice],
      });
    });
  }

  const titulos = [
    { titulo: "TP1", fechaEntrega: "2026-09-20" },
    { titulo: "TP2", fechaEntrega: "2026-10-04" },
    { titulo: "TP3", fechaEntrega: "2026-10-11" },
  ];
  const trabajos = [];
  for (const comision of comisiones) {
    titulos.forEach((trabajo, indice) => {
      trabajos.push({
        id: comision.id * 1000 + indice + 1,
        comisionId: comision.id,
        titulo: trabajo.titulo,
        fechaEntrega: trabajo.fechaEntrega,
      });
    });
  }

  // Índice de clase (0 a 5) en la que el alumno faltó, sin justificar.
  const faltas = {
    2: [4, 5],
    3: [5],
    6: [3, 4, 5],
    10: [4, 5],
    11: [5],
  };

  // Índice de trabajo (0 TP1, 1 TP2, 2 TP3) que sí entregó.
  const entregados = {
    1: [0],
    2: [0, 1, 2],
    3: [0, 1, 2],
    4: [0, 1, 2],
    5: [0, 1, 2],
    6: [],
    7: [0, 1],
    8: [0, 1, 2],
    9: [0, 1, 2],
    10: [0, 1],
    11: [0, 1],
    12: [0, 1, 2],
  };

  const asistencias = [];
  const entregas = [];
  let idAsistencia = 1;
  let idEntrega = 1;

  for (const alumno of estudiantes) {
    const clasesComision = clases.filter((c) => c.comisionId === alumno.comisionId);
    const faltasAlumno = faltas[alumno.id] || [];
    clasesComision.forEach((clase, indice) => {
      asistencias.push({
        id: idAsistencia++,
        estudianteId: alumno.id,
        claseId: clase.id,
        presente: !faltasAlumno.includes(indice),
        justificada: false,
      });
    });

    const trabajosComision = trabajos.filter((t) => t.comisionId === alumno.comisionId);
    const entrego = entregados[alumno.id] || [];
    trabajosComision.forEach((trabajo, indice) => {
      entregas.push({
        id: idEntrega++,
        estudianteId: alumno.id,
        trabajoPracticoId: trabajo.id,
        entregado: entrego.includes(indice),
      });
    });
  }

  // H7: umbrales por defecto. Se pueden cambiar desde la pantalla Umbrales.
  const umbrales = {
    asistenciaRojo: 75,
    asistenciaAmarillo: 85,
    entregasRojo: 60,
    entregasAmarillo: 80,
  };

  return {
    comisiones,
    estudiantes,
    clases,
    trabajos,
    asistencias,
    entregas,
    umbrales,
    siguienteId: 5000,
  };
}

if (typeof module !== "undefined" && module.exports) {
  module.exports = { crearDatosDemo, CSV_EJEMPLO };
}

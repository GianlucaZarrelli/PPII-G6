const { calcularRiesgo, resumirAlumno } = require("./riesgo");
const { esEmailValido } = require("./validacion");
const { crearDatosDemo } = require("./datos");

const casos = [];

function cerca(valor, esperado) {
  return Math.abs(valor - esperado) < 0.05;
}

function esperar(nombre, condicion) {
  casos.push({ nombre, ok: Boolean(condicion) });
}

const verde = calcularRiesgo({ presentes: 6, justificadas: 0, totalClases: 6, entregados: 3, totalTrabajos: 3 });
esperar("Cumplidor verde 100/100", verde.nivel === "verde" && verde.asistencia === 100 && verde.entregas === 100);

const medio = calcularRiesgo({ presentes: 4, justificadas: 0, totalClases: 5, entregados: 1, totalTrabajos: 1 });
esperar("Asistencia 80% amarillo", medio.nivel === "amarillo" && medio.asistencia === 80);

const baja = calcularRiesgo({ presentes: 7, justificadas: 0, totalClases: 10, entregados: 1, totalTrabajos: 1 });
esperar("Asistencia 70% rojo", baja.nivel === "rojo" && baja.asistencia === 70);

const vacio = calcularRiesgo({ presentes: 0, justificadas: 0, totalClases: 0, entregados: 0, totalTrabajos: 0 });
esperar("Sin clases ni trabajos verde 100", vacio.nivel === "verde" && vacio.asistencia === 100 && vacio.entregas === 100);

const justo75 = calcularRiesgo({ presentes: 3, justificadas: 0, totalClases: 4, entregados: 1, totalTrabajos: 1 });
esperar("Asistencia 75% amarillo", justo75.nivel === "amarillo" && justo75.asistencia === 75);

const justo85 = calcularRiesgo({ presentes: 17, justificadas: 0, totalClases: 20, entregados: 1, totalTrabajos: 1 });
esperar("Asistencia 85% verde", justo85.nivel === "verde" && justo85.asistencia === 85);

const entregaBaja = calcularRiesgo({ presentes: 6, justificadas: 0, totalClases: 6, entregados: 2, totalTrabajos: 5 });
esperar("Entregas 40% rojo", entregaBaja.nivel === "rojo" && entregaBaja.entregas === 40);

const mixto = calcularRiesgo({ presentes: 4, justificadas: 0, totalClases: 5, entregados: 2, totalTrabajos: 5 });
esperar("Amarillo y rojo: gana rojo", mixto.nivel === "rojo" && mixto.asistencia === 80 && mixto.entregas === 40);

const ausencia = calcularRiesgo({ presentes: 0, justificadas: 0, totalClases: 1, entregados: 1, totalTrabajos: 1 });
esperar("Ausencia no suma presente", ausencia.asistencia === 0 && ausencia.nivel === "rojo");

const injustificada = calcularRiesgo({ presentes: 5, justificadas: 0, totalClases: 6, entregados: 1, totalTrabajos: 1 });
const justificada = calcularRiesgo({ presentes: 5, justificadas: 1, totalClases: 6, entregados: 1, totalTrabajos: 1 });
esperar("Justificada 91,7 contra injustificada 83,3", cerca(injustificada.asistencia, 83.333) && cerca(justificada.asistencia, 91.666));

const dosJust = calcularRiesgo({ presentes: 0, justificadas: 2, totalClases: 2, entregados: 1, totalTrabajos: 1 });
const unPresente = calcularRiesgo({ presentes: 1, justificadas: 0, totalClases: 2, entregados: 1, totalTrabajos: 1 });
esperar("Dos justificadas equivalen a un presente", dosJust.asistencia === unPresente.asistencia && dosJust.asistencia === 50);

const enRojo = calcularRiesgo({ presentes: 7, justificadas: 0, totalClases: 10, entregados: 1, totalTrabajos: 1 });
const saleRojo = calcularRiesgo({ presentes: 7, justificadas: 1, totalClases: 10, entregados: 1, totalTrabajos: 1 });
esperar("Una justificada saca del rojo (70 a 75)", enRojo.nivel === "rojo" && enRojo.asistencia === 70 && saleRojo.nivel === "amarillo" && saleRojo.asistencia === 75);

const presenteMarcado = resumirAlumno(
  [{ id: 1 }],
  [{ claseId: 1, presente: true, justificada: true }],
  [{ id: 10 }],
  [{ trabajoPracticoId: 10, entregado: true }]
);
esperar("Presente justificado no suma de más", presenteMarcado.asistencia === 100 && presenteMarcado.presentes === 1);

const sinRegistro = resumirAlumno([{ id: 1 }, { id: 2 }], [], [{ id: 10 }], []);
esperar("Sin registro es falta y no entrega", sinRegistro.asistencia === 0 && sinRegistro.entregas === 0 && sinRegistro.nivel === "rojo");

const emailsOk = ["lucia.fernandez@demo.edu", "a@b.co", "  sofia@demo.edu  "];
emailsOk.forEach((email) => esperar("Email válido: " + JSON.stringify(email), esEmailValido(email)));

const emailsMal = [null, "", "   ", "sindominio", "@demo.edu", "sofia@", "sofia@demo", "Sofía sofia@demo.edu", "a@" + "b".repeat(98) + ".com"];
emailsMal.forEach((email) => esperar("Email inválido: " + JSON.stringify(email), !esEmailValido(email)));

const demo = crearDatosDemo();
const esperado = {
  "Joaquín Pérez": "rojo",
  "Valentina Díaz": "rojo",
  "Mateo Gómez": "amarillo",
  "Lucía Fernández": "verde",
  "Sofía Benítez": "verde",
  "Santiago Ruiz": "rojo",
  "Camila Torres": "amarillo",
  "Martina López": "verde",
  "Tomás Silva": "verde",
  "Facundo Medina": "rojo",
  "Julieta Herrera": "amarillo",
  "Nicolás Acosta": "verde",
};

for (const alumno of demo.estudiantes) {
  const clases = demo.clases.filter((c) => c.comisionId === alumno.comisionId);
  const trabajos = demo.trabajos.filter((t) => t.comisionId === alumno.comisionId);
  const asistencias = demo.asistencias.filter((a) => a.estudianteId === alumno.id);
  const entregas = demo.entregas.filter((e) => e.estudianteId === alumno.id);
  const riesgo = resumirAlumno(clases, asistencias, trabajos, entregas);
  const nombre = alumno.nombre + " " + alumno.apellido;
  esperar(nombre + " queda " + esperado[nombre], riesgo.nivel === esperado[nombre]);
}

const joaquin = demo.estudiantes[0];
const riesgoJoaquin = resumirAlumno(
  demo.clases.filter((c) => c.comisionId === 1),
  demo.asistencias.filter((a) => a.estudianteId === joaquin.id),
  demo.trabajos.filter((t) => t.comisionId === 1),
  demo.entregas.filter((e) => e.estudianteId === joaquin.id)
);
esperar("Joaquín 100% asistencia y 33% entregas", riesgoJoaquin.asistencia === 100 && cerca(riesgoJoaquin.entregas, 33.333));

const panelA = demo.estudiantes.filter((e) => e.comisionId === 1).map((alumno) => {
  const riesgo = resumirAlumno(
    demo.clases.filter((c) => c.comisionId === 1),
    demo.asistencias.filter((a) => a.estudianteId === alumno.id),
    demo.trabajos.filter((t) => t.comisionId === 1),
    demo.entregas.filter((e) => e.estudianteId === alumno.id)
  );
  return { nombre: alumno.nombre, orden: riesgo.orden, peor: riesgo.peor };
}).sort((a, b) => a.orden - b.orden || a.peor - b.peor);
esperar("En A, Joaquín queda antes que Valentina", panelA[0].nombre === "Joaquín" && panelA[1].nombre === "Valentina");

const fallos = casos.filter((c) => !c.ok);
console.log(casos.length + " pruebas, " + fallos.length + " fallos");
fallos.forEach((c) => console.log("FALLA: " + c.nombre));
if (fallos.length) process.exit(1);

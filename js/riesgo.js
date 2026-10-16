// Misma regla que Services/RiesgoService.cs → CalcularRiesgo.
// La llama resumirAlumno, y a resumirAlumno la llama riesgoDe() en js/app.js.
// El color no se guarda: se calcula cada vez que se abre una pantalla.
//
// Recibe números ya contados, no el alumno entero:
// presentes, justificadas, totalClases, entregados, totalTrabajos.
// Devuelve asistencia, entregas, nivel (rojo, amarillo o verde) y el texto del semáforo.

function calcularRiesgo({ presentes, justificadas, totalClases, entregados, totalTrabajos }) {
  const asistencia = totalClases === 0
    ? 100
    : ((presentes + 0.5 * justificadas) / totalClases) * 100;

  const entregas = totalTrabajos === 0
    ? 100
    : (entregados / totalTrabajos) * 100;

  let nivel = "verde";
  if (asistencia < 75 || entregas < 60) nivel = "rojo";
  else if (asistencia < 85 || entregas < 80) nivel = "amarillo";

  const textos = {
    rojo: "ALTO RIESGO",
    amarillo: "RIESGO MODERADO",
    verde: "AL DÍA",
  };

  return {
    asistencia,
    entregas,
    nivel,
    texto: textos[nivel],
    presentes,
    justificadas,
    totalClases,
    entregados,
    totalTrabajos,
    peor: Math.min(asistencia, entregas),
    orden: nivel === "rojo" ? 0 : nivel === "amarillo" ? 1 : 2,
  };
}

// La llama riesgoDe(alumno) en app.js.
// Recibe las clases y trabajos de la comisión de ese alumno, más sus asistencias y entregas.
// Arma los números de un alumno con las clases y trabajos de SU comisión.
// Sin registro de asistencia = ausencia. Sin registro de entrega = no entregó.
// Si está presente, una marca de justificada no suma de más.
function resumirAlumno(clases, asistencias, trabajos, entregas) {
  let presentes = 0;
  let justificadas = 0;

  for (const clase of clases) {
    const registro = asistencias.find((a) => a.claseId === clase.id);
    if (!registro) continue;
    if (registro.presente) presentes += 1;
    else if (registro.justificada) justificadas += 1;
  }

  let entregados = 0;
  for (const trabajo of trabajos) {
    const registro = entregas.find((e) => e.trabajoPracticoId === trabajo.id);
    if (registro && registro.entregado) entregados += 1;
  }

  return calcularRiesgo({
    presentes,
    justificadas,
    totalClases: clases.length,
    entregados,
    totalTrabajos: trabajos.length,
  });
}

function motivoRiesgo(riesgo) {
  if (riesgo.nivel === "verde") return "Asistencia y entregas en orden";

  const partes = [];
  const topeAsistencia = riesgo.nivel === "rojo" ? 75 : 85;
  const topeEntregas = riesgo.nivel === "rojo" ? 60 : 80;
  if (riesgo.asistencia < topeAsistencia) partes.push("la asistencia");
  if (riesgo.entregas < topeEntregas) partes.push("las entregas");
  if (partes.length === 0) return riesgo.texto;
  return "Lo marca " + partes.join(" y ");
}

if (typeof module !== "undefined" && module.exports) {
  module.exports = { calcularRiesgo, resumirAlumno, motivoRiesgo };
}

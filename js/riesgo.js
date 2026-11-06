// Misma regla que Services/RiesgoService.cs → CalcularRiesgo.
// H7: los umbrales ya no están fijos en el código. Vienen de db.umbrales
// (pantalla Umbrales). Si no se pasan, se usan estos valores por defecto.

const UMBRALES_POR_DEFECTO = {
  asistenciaRojo: 75,
  asistenciaAmarillo: 85,
  entregasRojo: 60,
  entregasAmarillo: 80,
};

function normalizarUmbrales(umbrales) {
  const base = { ...UMBRALES_POR_DEFECTO, ...(umbrales || {}) };
  return {
    asistenciaRojo: Number(base.asistenciaRojo),
    asistenciaAmarillo: Number(base.asistenciaAmarillo),
    entregasRojo: Number(base.entregasRojo),
    entregasAmarillo: Number(base.entregasAmarillo),
  };
}

// Recibe números ya contados y, opcionalmente, umbrales.
// Devuelve asistencia, entregas, nivel (rojo, amarillo o verde) y el texto del semáforo.
function calcularRiesgo({ presentes, justificadas, totalClases, entregados, totalTrabajos, umbrales }) {
  const u = normalizarUmbrales(umbrales);

  const asistencia = totalClases === 0
    ? 100
    : ((presentes + 0.5 * justificadas) / totalClases) * 100;

  const entregas = totalTrabajos === 0
    ? 100
    : (entregados / totalTrabajos) * 100;

  let nivel = "verde";
  if (asistencia < u.asistenciaRojo || entregas < u.entregasRojo) nivel = "rojo";
  else if (asistencia < u.asistenciaAmarillo || entregas < u.entregasAmarillo) nivel = "amarillo";

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
    umbrales: u,
    peor: Math.min(asistencia, entregas),
    orden: nivel === "rojo" ? 0 : nivel === "amarillo" ? 1 : 2,
  };
}

// La llama riesgoDe(alumno) en app.js.
// Recibe las clases y trabajos de la comisión de ese alumno, más sus asistencias y entregas.
// umbrales es opcional: si no viene, usa UMBRALES_POR_DEFECTO.
function resumirAlumno(clases, asistencias, trabajos, entregas, umbrales) {
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
    umbrales,
  });
}

function motivoRiesgo(riesgo) {
  if (riesgo.nivel === "verde") return "Asistencia y entregas en orden";

  const u = normalizarUmbrales(riesgo.umbrales);
  const partes = [];
  const topeAsistencia = riesgo.nivel === "rojo" ? u.asistenciaRojo : u.asistenciaAmarillo;
  const topeEntregas = riesgo.nivel === "rojo" ? u.entregasRojo : u.entregasAmarillo;
  if (riesgo.asistencia < topeAsistencia) partes.push("la asistencia");
  if (riesgo.entregas < topeEntregas) partes.push("las entregas");
  if (partes.length === 0) return riesgo.texto;
  return "Lo marca " + partes.join(" y ");
}

if (typeof module !== "undefined" && module.exports) {
  module.exports = { calcularRiesgo, resumirAlumno, motivoRiesgo, UMBRALES_POR_DEFECTO, normalizarUmbrales };
}

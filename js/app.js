// Versión web de la guía. Los datos viven en el navegador (localStorage), no en SQL Server.
//
// MAPA: dónde está cada cosa
// - Las pestañas (Inicio, Panel, Historial...) están en index.html, dentro de <nav>.
//   Cada botón tiene data-vista="inicio" (o panel, historial, asistencia, entregas, alumnos, csv).
// - El contenido de cada pestaña es un <section id="..."> en el mismo index.html.
//   El id tiene que ser IGUAL a data-vista. Si no coinciden, mostrar() no encuentra la pantalla.
// - Este archivo, app.js, es la lógica: lee el selector, llama a alumnosDe(comisionId) y dibuja.
// - La regla rojo/amarillo/verde está en js/riesgo.js (calcularRiesgo). app.js solo la llama.
//
// PARA AGREGAR UN BOTÓN DE PESTAÑA (ejemplo: "Contactos"):
// 1. En index.html, dentro de <nav>, un botón:
//      <button type="button" data-vista="contactos">Contactos</button>
// 2. En index.html, dentro de <main>, una sección con el MISMO nombre:
//      <section id="contactos" class="vista"> ...lo que se ve... </section>
// 3. Acá abajo, en mostrar(), una línea:
//      if (vista === "contactos") cargarContactos();
//    El click del nav ya está conectado al final de este archivo: no hace falta otro click
//    para las pestañas. mostrar() se entera sola por data-vista.
//
// PARA AGREGAR UN BOTÓN DENTRO de una pantalla (ejemplo: "Guardar" en Inicio):
// 1. El botón va en el <section> de esa pantalla, en index.html, con un id:
//      <button type="button" id="mi-boton">Guardar</button>
// 2. Al final de este archivo, donde dice "CONEXIÓN DE BOTONES", se engancha:
//      document.getElementById("mi-boton").addEventListener("click", miFuncion);
// 3. miFuncion se escribe arriba, junto a las otras funciones.

const CLAVE = "alerta-temprana-v1";

// Puente entre Panel e Historial. El Panel anota el id con pedir().
// Historial lo lee al abrirse. No viaja por la URL.
const seleccionAlumno = { id: null, pedir(id) { this.id = id; } };

let db = cargarDb();
let actualizando = false;
let panelToken = 0;
let historialToken = 0;
let filasCsv = [];

// Lee la base guardada en el navegador. Si no hay nada, usa los 12 alumnos de la demo.
function cargarDb() {
  const guardado = localStorage.getItem(CLAVE);
  if (!guardado) return crearDatosDemo();
  try { return JSON.parse(guardado); }
  catch { return crearDatosDemo(); }
}

// Escribe db de nuevo en el navegador. Hay que llamarla después de cada alta o cambio.
function guardarDb() {
  localStorage.setItem(CLAVE, JSON.stringify(db));
}

function nuevoId() {
  db.siguienteId += 1;
  return db.siguienteId;
}

function hoyIso() {
  const fecha = new Date();
  const mes = String(fecha.getMonth() + 1).padStart(2, "0");
  const dia = String(fecha.getDate()).padStart(2, "0");
  return fecha.getFullYear() + "-" + mes + "-" + dia;
}

function fmt(numero) {
  return (Math.round(numero * 10) / 10).toLocaleString("es-AR", {
    minimumFractionDigits: 0,
    maximumFractionDigits: 1,
  });
}

function esc(texto) {
  return String(texto ?? "").replace(/[&<>"']/g, (c) => ({
    "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;",
  }[c]));
}

function nombreCompleto(alumno) {
  return alumno.nombre + " " + alumno.apellido;
}

// comisionId es el número de la comisión elegida en un <select>.
// En el HTML el valor es "1", "2" o "3" (texto). Quien llama hace Number(...) y lo pasa acá.
// Estas tres funciones NO dibujan nada: solo filtran db y devuelven un array.
// Las llama el Panel, el Historial, Asistencia, Entregas, Alumnos y la carga CSV.

// Devuelve las clases de ESA comisión (no las de A si pediste B).
function clasesDe(comisionId) {
  return db.clases
    .filter((c) => c.comisionId === comisionId)
    .sort((a, b) => a.fecha.localeCompare(b.fecha));
}

// Devuelve los trabajos prácticos de ESA comisión.
function trabajosDe(comisionId) {
  return db.trabajos
    .filter((t) => t.comisionId === comisionId)
    .sort((a, b) => a.fechaEntrega.localeCompare(b.fechaEntrega));
}

// Devuelve los alumnos cuya comisión es comisionId, ordenados por apellido y nombre.
// Ejemplo: alumnosDe(1) devuelve Joaquín, Lucía, Mateo, Sofía y Valentina (la comisión A).
// No recibe el nombre "A": recibe el id 1. El select guarda el id en value="1".
function alumnosDe(comisionId) {
  return db.estudiantes
    .filter((e) => e.comisionId === comisionId)
    .sort((a, b) => nombreCompleto(a).localeCompare(nombreCompleto(b), "es"));
}

// Calcula el semáforo de un alumno usando las clases y trabajos de SU comisión.
// Llama a resumirAlumno, que está en js/riesgo.js.
function riesgoDe(alumno) {
  const clases = clasesDe(alumno.comisionId);
  const trabajos = trabajosDe(alumno.comisionId);
  const asistencias = db.asistencias.filter((a) => a.estudianteId === alumno.id);
  const entregas = db.entregas.filter((e) => e.estudianteId === alumno.id);
  return resumirAlumno(clases, asistencias, trabajos, entregas);
}

// Llena un <select> con Comisión A, B y C.
// select es el elemento del HTML (por ejemplo #panel-comision).
// elegida es el id que queda seleccionado, o null para no cambiarlo.
function llenarComisiones(select, elegida) {
  const valor = elegida ?? select.value;
  select.innerHTML = db.comisiones
    .map((c) => `<option value="${c.id}">Comisión ${esc(c.nombre)}</option>`)
    .join("");
  if (valor) select.value = valor;
}

function mostrarAviso(id, texto, esError) {
  const aviso = document.getElementById(id);
  aviso.hidden = !texto;
  aviso.textContent = texto || "";
  aviso.classList.toggle("error", Boolean(esError));
}

// Cambia de pestaña. vista es el texto de data-vista: "inicio", "panel", "historial", etc.
// 1. Oculta todas las secciones. 2. Marca el botón. 3. Muestra la sección con ese id.
// 4. Llama a la función que carga los datos de esa pantalla.
function mostrar(vista) {
  document.querySelectorAll(".vista").forEach((nodo) => nodo.classList.remove("activa"));
  document.querySelectorAll("nav button").forEach((boton) => {
    boton.classList.toggle("activa", boton.dataset.vista === vista);
  });
  document.getElementById(vista).classList.add("activa");
  if (location.hash !== "#" + vista) history.replaceState(null, "", "#" + vista);
  if (vista === "inicio") cargarMetricas();
  if (vista === "panel") cargarPanelAsync();
  if (vista === "historial") prepararHistorial();
  if (vista === "asistencia") cargarAsistencia();
  if (vista === "entregas") cargarEntregas();
  if (vista === "alumnos") cargarAlumnos();
  if (vista === "csv") evaluarCsv();
}

// Pestaña Inicio. Cuenta alumnos y colores. El HTML vacío está en #metricas.
function cargarMetricas() {
  const riesgos = db.estudiantes.map(riesgoDe);
  const contar = (nivel) => riesgos.filter((r) => r.nivel === nivel).length;
  const tarjetas = [
    ["Alumnos", db.estudiantes.length],
    ["Alto riesgo", contar("rojo")],
    ["Riesgo moderado", contar("amarillo")],
    ["Al día", contar("verde")],
  ];
  document.getElementById("metricas").innerHTML = tarjetas.map(([titulo, valor]) => `
    <article class="tarjeta metrica"><strong>${valor}</strong><span>${titulo}</span></article>
  `).join("");
}

// Pestaña Panel. Lee #panel-comision, pasa ese id a alumnosDe y a riesgoDe,
// ordena (rojo primero) y escribe las filas en #panel-lista.
// Al tocar un alumno llama a seleccionAlumno.pedir(id) y abre Historial.
async function cargarPanelAsync() {
  const select = document.getElementById("panel-comision");
  if (!select.options.length) llenarComisiones(select, "1");
  const comisionId = Number(select.value);
  const token = ++panelToken;
  const alumnos = alumnosDe(comisionId);
  if (token !== panelToken || Number(select.value) !== comisionId) return;

  const filas = alumnos.map((alumno) => {
    const riesgo = riesgoDe(alumno);
    return { alumno, riesgo };
  }).sort((a, b) => a.riesgo.orden - b.riesgo.orden || a.riesgo.peor - b.riesgo.peor);

  const lista = document.getElementById("panel-lista");
  lista.innerHTML = filas.map(({ alumno, riesgo }) => `
    <button type="button" class="alumno" data-id="${alumno.id}">
      <span class="nivel ${riesgo.nivel}">${riesgo.texto}</span>
      <span>
        <strong>${esc(nombreCompleto(alumno))}</strong><br>
        <small>${motivoRiesgo(riesgo)} · asistencia ${riesgo.presentes}/${riesgo.totalClases} (${fmt(riesgo.asistencia)}%) · entregas ${riesgo.entregados}/${riesgo.totalTrabajos} (${fmt(riesgo.entregas)}%)</small>
      </span>
      <small>Abrir ficha</small>
    </button>
  `).join("") || "<p>Esta comisión no tiene alumnos.</p>";

  lista.querySelectorAll(".alumno").forEach((boton) => {
    boton.addEventListener("click", () => {
      seleccionAlumno.pedir(Number(boton.dataset.id));
      mostrar("historial");
    });
  });
}

// Pestaña Historial. Si el Panel pidió un alumno, elige esa comisión y ese alumno.
// Si no, usa lo que haya en los dos select: #historial-comision y #historial-alumno.
function prepararHistorial() {
  const pedido = seleccionAlumno.id;
  const selectComision = document.getElementById("historial-comision");
  if (pedido) {
    const alumno = db.estudiantes.find((e) => e.id === pedido);
    if (alumno) {
      actualizando = true;
      llenarComisiones(selectComision, String(alumno.comisionId));
      llenarAlumnosHistorial(alumno.id);
      actualizando = false;
      cargarDetalleAsync();
      return;
    }
  }
  if (!selectComision.options.length) llenarComisiones(selectComision, "1");
  llenarAlumnosHistorial();
  cargarDetalleAsync();
}

// Arma el select de alumnos del Historial con alumnosDe(comisionId).
// comisionId sale de #historial-comision. alumnoId, si viene, deja ese alumno elegido.
function llenarAlumnosHistorial(alumnoId) {
  const comisionId = Number(document.getElementById("historial-comision").value);
  const select = document.getElementById("historial-alumno");
  const alumnos = alumnosDe(comisionId);
  select.innerHTML = alumnos
    .map((a) => `<option value="${a.id}">${esc(nombreCompleto(a))}</option>`)
    .join("");
  if (alumnoId) select.value = String(alumnoId);
}

// Dibuja la ficha en #historial-detalle: color, clases y trabajos de ese alumno.
async function cargarDetalleAsync() {
  const comisionId = Number(document.getElementById("historial-comision").value);
  const alumnoId = Number(document.getElementById("historial-alumno").value);
  const token = ++historialToken;
  const alumno = db.estudiantes.find((e) => e.id === alumnoId);
  if (token !== historialToken) return;
  const detalle = document.getElementById("historial-detalle");
  if (!alumno || alumno.comisionId !== comisionId) {
    detalle.innerHTML = "<p>Elegí un alumno.</p>";
    return;
  }

  const riesgo = riesgoDe(alumno);
  const clases = clasesDe(comisionId).map((clase) => {
    const registro = db.asistencias.find((a) => a.estudianteId === alumno.id && a.claseId === clase.id);
    let estado = "Sin registro";
    if (registro?.presente) estado = "Presente";
    else if (registro?.justificada) estado = "Ausente justificada";
    else if (registro) estado = "Ausente";
    return `<tr><td>${clase.fecha}</td><td>${esc(clase.descripcion)}</td><td>${estado}</td></tr>`;
  }).join("");

  const trabajos = trabajosDe(comisionId).map((trabajo) => {
    const registro = db.entregas.find((e) => e.estudianteId === alumno.id && e.trabajoPracticoId === trabajo.id);
    const estado = !registro ? "Sin registro" : registro.entregado ? "Entregó" : "No entregó";
    return `<tr><td>${esc(trabajo.titulo)}</td><td>${trabajo.fechaEntrega}</td><td>${estado}</td></tr>`;
  }).join("");

  detalle.innerHTML = `
    <article class="tarjeta" style="margin-top:14px">
      <span class="nivel ${riesgo.nivel}">${riesgo.texto}</span>
      <h2>${esc(nombreCompleto(alumno))}</h2>
      <p>DNI ${esc(alumno.dni)} · ${esc(alumno.email)}</p>
      <p>${motivoRiesgo(riesgo)}. Asistencia ${fmt(riesgo.asistencia)}% (${riesgo.presentes} presentes y ${riesgo.justificadas} justificadas sobre ${riesgo.totalClases} clases). Entregas ${fmt(riesgo.entregas)}% (${riesgo.entregados} de ${riesgo.totalTrabajos}).</p>
      <h3>Clases</h3>
      <table><thead><tr><th>Fecha</th><th>Tema</th><th>Estado</th></tr></thead><tbody>${clases || "<tr><td colspan='3'>Sin clases</td></tr>"}</tbody></table>
      <h3>Trabajos</h3>
      <table><thead><tr><th>Título</th><th>Fecha</th><th>Estado</th></tr></thead><tbody>${trabajos || "<tr><td colspan='3'>Sin trabajos</td></tr>"}</tbody></table>
    </article>
  `;
}

// Pestaña Asistencia. Lista alumnosDe(comisionId) con los checks Presente y Justificada.
// comisionId sale de #asistencia-comision. La fecha sale de #asistencia-fecha.
function cargarAsistencia() {
  const select = document.getElementById("asistencia-comision");
  if (!select.options.length) llenarComisiones(select, "1");
  const fecha = document.getElementById("asistencia-fecha");
  if (!fecha.value) fecha.value = hoyIso();
  const comisionId = Number(select.value);
  const clase = db.clases.find((c) => c.comisionId === comisionId && c.fecha === fecha.value);
  const alumnos = alumnosDe(comisionId);

  document.getElementById("asistencia-lista").innerHTML = alumnos.map((alumno) => {
    const registro = clase && db.asistencias.find((a) => a.estudianteId === alumno.id && a.claseId === clase.id);
    const presente = registro ? registro.presente : true;
    const justificada = registro ? registro.justificada : false;
    return `
      <div class="item">
        <strong>${esc(nombreCompleto(alumno))}</strong>
        <span class="controles">
          <label>Presente <input type="checkbox" data-presente="${alumno.id}" ${presente ? "checked" : ""}></label>
          <label>Justificada <input type="checkbox" data-justificada="${alumno.id}" ${justificada && !presente ? "checked" : ""} ${presente ? "disabled" : ""}></label>
        </span>
      </div>
    `;
  }).join("") || "<p>Esta comisión no tiene alumnos.</p>";

  document.querySelectorAll("[data-presente]").forEach((check) => {
    check.addEventListener("change", () => {
      const justa = document.querySelector(`[data-justificada="${check.dataset.presente}"]`);
      justa.disabled = check.checked;
      if (check.checked) justa.checked = false;
    });
  });
}

// La dispara el botón #guardar-asistencia. Crea la clase si esa fecha no existía
// y actualiza la fila de cada alumno. No duplica si se guarda dos veces.
function guardarAsistenciaAsync() {
  const comisionId = Number(document.getElementById("asistencia-comision").value);
  const fecha = document.getElementById("asistencia-fecha").value;
  if (!fecha) {
    mostrarAviso("asistencia-aviso", "Elegí una fecha.", true);
    return;
  }
  if (fecha > hoyIso()) {
    mostrarAviso("asistencia-aviso", "No se puede tomar asistencia de una fecha futura.", true);
    return;
  }

  let clase = db.clases.find((c) => c.comisionId === comisionId && c.fecha === fecha);
  if (!clase) {
    clase = { id: nuevoId(), fecha, comisionId, descripcion: "Clase del " + fecha };
    db.clases.push(clase);
  }

  for (const alumno of alumnosDe(comisionId)) {
    const presente = document.querySelector(`[data-presente="${alumno.id}"]`).checked;
    const justificada = document.querySelector(`[data-justificada="${alumno.id}"]`).checked;
    let registro = db.asistencias.find((a) => a.estudianteId === alumno.id && a.claseId === clase.id);
    if (!registro) {
      registro = { id: nuevoId(), estudianteId: alumno.id, claseId: clase.id, presente, justificada: !presente && justificada };
      db.asistencias.push(registro);
    } else {
      registro.presente = presente;
      registro.justificada = !presente && justificada;
    }
  }

  guardarDb();
  mostrarAviso("asistencia-aviso", "Asistencia guardada. El panel recalcula el color al abrirlo.", false);
  cargarAsistencia();
}

// Pestaña Entregas. Llena el select de trabajos de la comisión y después la lista de alumnos.
function cargarEntregas() {
  const select = document.getElementById("entregas-comision");
  if (!select.options.length) llenarComisiones(select, "1");
  const trabajos = trabajosDe(Number(select.value));
  const trabajoSelect = document.getElementById("entregas-trabajo");
  const elegido = trabajoSelect.value;
  trabajoSelect.innerHTML = trabajos
    .map((t) => `<option value="${t.id}">${esc(t.titulo)} (${t.fechaEntrega})</option>`)
    .join("");
  if (elegido) trabajoSelect.value = elegido;
  pintarEntregas();
}

// Dibuja quién entregó el trabajo elegido. Sin registro se muestra como no entregado.
function pintarEntregas() {
  const comisionId = Number(document.getElementById("entregas-comision").value);
  const trabajoId = Number(document.getElementById("entregas-trabajo").value);
  const lista = document.getElementById("entregas-lista");
  if (!trabajoId) {
    lista.innerHTML = "<p>Esta comisión todavía no tiene trabajos.</p>";
    return;
  }
  lista.innerHTML = alumnosDe(comisionId).map((alumno) => {
    const registro = db.entregas.find((e) => e.estudianteId === alumno.id && e.trabajoPracticoId === trabajoId);
    const entregado = registro ? registro.entregado : false;
    return `
      <div class="item">
        <strong>${esc(nombreCompleto(alumno))}</strong>
        <label>Entregó <input type="checkbox" data-entrego="${alumno.id}" ${entregado ? "checked" : ""}></label>
      </div>
    `;
  }).join("");
}

// La dispara #crear-trabajo. Rechaza un título repetido en la misma comisión.
function crearTrabajoAsync() {
  const comisionId = Number(document.getElementById("entregas-comision").value);
  const titulo = document.getElementById("trabajo-titulo").value.trim();
  const fecha = document.getElementById("trabajo-fecha").value;
  if (!titulo || !fecha) {
    mostrarAviso("entregas-aviso", "Título y fecha son obligatorios.", true);
    return;
  }
  const repetido = trabajosDe(comisionId).some((t) => t.titulo.toLowerCase() === titulo.toLowerCase());
  if (repetido) {
    mostrarAviso("entregas-aviso", "Esa comisión ya tiene un trabajo con ese título.", true);
    return;
  }
  const trabajo = { id: nuevoId(), comisionId, titulo, fechaEntrega: fecha };
  db.trabajos.push(trabajo);
  guardarDb();
  document.getElementById("trabajo-titulo").value = "";
  mostrarAviso("entregas-aviso", "Trabajo creado.", false);
  cargarEntregas();
  document.getElementById("entregas-trabajo").value = String(trabajo.id);
  pintarEntregas();
}

// La dispara #guardar-entregas. Guarda el check de cada alumno para el trabajo elegido.
function guardarEntregasAsync() {
  const trabajoId = Number(document.getElementById("entregas-trabajo").value);
  const comisionId = Number(document.getElementById("entregas-comision").value);
  if (!trabajoId) {
    mostrarAviso("entregas-aviso", "Elegí un trabajo.", true);
    return;
  }
  for (const alumno of alumnosDe(comisionId)) {
    const entregado = document.querySelector(`[data-entrego="${alumno.id}"]`).checked;
    let registro = db.entregas.find((e) => e.estudianteId === alumno.id && e.trabajoPracticoId === trabajoId);
    if (!registro) {
      db.entregas.push({ id: nuevoId(), estudianteId: alumno.id, trabajoPracticoId: trabajoId, entregado });
    } else {
      registro.entregado = entregado;
    }
  }
  guardarDb();
  mostrarAviso("entregas-aviso", "Entregas guardadas.", false);
}

// Pestaña Alumnos. Muestra la nómina de #alumnos-comision usando alumnosDe(comisionId).
function cargarAlumnos() {
  const select = document.getElementById("alumnos-comision");
  const elegida = select.value || "1";
  llenarComisiones(select, elegida);
  const alumnos = alumnosDe(Number(select.value));
  document.getElementById("alumnos-lista").innerHTML = alumnos.map((alumno) => `
    <div class="item"><span><strong>${esc(nombreCompleto(alumno))}</strong><br><small>DNI ${esc(alumno.dni)} · ${esc(alumno.email)}</small></span></div>
  `).join("") || "<p>Esta comisión no tiene alumnos.</p>";
}

// La dispara #guardar-alumno. Valida campos, email y DNI repetido en esa comisión.
function guardarEstudianteAsync() {
  const comisionId = Number(document.getElementById("alumnos-comision").value);
  const dni = normalizarDni(document.getElementById("alumno-dni").value);
  const nombre = document.getElementById("alumno-nombre").value.trim();
  const apellido = document.getElementById("alumno-apellido").value.trim();
  const email = document.getElementById("alumno-email").value.trim();

  if (!dni || !nombre || !apellido || !email || !comisionId) {
    mostrarAviso("alumnos-aviso", "DNI, nombre, apellido, email y comisión son obligatorios.", true);
    return;
  }
  if (dni.length > LIMITES.dni || nombre.length > LIMITES.nombre || apellido.length > LIMITES.apellido) {
    mostrarAviso("alumnos-aviso", "Hay un dato más largo de lo que acepta la base.", true);
    return;
  }
  if (!esEmailValido(email)) {
    mostrarAviso("alumnos-aviso", "El email no tiene formato de dirección.", true);
    return;
  }
  const existe = db.estudiantes.some((e) => e.comisionId === comisionId && e.dni === dni);
  if (existe) {
    mostrarAviso("alumnos-aviso", "Ese DNI ya está en la comisión.", true);
    return;
  }

  db.estudiantes.push({ id: nuevoId(), dni, nombre, apellido, email, comisionId });
  guardarDb();
  document.getElementById("alumno-dni").value = "";
  document.getElementById("alumno-nombre").value = "";
  document.getElementById("alumno-apellido").value = "";
  document.getElementById("alumno-email").value = "";
  mostrarAviso("alumnos-aviso", "Alumno cargado en la comisión elegida.", false);
  cargarAlumnos();
}

// Parte el texto del CSV en filas. Ignora el encabezado y las filas con menos de 3 columnas.
function parseCsv(texto) {
  const lineas = texto.replace(/^\uFEFF/, "").split(/\r?\n/).filter((linea) => linea.trim());
  const filas = [];
  lineas.forEach((linea, indice) => {
    const separador = linea.includes(";") ? ";" : ",";
    const columnas = linea.split(separador).map((c) => c.trim());
    if (indice === 0 && columnas[0]?.toLowerCase() === "dni") return;
    if (columnas.length < 3) return;
    filas.push({
      dni: columnas[0] || "",
      nombre: columnas[1] || "",
      apellido: columnas[2] || "",
      email: columnas[3] || "",
    });
  });
  return filas;
}

// Pestaña Carga CSV. Compara cada fila con la comisión de #csv-comision
// y marca Nuevo, Ya existe, DNI repetido o error. Todavía no guarda.
function evaluarCsv() {
  const select = document.getElementById("csv-comision");
  if (!select.options.length) llenarComisiones(select, "1");
  const comisionId = Number(select.value);
  const vistos = new Set();
  filasCsv = filasCsv.map((fila) => {
    const dni = normalizarDni(fila.dni);
    let estado = "Nuevo";
    let importa = true;
    if (!dni || !fila.nombre.trim() || !fila.apellido.trim() || !fila.email.trim()) {
      estado = "Datos incompletos";
      importa = false;
    } else if (!esEmailValido(fila.email)) {
      estado = "Email inválido";
      importa = false;
    } else if (dni.length > LIMITES.dni || fila.nombre.trim().length > LIMITES.nombre || fila.apellido.trim().length > LIMITES.apellido) {
      estado = "Dato demasiado largo";
      importa = false;
    } else if (vistos.has(dni)) {
      estado = "DNI repetido en el archivo";
      importa = false;
    } else if (db.estudiantes.some((e) => e.comisionId === comisionId && e.dni === dni)) {
      estado = "Ya existe en la comisión";
      importa = false;
    }
    vistos.add(dni);
    return { ...fila, dniNormalizado: dni, estado, importa };
  });

  const contar = (estado) => filasCsv.filter((f) => f.estado === estado).length;
  const resumen = document.getElementById("csv-resumen");
  if (!filasCsv.length) {
    resumen.textContent = "Elegí un CSV. Con el ejemplo y la comisión A: 6 para importar, 1 ya existe (Lucía), 1 repetido y 1 con errores.";
  } else {
    resumen.textContent = `${contar("Nuevo")} para importar, ${contar("Ya existe en la comisión")} ya existe, ${contar("DNI repetido en el archivo")} repetido en el archivo, ${contar("Datos incompletos") + contar("Email inválido") + contar("Dato demasiado largo")} con errores.`;
  }

  document.getElementById("csv-lista").innerHTML = filasCsv.length ? `
    <table>
      <thead><tr><th>DNI</th><th>Nombre</th><th>Email</th><th>Estado</th></tr></thead>
      <tbody>
        ${filasCsv.map((fila) => `
          <tr>
            <td>${esc(fila.dni)}</td>
            <td>${esc((fila.nombre + " " + fila.apellido).trim())}</td>
            <td>${esc(fila.email)}</td>
            <td class="${fila.importa ? "estado-nuevo" : "estado-malo"}">${esc(fila.estado)}</td>
          </tr>
        `).join("")}
      </tbody>
    </table>
  ` : "";
}

// La dispara #importar-csv. Guarda solo las filas Nuevas, todas juntas.
function importarAsync() {
  const nuevas = filasCsv.filter((f) => f.importa);
  if (!nuevas.length) {
    document.getElementById("csv-resumen").textContent = "No hay filas nuevas para importar.";
    return;
  }
  const comisionId = Number(document.getElementById("csv-comision").value);
  const copia = db.estudiantes.map((e) => ({ ...e }));
  try {
    for (const fila of nuevas) {
      db.estudiantes.push({
        id: nuevoId(),
        dni: fila.dniNormalizado,
        nombre: fila.nombre.trim(),
        apellido: fila.apellido.trim(),
        email: fila.email.trim(),
        comisionId,
      });
    }
    guardarDb();
  } catch (error) {
    db.estudiantes = copia;
    document.getElementById("csv-resumen").textContent = "No se importó nadie. " + error.message;
    return;
  }
  filasCsv = [];
  evaluarCsv();
  document.getElementById("csv-resumen").textContent = "Se importaron " + nuevas.length + " alumnos, todos juntos.";
}

// La dispara #restaurar. Borra los cambios y vuelve a los 12 alumnos de la demo.
function restaurarDemo() {
  db = crearDatosDemo();
  guardarDb();
  filasCsv = [];
  document.querySelectorAll("select").forEach((select) => { select.innerHTML = ""; });
  mostrar("inicio");
}

// CONEXIÓN DE BOTONES.
// El botón vive en index.html. Acá se dice qué función corre al hacer click.
// Pestaña nueva: no se agrega acá. El nav ya llama a mostrar() con data-vista.
// Botón dentro de una pantalla: sí se agrega acá, con el id del botón.
document.querySelectorAll("nav button").forEach((boton) => {
  boton.addEventListener("click", () => mostrar(boton.dataset.vista));
});

document.getElementById("tema").addEventListener("click", () => {
  const oscuro = document.body.dataset.tema === "oscuro";
  document.body.dataset.tema = oscuro ? "" : "oscuro";
  document.getElementById("tema").textContent = oscuro ? "Tema oscuro" : "Tema claro";
});

document.getElementById("ir-panel").addEventListener("click", () => {
  const select = document.getElementById("panel-comision");
  llenarComisiones(select, "1");
  mostrar("panel");
});
document.getElementById("restaurar").addEventListener("click", restaurarDemo);
document.getElementById("panel-comision").addEventListener("change", cargarPanelAsync);
document.getElementById("historial-comision").addEventListener("change", () => {
  if (actualizando) return;
  seleccionAlumno.id = null;
  llenarAlumnosHistorial();
  cargarDetalleAsync();
});
document.getElementById("historial-alumno").addEventListener("change", () => {
  if (actualizando) return;
  seleccionAlumno.id = null;
  cargarDetalleAsync();
});
document.getElementById("asistencia-comision").addEventListener("change", cargarAsistencia);
document.getElementById("asistencia-fecha").addEventListener("change", cargarAsistencia);
document.getElementById("guardar-asistencia").addEventListener("click", guardarAsistenciaAsync);
document.getElementById("entregas-comision").addEventListener("change", cargarEntregas);
document.getElementById("entregas-trabajo").addEventListener("change", pintarEntregas);
document.getElementById("crear-trabajo").addEventListener("click", crearTrabajoAsync);
document.getElementById("guardar-entregas").addEventListener("click", guardarEntregasAsync);
document.getElementById("alumnos-comision").addEventListener("change", cargarAlumnos);
document.getElementById("guardar-alumno").addEventListener("click", guardarEstudianteAsync);
document.getElementById("csv-comision").addEventListener("change", evaluarCsv);
document.getElementById("csv-archivo").addEventListener("change", async (evento) => {
  const archivo = evento.target.files[0];
  if (!archivo) return;
  filasCsv = parseCsv(await archivo.text());
  evaluarCsv();
});
document.getElementById("bajar-csv").addEventListener("click", () => {
  const blob = new Blob([CSV_EJEMPLO], { type: "text/csv;charset=utf-8" });
  const enlace = document.createElement("a");
  enlace.href = URL.createObjectURL(blob);
  enlace.download = "alumnos_demo.csv";
  enlace.click();
  URL.revokeObjectURL(enlace.href);
});
document.getElementById("importar-csv").addEventListener("click", importarAsync);

const vistaInicial = location.hash.replace("#", "");
if (document.getElementById(vistaInicial)) mostrar(vistaInicial);
else cargarMetricas();

// Misma regla que Services/ValidacionEmail.cs → EsValido.
// No vacío, formato de dirección, dominio con punto, hasta 100 caracteres.

function esEmailValido(email) {
  if (email == null) return false;
  const texto = String(email).trim();
  if (!texto || texto.length > 100) return false;
  if (/\s/.test(texto)) return false;
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(texto);
}

function normalizarDni(dni) {
  return String(dni ?? "").replace(/[.\s]/g, "");
}

const LIMITES = { dni: 15, nombre: 50, apellido: 50, email: 100 };

if (typeof module !== "undefined" && module.exports) {
  module.exports = { esEmailValido, normalizarDni, LIMITES };
}

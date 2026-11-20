# ADR 3: Qué pasa cuando faltan datos: sin clases, sin trabajos o alumno incorporado tarde

**Estado:** aceptada

## Contexto

El cliente nunca dijo qué hacer en tres casos: un alumno sin clases dictadas todavía, una comisión sin trabajos vencidos, y un alumno que se incorpora cuando la cursada ya empezó.

Las fuerzas en juego:

- Marcar en verde a quien no tiene datos da una falsa tranquilidad.
- Marcarlo en rojo genera falsas alarmas.
- Un alumno que se incorpora tarde no tiene la culpa de las clases que se dieron antes.

## Decisión

Vamos a resolver los tres casos así:

1. Si no hay clases que contar, el porcentaje de asistencia es 100 %; si no hay trabajos que contar, el de entregas es 100 %. Esa señal no penaliza.
2. Si no hay **ni clases ni trabajos** que contar, el nivel es **"Sin datos"** y no verde.
3. A cada alumno le cuentan solo las clases (y trabajos) desde su **fecha de incorporación** (`Estudiante.FechaAlta`), si la tiene, y las clases anteriores en las que ya tiene un registro de asistencia. Sin fecha, le cuenta toda la historia de la comisión. Ver también el [ADR 10](adr-010-racha-e-incorporacion.md).

## Consecuencias

**Positivas**

- Un alumno recién incorporado (con fecha de hoy) no arranca en rojo.
- "Sin datos" distingue "no hay nada que evaluar" de "está al día".
- Los tres casos tienen pruebas automáticas, junto con el caso de un alumno justo en el umbral.

**Negativas**

- Si se deja la fecha vacía al cargar a alguien que en realidad se incorporó tarde, le cuentan clases y trabajos previos y puede figurar en riesgo sin haber faltado.

**Neutras**

- Una comisión con clases pero sin trabajos se evalúa solo por asistencia, y al revés.

# ADR 10: Racha de ausencias seguidas e incorporación tardía

**Estado:** aceptada

## Contexto

El panel ya ordenaba por riesgo porcentual, pero el tutor necesita dos señales más:

1. Quién dejó de venir (ausencias seguidas), para llamar primero a esa persona y no a quien faltó salteado.
2. Desde cuándo cursa un alumno que se incorporó tarde, para no marcarlo en riesgo por clases y trabajos anteriores a su llegada.

## Decisión

1. **Racha vigente.** Se calcula solo sobre clases con asistencia tomada en la comisión (hay al menos un registro de lista). Las clases futuras y las pasadas sin lista no suman ni cortan. Una presencia corta la racha; una ausencia justificada la sigue. Sin registros del alumno en esas clases → "sin datos", distinto de "sin ausencias seguidas".
2. **Fecha de incorporación opcional** (`Estudiante.FechaAlta`). Si es null, el riesgo cuenta toda la historia de la comisión. Si tiene valor, solo clases y trabajos desde esa fecha (inclusive). No se acepta una fecha posterior a hoy. Se puede cargar en el alta y corregir en la ficha.
3. El panel muestra la base del porcentaje ("1 de 4 clases") y la racha junto al dato que justifica el riesgo.

## Consecuencias

**Positivas**

- El tutor ve de un vistazo quién cortó de asistir.
- Un incorporado tarde no arrastra faltas ni TPs previos.
- "Sin datos" de racha y "sin datos" de nivel siguen siendo distintos de "al día" o "cero faltas".

**Negativas**

- La racha no entra en el color rojo/amarillo/verde: es una señal explicativa. Ordenar también por racha quedaría para otra historia.
- Hay que correr `hacer_fecha_alta_opcional.sql` en bases ya creadas.

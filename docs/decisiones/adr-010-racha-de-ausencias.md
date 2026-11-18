# ADR 10: Racha de ausencias en el panel

**Estado:** aceptada

## Contexto

El tutor quiere ver en el panel cuántas clases seguidas lleva faltando cada estudiante, para llamar primero a quien dejó de venir y no a quien faltó salteado. El cliente definió que la racha se calcula hasta la última clase con asistencia tomada, que se muestra junto al dato del riesgo, y que hay que distinguir "sin ausencias seguidas" de "sin datos". No precisó qué cuenta como "asistencia tomada", si una ausencia justificada corta la racha, ni si el panel tiene que reordenarse por racha.

Las fuerzas en juego:

- El riesgo ya cuenta clases pasadas aunque el tutor no haya tomado lista (ADR 4 y 7): si la racha usara las mismas clases, una clase olvidada alargaría la racha de todos.
- Una justificada sigue siendo "no vino": el objetivo de la historia es priorizar a quien dejó de asistir.
- Mostrar "0 ausencias seguidas" cuando no hay registros confunde con "vino a la última".

## Decisión

Vamos a calcular la racha aparte del riesgo, con estas reglas:

- Una clase tiene asistencia tomada cuando existe **al menos un** registro de asistencia con ese `ClaseId` en la comisión.
- Solo entran clases con lista tomada y fecha hasta hoy. Las futuras y las pasadas sin lista no suman ni cortan.
- Se recorre de la más reciente a la más antigua: cada ausencia (o falta de registro del alumno en una clase con lista) suma uno; la primera presencia corta.
- Una ausencia **justificada** no corta la racha.
- Si el alumno no tiene ningún registro en esas clases, el estado es **sin datos** (no "0 ausencias seguidas").
- Si tiene datos y la última clase con lista lo tuvo presente, el estado es **sin racha** y el panel no muestra un número.
- El orden del panel no cambia: sigue por nivel de riesgo; la racha es informativa.

## Consecuencias

**Positivas**

- La racha responde a "quién dejó de venir" sin mezclarse con el porcentaje de asistencia.
- Los criterios de aceptación se pueden probar sin la base ni MAUI.
- Queda documentado por qué no se reutiliza `SeleccionarClasesDelAlumno`.

**Negativas**

- Si el tutor toma lista parcial y olvida a un alumno, esa clase con lista cuenta como ausencia para ese alumno y puede alargar su racha.
- Una justificada larga aparece como racha vigente aunque el tutor ya sepa el motivo.

**Neutras**

- El texto del panel concatena la racha al detalle de asistencia y entregas cuando hay algo que mostrar.

# ADR 8: El reporte de la comisión cuenta lo que dice el panel y avisa cuando no hay datos

**Estado:** aceptada

## Contexto

La historia H12 pide que el tutor vea cuántos estudiantes de una comisión están en riesgo de abandonar, para saber si el problema es de algunos estudiantes o de la comisión entera. Se desarrolla sobre los datos que ya existen, sin modificar el modelo ni el cálculo de riesgo.

Las fuerzas en juego:

- Si el reporte tuviera su propio cálculo, podría discrepar del panel: es el error que la historia pide evitar.
- El sistema tiene tres niveles (alto riesgo, riesgo moderado y al día) y la historia habla de "en riesgo de abandonar" sin precisar cuáles cuentan.
- Contar "0 en riesgo" cuando no se registró ninguna asistencia ni entrega haría creer que todo está bien; y con clases cargadas pero sin registros, el cálculo daría "todos en alto riesgo", que tampoco es cierto.

## Decisión

Vamos a armar el reporte contando los niveles que ya calculó `ConsultaSeguimiento`, la misma consulta que usa el panel, de modo que los dos no puedan discrepar. Es una clase del dominio (`ReporteComision`) que solo cuenta, y la pantalla es una barra proporcional por nivel, sin librerías de gráficos.

"En riesgo de abandonar" es el nivel de **alto riesgo**, igual que en Inicio y en el panel; los de riesgo moderado se muestran aparte.

El reporte dice que **todavía no hay datos** cuando ningún estudiante de la comisión tiene una sola asistencia, ausencia justificada o entrega registrada, y que **la comisión no tiene estudiantes** cuando está vacía. En esos casos no se muestra ningún número.

## Consecuencias

**Positivas**

- Hay un solo cálculo de riesgo: una prueba de aceptación comprueba que el reporte y el panel dan el mismo número.
- No se muestra "0 en riesgo" ni "todos en riesgo" cuando en realidad no hay datos.
- No cambia el modelo ni el cálculo de riesgo.

**Negativas**

- Para armar el reporte se evalúa a todos los estudiantes de la comisión, con la consulta de entregas alumno por alumno: con cientos de estudiantes tarda varios segundos.
- La regla de "sin datos" es una aproximación: una comisión donde todos hubieran faltado a todas las clases y no entregado nada también diría que no hay datos.
- El reporte hereda una limitación del cálculo: cuenta cuántas clases faltó un estudiante, pero no cuándo. Tres faltas seguidas al final y tres faltas salteadas dan el mismo nivel. Para distinguirlas habría que cambiar el cálculo (`RiesgoService` y el armado de sus datos en `SeguimientoService`) y nada más: las pantallas y el reporte solo consumen el nivel ya calculado.

**Neutras**

- El riesgo moderado se cuenta aparte, así que "en riesgo" en el reporte no es la suma de los dos niveles.
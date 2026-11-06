# ADR 6: Los umbrales de riesgo se configuran desde la aplicación

**Estado:** aceptada. Reemplaza la parte del [ADR 2](adr-002-calculo-del-riesgo.md) que ubicaba los umbrales como constantes del código.

## Contexto

En la clase 9 (23/10) el requerimiento cambió: los umbrales con los que se decide el nivel de riesgo tienen que poder modificarse. Hasta entonces eran cuatro constantes del código, y cambiarlas obligaba a modificar y recompilar la aplicación.

Las fuerzas en juego:

- El tutor es quien conoce su comisión y puede querer ser más o menos exigente, sin saber programar.
- Si el umbral cambia, el nivel de todos los alumnos tiene que cambiar al instante y de manera coherente en todas las pantallas.
- Un umbral mal cargado (por ejemplo, un porcentaje de más de 100 o un rojo mayor que el amarillo) dejaría resultados sin sentido.
- El panel, el Inicio y la ficha tienen que usar siempre los mismos valores.

## Decisión

Vamos a guardar los umbrales en la base de datos, en una tabla de una sola fila (`UmbralesRiesgo`), con los mismos valores de siempre como valores iniciales (75, 60, 85 y 80). Una pantalla (pestaña Umbrales) permite verlos, cambiarlos y volver a los valores por defecto. Se validan antes de guardar: son porcentajes de 0 a 100 y, en la asistencia y en las entregas, el umbral de alto riesgo tiene que ser menor que el de riesgo moderado. Los umbrales siguen expresados en porcentajes, como en el ADR 2.

El cálculo recibe los umbrales como parámetro y los lee cada vez que evalúa a un alumno, de modo que el cambio se refleja de inmediato en todas las pantallas. Este registro complementa al ADR 2: cambia dónde viven los umbrales, no cómo se combinan las señales. Para una base ya existente se agregó el script `agregar_umbrales.sql`, que no borra nada.

## Consecuencias

**Positivas**

- El tutor cambia los umbrales sin tocar el código, y el resultado se ve en la siguiente carga de cada pantalla.
- Los valores están en un solo lugar, la base de datos, y tienen una única regla de validación.
- Los valores por defecto son los de siempre: sin cambios, el sistema se comporta como antes.

**Negativas**

- Es una configuración para todo el sistema: no hay umbrales distintos por comisión.
- Cada evaluación consulta los umbrales a la base, un acceso más por pantalla.
- Hay que actualizar la base de datos existente con un script (`agregar_umbrales.sql`) o recrearla. Si la tabla no existe, el panel y Inicio quedan vacíos sin avisar.

**Neutras**

- Los porcentajes valen para cualquier cantidad de clases; una cantidad fija de faltas no.

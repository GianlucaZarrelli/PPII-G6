# ADR 2: El riesgo combina asistencia y entregas, con umbrales en porcentaje

**Estado:** aceptada. La parte que ubica los umbrales como constantes del código fue reemplazada por el [ADR 6](adr-006-umbrales-configurables.md).

## Contexto

El sistema tiene que contestar "¿a quién llamo esta semana?". El tutor ya registra dos datos de cada alumno: la asistencia y las entregas de trabajos. El cliente no indicó cómo combinarlos ni con qué valores considerar que un alumno está en riesgo.

Las fuerzas en juego:

- Un alumno puede dejar de asistir sin dejar de entregar, y al revés.
- Una ausencia justificada no es lo mismo que una injustificada.
- El resultado se tiene que poder explicar con números que el tutor entienda.
- Un porcentaje sirve para cualquier cantidad de clases; una cantidad fija de faltas, no.

## Decisión

Vamos a calcular dos porcentajes por alumno, el de asistencia y el de entregas, y a clasificarlo así:

- **Rojo** si la asistencia es menor al 75 % **o** las entregas son menores al 60 %.
- **Amarillo** si no es rojo y la asistencia es menor al 85 % **o** las entregas son menores al 80 %.
- **Verde** en cualquier otro caso.

Una ausencia justificada vale la mitad de una presente (0,5). Los valores de los umbrales los fijó el equipo; el cliente no los indicó. Están como constantes con nombre en `Services/RiesgoService.cs`.

## Consecuencias

**Positivas**

- Alcanza con que una sola señal falle para encender la alerta: no se pierde ni al que asiste pero no entrega ni al que entrega pero no asiste.
- El nivel se explica con dos números, que el panel muestra junto al alumno.

**Negativas**

- La regla "o" produce más alumnos en rojo que una combinación ponderada.
- Los umbrales están en el código: cambiarlos requiere modificar y recompilar. Esta consecuencia se resolvió después con el [ADR 6](adr-006-umbrales-configurables.md).
- Con pocas clases, una sola ausencia mueve mucho el porcentaje.

**Neutras**

- El valor de 0,5 para la ausencia justificada es un criterio del equipo y puede ajustarse.

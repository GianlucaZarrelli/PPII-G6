# ADR 5: El riesgo se calcula siempre y el contacto no lo modifica

**Estado:** aceptada

## Contexto

Con las historias H10 y H11 el tutor registra los contactos con cada alumno. Surgieron dos preguntas que el cliente no contestó: si el nivel de riesgo se guarda o se calcula cada vez, y si contactar a un alumno tiene que bajarle el riesgo.

Las fuerzas en juego:

- Un nivel guardado puede quedar desactualizado si cambia la asistencia o las entregas.
- Que hayan llamado a un alumno no significa que haya empezado a asistir ni a entregar.
- El tutor necesita distinguir a quién ya llamó y a quién le falta llamar.

## Decisión

Vamos a calcular el nivel de riesgo cada vez que se muestra una pantalla, a partir de la asistencia y las entregas, y a no guardarlo. Los contactos se registran aparte: se suman al resumen del alumno (fecha y hora del último contacto y cantidad), pero **no cambian su nivel**. El panel muestra el contacto en verde cuando existe, y "Sin contactar" en naranja si el alumno está en riesgo y nadie lo contactó.

## Consecuencias

**Positivas**

- El nivel nunca está desactualizado.
- El panel muestra a quién falta llamar sin esconder a los alumnos en riesgo.

**Negativas**

- Un alumno en rojo ya contactado sigue en rojo hasta que cambie su asistencia o sus entregas; la diferencia se ve solo en el texto del contacto.
- Cada pantalla vuelve a calcular: las entregas se consultan alumno por alumno, lo que no escala bien en comisiones grandes.

**Neutras**

- El resumen del alumno incluye el último contacto, pero ninguna regla de riesgo lo usa.

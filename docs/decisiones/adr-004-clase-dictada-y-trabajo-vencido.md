# ADR 4: Qué cuenta como clase dictada y como trabajo vencido

**Estado:** aceptada. Su primera regla (las clases existen desde que se toma asistencia) fue reemplazada por el [ADR 7](adr-007-calendario-por-adelantado.md); las otras tres siguen vigentes.

## Contexto

El riesgo se calcula contra lo que ya pasó, y el cliente no precisó qué cuenta como "ya pasó". Hay dos preguntas: si una clase sin asistencia cargada cuenta, y si un trabajo cuya fecha de entrega es hoy cuenta como no entregado.

Las fuerzas en juego:

- No hay que marcar como "no entregó" a quien todavía puede entregar.
- Si el tutor olvida marcar a un alumno en una clase, el sistema tiene que decidir qué hacer.

## Decisión

Vamos a aplicar estas reglas:

- Una clase existe en el sistema desde que se registra su asistencia. Por ahora no se cargan clases por adelantado.
- Si un alumno no tiene registro en una clase que existe, cuenta como **ausente**.
- Un trabajo cuenta recién desde el **día siguiente** a su fecha de entrega: hasta ese día todavía se puede entregar.
- Un trabajo vencido sin registro de entrega cuenta como **no entregado**.

## Consecuencias

**Positivas**

- No se marca como no entregado un trabajo que todavía está a tiempo.
- No quedan alumnos "sin contar" por falta de un registro: la falta de registro se trata siempre igual.

**Negativas**

- Si el tutor olvida marcar a un alumno, figura ausente aunque haya venido.
- Mientras no exista el calendario por adelantado (historia H2), las clases solo aparecen cuando se toma asistencia. Esta limitación se resolvió después con el [ADR 7](adr-007-calendario-por-adelantado.md).

**Neutras**

- Cuando se cargue el calendario completo, las clases futuras tendrán que quedar fuera del cálculo; ese día este registro se reemplaza por uno nuevo.

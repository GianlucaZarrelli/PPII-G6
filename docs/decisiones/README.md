# Registro de decisiones

Cada registro describe una decisión que el cliente no tomó y que tuvo que tomar el equipo, con su contexto y sus consecuencias, incluidas las negativas. Cuando una decisión cambia, no se edita el registro: se escribe uno nuevo que lo reemplaza, y el viejo indica en su estado cuál lo reemplazó.

| N.º | Decisión | Estado |
|---|---|---|
| [1](adr-001-motor-de-base-de-datos.md) | Usamos SQL Server con Entity Framework Core | Aceptada |
| [2](adr-002-calculo-del-riesgo.md) | El riesgo combina asistencia y entregas, con umbrales en porcentaje | Aceptada; los umbrales en el código fueron reemplazados por el ADR 6 |
| [3](adr-003-datos-que-faltan.md) | Qué pasa cuando faltan datos: sin clases, sin trabajos o alumno incorporado tarde | Aceptada |
| [4](adr-004-clase-dictada-y-trabajo-vencido.md) | Qué cuenta como clase dictada y como trabajo vencido | Aceptada; su primera regla fue reemplazada por el ADR 7 |
| [5](adr-005-el-contacto-no-cambia-el-riesgo.md) | El riesgo se calcula siempre y el contacto no lo modifica | Aceptada |
| [6](adr-006-umbrales-configurables.md) | Los umbrales de riesgo se configuran desde la aplicación | Aceptada |
| [7](adr-007-calendario-por-adelantado.md) | Las clases se cargan por adelantado y las futuras no cuentan | Aceptada |
| [8](adr-008-reporte-de-la-comision.md) | El reporte de la comisión cuenta lo que dice el panel y avisa cuando no hay datos | Aceptada |
| [9](adr-009-restricciones-unicas-en-el-modelo.md) | Las restricciones únicas también se declaran en el modelo, para probarlas contra una base real | Aceptada |
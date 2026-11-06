# ADR 7: Las clases se cargan por adelantado y las futuras no cuentan

**Estado:** aceptada. Reemplaza la primera regla del [ADR 4](adr-004-clase-dictada-y-trabajo-vencido.md).

## Contexto

La historia H2 pide un calendario de clases y trabajos por comisión. Hasta ese momento una clase existía solo desde que se tomaba su asistencia, así que el calendario no se podía armar con anticipación.

Las fuerzas en juego:

- El tutor sabe de antemano las fechas del cuatrimestre y quiere cargarlas de una vez.
- Si una clase futura contara para el riesgo, todos los alumnos figurarían ausentes en clases que todavía no se dieron y pasarían a rojo.
- Los trabajos ya se crean con su fecha en la pantalla Entregas.
- La base no admite dos clases en la misma fecha de una comisión.

## Decisión

Vamos a permitir cargar clases con fecha futura, dentro del año en curso, una por día y por comisión, con una descripción opcional de hasta 200 caracteres. El calendario muestra juntas las clases y las entregas de los trabajos, ordenadas por fecha.

Al calcular el riesgo, **solo cuentan las clases hasta hoy inclusive** (y desde el alta del alumno): una clase futura no cuenta, y una clase de hoy cuenta aunque todavía no tenga asistencia. En la ficha, una clase futura se muestra como "Próxima clase (aún no cuenta)". Los trabajos se siguen creando en la pantalla Entregas, y la regla de que la fecha esté dentro del año en curso es la misma que ya usaban los trabajos.

## Consecuencias

**Positivas**

- Cargar el cuatrimestre entero no cambia el nivel de ningún alumno: una prueba automática lo verifica.
- Los trabajos y las clases se ven juntos en una sola pantalla.

**Negativas**

- Una clase cargada por error no se puede editar ni borrar desde la aplicación.
- Solo se aceptan fechas del año en curso, igual que en los trabajos.
- Si el tutor carga la clase de hoy y no toma asistencia, todos figuran ausentes en ella, como en el ADR 4.

**Neutras**

- Este registro reemplaza solo la primera regla del ADR 4; las otras tres (ausencia sin registro, trabajo desde el día siguiente al vencimiento y no entregado sin registro) siguen vigentes.

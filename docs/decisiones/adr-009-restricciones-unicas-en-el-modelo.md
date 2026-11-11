# ADR 9: Las restricciones únicas también se declaran en el modelo, para probarlas contra una base real

**Estado:** aceptada

## Contexto

La base de datos se arma con scripts SQL y sin migraciones (ADR 1). Las restricciones únicas viven solo en esos scripts: un DNI por comisión, un nombre de comisión, una clase por fecha en cada comisión, una asistencia por estudiante y clase, y una entrega por estudiante y trabajo.

El sprint 8 pide pruebas de integración del adaptador. Para correrlas rápido y sin ensuciar la base de la aplicación, usamos una base SQLite en memoria que se crea a partir del modelo de `AppDbContext`.

Las fuerzas en juego:

- El doble de las pruebas unitarias acepta dos estudiantes con el mismo DNI; la base real no. Probar eso es el sentido de una prueba de integración.
- Si el modelo no declara las restricciones, la base SQLite armada desde él acepta los duplicados y la prueba no prueba nada.
- No queremos agregar migraciones ni cambiar cómo se crea la base real.
- Tener la misma regla en dos lugares puede hacer que se desalineen.

## Decisión

Vamos a declarar en `AppDbContext`, con `HasIndex(...).IsUnique()`, las mismas cinco restricciones únicas que ya tienen los scripts. Los scripts siguen siendo la forma de crear la base real, y no se agregan migraciones. Las pruebas de integración usan una base SQLite en memoria creada desde el modelo.

## Consecuencias

**Positivas**

- Se puede probar el adaptador contra una base real que rechaza los duplicados, y no solo contra un doble que los acepta.
- El modelo documenta las restricciones que la base impone.

**Negativas**

- Cada restricción está escrita en dos lugares (el script y el modelo), y si se cambia una sola pueden quedar desalineadas.
- SQLite no es SQL Server: no cubre las restricciones `CHECK` ni las claves foráneas de los scripts, y se comporta distinto en cosas como la comparación de nombres con mayúsculas.

**Neutras**

- La base real no cambia: ya tenía estas restricciones. Declararlas en el modelo no crea ni modifica nada en ella.
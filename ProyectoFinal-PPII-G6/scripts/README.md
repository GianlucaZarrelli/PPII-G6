# Scripts de base de datos

Todos se ejecutan en SQL Server Management Studio, conectado con una
cuenta administradora (por ejemplo `sa`).

## Armar la base de cero (orden obligatorio)

| Paso | Archivo | Qué hace |
|---|---|---|
| 1 | `crear_base.sql` | Borra la base si existe y la crea con todas las tablas, incluida la de umbrales de riesgo con sus valores por defecto (75, 60, 85 y 80) |
| 2 | `comisiones.sql` | Crea las comisiones A, B y C (opcional: las comisiones también se pueden crear desde la aplicación) |
| 3 | `datos_demo.sql` | Carga 12 alumnos con clases, trabajos, entregas y algunos contactos, y deja los umbrales en sus valores por defecto (opcional) |

## Actualizar una base que ya existe, sin borrarla

`agregar_umbrales.sql` agrega la tabla de umbrales de riesgo con sus valores por defecto. No borra nada y se puede correr más de una vez: si la tabla ya está, no hace nada. Sirve para una base creada antes de que los umbrales fueran configurables.

## Ensayar la demo sin recrear la base

1. `limpiar_datos.sql`: vacía los datos y reinicia los ids. No toca los umbrales.
2. `datos_demo.sql`: vuelve a cargar los datos de demostración.

## Advertencias

- `crear_base.sql` borra todos los datos. No ejecutarlo después de cargar datos reales.
- Las fechas de `datos_demo.sql` se calculan hacia atrás desde hoy: conviene correrlo el día de la demo o el anterior.
- La tabla `Estudiantes` tiene la columna `FechaAlta` (desde cuándo cuenta el alumno en su comisión). Una base creada con una versión anterior a la 0.2.0 hay que recrearla con `crear_base.sql`.
- `datos_demo.sql` da de alta a los 12 alumnos 60 días atrás, antes de la primera clase. Un alumno que se cargue después queda con la fecha de hoy y figura como "Sin datos" hasta que haya clases o trabajos desde su alta.
- Si la tabla de umbrales no existe, el panel, Inicio y la ficha aparecen vacíos sin ningún aviso. Se arregla con `agregar_umbrales.sql`.

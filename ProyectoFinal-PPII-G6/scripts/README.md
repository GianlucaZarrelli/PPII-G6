# Scripts de base de datos

Todos se ejecutan en SQL Server Management Studio, conectado con una
cuenta administradora (por ejemplo `sa`).

## Armar la base de cero (orden obligatorio)

| Paso | Archivo | Qué hace |
|---|---|---|
| 1 | `crear_base.sql` | Borra la base si existe y la crea con todas las tablas |
| 2 | `comisiones.sql` | Crea las comisiones A, B y C |
| 3 | `datos_demo.sql` | Carga 12 alumnos con clases, trabajos y entregas (opcional) |

## Ensayar la demo sin recrear la base

1. `limpiar_datos.sql`: vacía los datos y reinicia los ids.
2. `datos_demo.sql`: vuelve a cargar los datos de demostración.

## Advertencias

- `crear_base.sql` borra todos los datos. No ejecutarlo después de cargar datos reales.
- Las fechas de `datos_demo.sql` se calculan hacia atrás desde hoy: conviene correrlo el día de la demo o el anterior.

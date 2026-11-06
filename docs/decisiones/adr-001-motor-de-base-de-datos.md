# ADR 1: Usamos SQL Server con Entity Framework Core

**Estado:** aceptada

## Contexto

El sprint 3 pidió implementar un motor de base de datos. El proyecto empezó guardando los datos en SQLite y pasó a SQL Server con Entity Framework Core. Se decidió usar desde temprano un motor de base de datos de servidor, para que, si la aplicación crece (más usuarios o accesos al mismo tiempo), no haya que revisar esta decisión más adelante.

Las fuerzas en juego:

- Un motor embebido como SQLite no requiere instalar nada: la base es un archivo y se crea sola.
- SQL Server requiere instalar y configurar una instancia, pero la base impone restricciones propias (claves foráneas, valores únicos y restricciones de valores) y se administra con usuarios y permisos.
- El sistema lo usa una sola persona y no tiene integraciones.
- Las reglas de negocio no tienen por qué saber qué motor hay debajo.

## Decisión

Vamos a usar SQL Server con Entity Framework Core. Todo el acceso a datos queda detrás de la interfaz `IDataService`, que implementa `SqlServerDataService` en la carpeta `Infraestructura`. La base se arma con scripts SQL (`scripts/`), sin migraciones. La conexión se toma de la variable de entorno `PPII_DB`; si no existe, se usa `Server=.\SQLEXPRESS` con autenticación de Windows.

## Consecuencias

**Positivas**

- Las restricciones de la base (`UNIQUE`, claves foráneas, `CHECK`) respaldan las validaciones de la aplicación.
- El dominio y las pruebas no dependen del motor: las pruebas usan una base falsa en memoria.

**Negativas**

- Para ejecutar el sistema hay que instalar y configurar SQL Server.
- Los scripts hay que correrlos en orden, y un cambio de esquema obliga a recrear la base o a escribir un script de actualización (como `agregar_umbrales.sql`).
- Ejecutar la aplicación en otro sistema operativo requiere un servidor accesible por red.
- Las pruebas unitarias usan una base falsa, así que no prueban el acceso a datos real: faltan las pruebas de integración.

**Neutras**

- Cambiar de motor significa escribir otra implementación de `IDataService`, sin tocar el resto.

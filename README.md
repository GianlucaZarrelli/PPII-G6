# Sistema de Seguimiento de Cursada y Alerta Temprana — Grupo 6 (PPII)

![Pruebas](https://github.com/GianlucaZarrelli/PPII-G6/actions/workflows/pruebas.yml/badge.svg)

Aplicación **.NET MAUI** (patrón **MVVM**) para el seguimiento de la cursada y la detección temprana de alumnos en riesgo. Responde a una pregunta: **"¿A quién llamo esta semana?"**.

## Qué hace

- **Alumnos:** se elige una comisión, se ve su nómina y se cargan alumnos en ella. DNI, nombre, apellido y email son obligatorios. El DNI admite solo números (de 6 a 9 dígitos) y no puede repetirse dentro de la comisión; el email se valida.
- **Carga masiva:** se importa un archivo CSV (`Dni;Nombre;Apellido;Email`, con encabezado). Antes de guardar se muestra una vista previa que marca cada fila como nueva, ya existente, repetida en el archivo o con errores, y se guarda todo o nada.
- **Asistencia por fecha:** se elige comisión y fecha, y se marca a cada alumno como presente o ausente (justificada o no). Sirve para tomar la asistencia y para corregirla. No se acepta una fecha futura.
- **Entregas:** se crean los trabajos prácticos de una comisión (con un título de hasta 150 caracteres y una fecha de entrega dentro del año en curso) y se marca quién entregó cada uno.
- **Panel de riesgo:** los alumnos de una comisión ordenados de mayor a menor riesgo, con el dato que lo justifica (por ejemplo, "Asistencia 4/6 (67 %)  |  Entregas 1/3 (33 %)").
- **Historial del alumno:** ficha con su nivel, el detalle de asistencia clase por clase y el estado de cada trabajo.
- **Inicio:** resumen con el total de alumnos y los que están en riesgo alto.

## Cómo se calcula el riesgo

Se combinan dos señales, siempre contra las clases y los trabajos **de la comisión del alumno**:

| Nivel | Condición |
|---|---|
| Rojo | Asistencia menor al 75 % **o** entregas menores al 60 % |
| Amarillo | Asistencia menor al 85 % **o** entregas menores al 80 % |
| Verde | El resto |

- Una ausencia **justificada** vale la mitad de una injustificada.
- Una clase sin registro de asistencia cuenta como ausencia, y un trabajo vencido sin registro, como no entregado.
- A cada alumno le cuentan las clases desde su **fecha de alta** (y las anteriores en las que ya tiene un registro). Un alumno que se incorpora tarde no arranca en rojo.
- Un trabajo cuenta recién desde el **día siguiente** a su fecha de entrega: antes de eso todavía se puede entregar.
- Si no hay clases ni trabajos que contar, el nivel es **"Sin datos"** y no verde.
- Los umbrales son constantes de `Services/RiesgoService.cs`.

## Alcance

| Historia | Estado |
|---|---|
| H1 Nómina | Alta manual, nómina por comisión y carga masiva por CSV |
| H2 Calendario | Parcial: las clases se crean al registrar asistencia y los trabajos desde la pantalla de entregas |
| H3 / H4 Asistencia | Registro y corrección por fecha |
| H5 Entregas | Crear trabajos y marcar entregas |
| H6 Riesgo | Semáforo con dos señales |
| H7 Umbrales configurables | Pendiente |
| H8 Panel por riesgo | Lista por comisión con el detalle |
| H9 Ficha del alumno | Historial de asistencia y entregas |
| H10 / H11 Intervenciones | Pendiente |

## Estructura

    ProyectoFinal-PPII-G6/            Aplicación MAUI
    ├── Models/                       Entidades (Estudiante, Comision, Clase, Asistencia, TrabajoPractico, Entrega)
    ├── Services/                     Lógica sin Entity Framework ni MAUI: cálculo de riesgo, seguimiento,
    │                                 validaciones y lectura de CSV (IDataService es la interfaz de acceso a datos)
    ├── Infraestructura/              Acceso a datos con Entity Framework Core y SQL Server
    ├── Presentacion/                 Textos, colores y avisos que dependen de la pantalla
    ├── ViewModels/                   Lógica de cada pantalla (MVVM)
    ├── Views/                        Pantallas en XAML
    └── scripts/                      Scripts de la base de datos (ver scripts/README.md)
    ProyectoFinal-PPII-G6.Tests/      Pruebas unitarias xUnit
    .github/workflows/pruebas.yml     Compila la aplicación y corre las pruebas en cada cambio

`Models` y `Services` no dependen de Entity Framework ni de MAUI: una prueba automática lo vigila. Cambiar de motor de base de datos implicaría escribir otra implementación de `IDataService` en `Infraestructura`, sin tocar el resto.

## Decisiones de diseño

- La comisión es un dato de cada alumno, clase y trabajo, y se elige de una lista: así no hay comisiones duplicadas por un error de tipeo.
- El riesgo se calcula cada vez y nunca se guarda: siempre refleja los últimos datos cargados.
- Hay una sola regla de riesgo, en un solo servicio: ninguna pantalla vuelve a calcularla.
- La base se arma con scripts, sin migraciones.

## Tecnologías

.NET MAUI · C# · MVVM · Entity Framework Core · SQL Server · xUnit · GitHub Actions

## Cómo ejecutarlo

1. Tener **Visual Studio** con la carga de trabajo **.NET Multi-platform App UI**, el SDK de **.NET 10** y **SQL Server Express** (instancia `SQLEXPRESS`) o LocalDB. La aplicación se ejecuta en **Windows**.
2. Clonar el repositorio y abrir `ProyectoFinal-PPII-G6/ProyectoFinal-PPII-G6.slnx`.
3. Crear la base: ejecutar en SSMS los scripts de `ProyectoFinal-PPII-G6/scripts/` en orden (`crear_base.sql`, `comisiones.sql` y, opcionalmente, `datos_demo.sql`).
4. Elegir `ProyectoFinal-PPII-G6` como proyecto de inicio y **Windows Machine** como destino, y ejecutar.

La conexión se toma de la variable de entorno `PPII_DB`. Si no existe, se usa `Server=.\SQLEXPRESS` con autenticación de Windows.

Para correr las pruebas: *Test Explorer* en Visual Studio, o `dotnet test ProyectoFinal-PPII-G6.Tests` desde la raíz del repositorio.

## 👥 Equipo de Desarrollo — Práctica Profesional II (Grupo 6)

```text
Integrantes/
├── Gianluca Zarrelli     
├── Maia Celeste Mazza    
├── Facundo Araujo        
└── Gabriel Bergamini     
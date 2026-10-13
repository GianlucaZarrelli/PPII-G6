# Sistema de Seguimiento de Cursada y Alerta Temprana — Grupo 6 (PPII)

Aplicación **.NET MAUI** (patrón **MVVM**) para el seguimiento de la cursada y la detección temprana de alumnos en riesgo. Responde a una pregunta: **"¿A quién llamo esta semana?"**.

## Qué hace

- **Alumnos:** alta manual con su comisión (la comisión se crea si no existe) y listado.
- **Asistencia por fecha:** se elige comisión y fecha, y se marca a cada alumno como presente o ausente (justificada o no). Sirve para tomar la asistencia y para corregirla.
- **Entregas:** se crean los trabajos prácticos de una comisión y se marca quién entregó cada uno.
- **Panel de riesgo:** los alumnos de una comisión ordenados de mayor a menor riesgo, con el dato que lo justifica (por ejemplo, "Asistencia 4/6 (67%) | Entregas 1/3 (33%)").
- **Historial del alumno:** ficha con su semáforo, el detalle de asistencia clase por clase y el estado de cada trabajo.
- **Inicio:** resumen con el total de alumnos y los que están en riesgo.

## Cómo se calcula el riesgo

Se combinan dos señales, siempre contra las clases y los trabajos **de la comisión del alumno**:

| Nivel | Condición |
|---|---|
| Rojo | asistencia menor al 75 % **o** entregas menores al 60 % |
| Amarillo | asistencia menor al 85 % **o** entregas menores al 80 % |
| Verde | el resto |

- Una ausencia **justificada** vale la mitad de una injustificada.
- Una clase sin registro de asistencia cuenta como ausencia.
- Los umbrales están fijos en `Services/RiesgoService.cs`.

## Alcance de la demo

| Historia | Estado |
|---|---|
| H1 Nómina | Alta manual y listado |
| H2 Calendario | Parcial: las clases se crean al registrar asistencia y los trabajos desde la pantalla de entregas |
| H3 / H4 Asistencia | Registro y corrección por fecha |
| H5 Entregas | Crear trabajos y marcar entregas |
| H6 Riesgo | Semáforo con dos señales |
| H7 Umbrales configurables | No incluida |
| H8 Panel por riesgo | Lista por comisión con el detalle |
| H9 Ficha del alumno | Historial de asistencia y entregas |
| H10 / H11 Intervenciones | No incluidas |

## Estructura

    ProyectoFinal-PPII-G6/        Aplicación MAUI
    ├── Models/                   Entidades (Estudiante, Comision, Clase, Asistencia, TrabajoPractico, Entrega)
    ├── ViewModels/               Lógica de presentación (MVVM)
    ├── Views/                    Pantallas en XAML
    └── Services/                 Acceso a datos (IDataService, EF Core) y cálculo de riesgo (RiesgoService)
    ProyectoFinal-PPII-G6.Tests/  Pruebas unitarias xUnit del cálculo de riesgo
    scripts/                      Scripts de la base de datos (ver scripts/README.md)

## Tecnologías

.NET MAUI · C# · MVVM · Entity Framework Core · SQL Server · xUnit

## Cómo ejecutarlo

1. Tener **Visual Studio** con la carga de trabajo **.NET Multi-platform App UI**, el SDK de **.NET 10** y **SQL Server Express** (instancia `SQLEXPRESS`) o LocalDB.
2. Clonar el repositorio y abrir `ProyectoFinal-PPII-G6/ProyectoFinal-PPII-G6.sln`.
3. Crear la base: ejecutar en SSMS los scripts de `scripts/` en orden (`crear_base.sql`, `comisiones.sql` y opcionalmente `datos_demo.sql`).
4. Elegir `ProyectoFinal-PPII-G6` como proyecto de inicio, **Windows Machine** como destino y ejecutar con F5.

Las pruebas se corren desde **Test → Run All Tests**.

## 👥 Equipo de Desarrollo — Práctica Profesional II (Grupo 6)

```text
Integrantes/
├── Gianluca Zarrelli     
├── Maia Celeste Mazza    
├── Facundo Araujo        
└── Gabriel Bergamini     

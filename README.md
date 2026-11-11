# Sistema de Seguimiento de Cursada y Alerta Temprana — Grupo 6 (PPII)

![Pruebas](https://github.com/GianlucaZarrelli/PPII-G6/actions/workflows/pruebas.yml/badge.svg)

Aplicación **.NET MAUI** (patrón **MVVM**) para el seguimiento de la cursada y la detección temprana de alumnos en riesgo. Responde a una pregunta: **"¿A quién llamo esta semana?"**.

## Qué hace

- **Alumnos:** se elige una comisión, se ve su nómina y se cargan alumnos en ella. DNI, nombre, apellido y email son obligatorios. El DNI admite solo números (de 6 a 9 dígitos) y no puede repetirse dentro de la comisión; el email se valida. Desde la misma pantalla se pueden **crear comisiones nuevas**.
- **Carga masiva:** se importa un archivo CSV (`Dni;Nombre;Apellido;Email`, con encabezado). Antes de guardar se muestra una vista previa que marca cada fila como nueva, ya existente, repetida en el archivo o con errores, y se guarda todo o nada.
- **Asistencia por fecha:** se elige comisión y fecha, y se marca a cada alumno como presente o ausente (justificada o no). Sirve para tomar la asistencia y para corregirla. No se acepta una fecha futura.
- **Calendario:** se cargan por adelantado las fechas de las clases de una comisión (con una descripción opcional) y se consulta el calendario completo, con las clases y las entregas de los trabajos ordenadas por fecha y cuánto falta o cuánto pasó de cada una.
- **Entregas:** se crean los trabajos prácticos de una comisión (con un título de hasta 150 caracteres y una fecha de entrega dentro del año en curso) y se marca quién entregó cada uno.
- **Panel de riesgo:** los alumnos de una comisión ordenados de mayor a menor riesgo, con el dato que lo justifica (por ejemplo, "Asistencia 4/6 (67 %)  |  Entregas 1/3 (33 %)") y si ya se lo contactó: la fecha y hora del último contacto o "Sin contactar". Al tocar un alumno se abre su ficha.
- **Reporte:** se elige una comisión y se ve cuántos de sus estudiantes están en riesgo de abandonar ("7 de 30 estudiantes en alto riesgo de abandonar"), con una barra por nivel. Usa el mismo cálculo que el panel, así que los números coinciden. Si la comisión no tiene estudiantes, o todavía no hay ninguna asistencia ni entrega registrada, lo dice en lugar de mostrar "0 en riesgo".
- **Contactos:** se registra un contacto con un alumno (fecha y hora, medio —llamada, mail, presencial o mensaje— y resultado) y se ve el historial de contactos de ese alumno.
- **Umbrales:** el tutor cambia los porcentajes con los que se decide el nivel de riesgo (alto riesgo y riesgo moderado, para la asistencia y para las entregas) y ve cuántos alumnos quedan en cada nivel. Los niveles se recalculan de inmediato en todas las pantallas, y se puede volver a los valores por defecto.
- **Historial del alumno:** ficha con su nivel, el detalle de asistencia clase por clase (las clases futuras figuran como "Próxima clase"), el estado de cada trabajo y sus contactos.
- **Inicio:** resumen con el total de alumnos y los que están en riesgo alto, acceso a los módulos y cambio entre tema claro y oscuro.

## Cómo se calcula el riesgo

Se combinan dos señales, siempre contra las clases y los trabajos **de la comisión del alumno**. Con los umbrales por defecto:

| Nivel | Condición |
|---|---|
| Rojo | Asistencia menor al 75 % **o** entregas menores al 60 % |
| Amarillo | Asistencia menor al 85 % **o** entregas menores al 80 % |
| Verde | El resto |

- Una ausencia **justificada** vale la mitad de una injustificada.
- Una clase sin registro de asistencia cuenta como ausencia, y un trabajo vencido sin registro, como no entregado.
- A cada alumno le cuentan las clases desde su **fecha de alta** hasta hoy (y las anteriores en las que ya tiene un registro). Un alumno que se incorpora tarde no arranca en rojo, y una clase cargada por adelantado **no cuenta hasta que llega su día**.
- Un trabajo cuenta recién desde el **día siguiente** a su fecha de entrega: antes de eso todavía se puede entregar.
- Si no hay clases ni trabajos que contar, el nivel es **"Sin datos"** y no verde.
- Contactar a un alumno **no cambia su nivel**: el contacto se muestra aparte.
- Los cuatro umbrales (por defecto 75, 60, 85 y 80) se cambian desde la pantalla **Umbrales** y se guardan en la base de datos.

## Alcance: las once historias y la H12

| Historia | Qué incluye |
|---|---|
| H1 Nómina | Alta manual, nómina por comisión, carga masiva por CSV y creación de comisiones |
| H2 Calendario | Fechas de clases cargadas por adelantado y calendario de la comisión; los trabajos se crean en Entregas |
| H3 / H4 Asistencia | Registro y corrección por fecha |
| H5 Entregas | Crear trabajos y marcar entregas |
| H6 Riesgo | Semáforo con dos señales |
| H7 Umbrales configurables | Pantalla para cambiarlos, con recálculo inmediato |
| H8 Panel por riesgo | Lista por comisión con el detalle |
| H9 Ficha del alumno | Historial de asistencia, entregas y contactos |
| H10 Registro de contactos | Registro con fecha y hora, medio y resultado, e historial por alumno |
| H11 Contactos en el panel | El panel y la ficha muestran el último contacto o "Sin contactar" |
| H12 Reporte de la comisión | Cuántos estudiantes están en riesgo de abandonar, con el mismo cálculo del panel y una barra por nivel; avisa cuando todavía no hay datos |

## Estructura

    ProyectoFinal-PPII-G6/            Aplicación MAUI
    ├── Models/                       Entidades (Estudiante, Comision, Clase, Asistencia, TrabajoPractico, Entrega,
    │                                 Contacto, UmbralesRiesgo)
    ├── Services/                     Lógica sin Entity Framework ni MAUI: cálculo de riesgo, seguimiento, calendario,
    │                                 validaciones y lectura de CSV. Las interfaces IDataService, IDialogoService,
    │                                 INavegacion e ITema son los puntos de contacto con lo que depende de la plataforma
    ├── Infraestructura/              Acceso a datos con Entity Framework Core y SQL Server
    ├── Presentacion/                 Lo que depende de MAUI y no es una pantalla: textos y colores de cada nivel y estado,
    │                                 avisos, navegación y tema
    ├── ViewModels/                   Lógica de cada pantalla (MVVM)
    ├── Views/                        Pantallas en XAML, sin lógica: el código de cada una solo la crea y avisa que apareció
    └── scripts/                      Scripts de la base de datos (ver scripts/README.md)
    ProyectoFinal-PPII-G6.Tests/      Pruebas unitarias xUnit
    docs/                             Arquitectura, requerimientos no funcionales y registro de decisiones
    .github/workflows/pruebas.yml     Compila la aplicación y corre las pruebas en cada cambio

`Models` y `Services` no dependen de Entity Framework ni de MAUI: una prueba automática lo vigila. Cambiar de motor de base de datos implicaría escribir otra implementación de `IDataService` en `Infraestructura`, sin tocar el resto.

## Documentación

- [Arquitectura actual](docs/arquitectura.md): cómo está organizado el sistema y hacia dónde apuntan las dependencias.
- [Especificación complementaria](docs/especificacion-complementaria.md): los requerimientos no funcionales, escritos como medidas que se pueden comprobar.
- [Registro de decisiones](docs/decisiones/README.md): las reglas de negocio que no vinieron del cliente y por qué las tomamos así.

## Tecnologías

.NET MAUI · C# · MVVM · Entity Framework Core · SQL Server · xUnit · GitHub Actions

## Cómo ejecutarlo

1. Tener **Visual Studio** con la carga de trabajo **.NET Multi-platform App UI**, el SDK de **.NET 10** y **SQL Server Express** (instancia `SQLEXPRESS`) o LocalDB. La aplicación se ejecuta en **Windows**.
2. Clonar el repositorio y abrir `ProyectoFinal-PPII-G6/ProyectoFinal-PPII-G6.slnx`.
3. Crear la base: ejecutar en SSMS, en orden, `ProyectoFinal-PPII-G6/scripts/crear_base.sql` y, opcionalmente, `comisiones.sql` (las comisiones también se pueden crear desde la aplicación) y `datos_demo.sql` (datos de ejemplo). Para volver a una base vacía se vuelve a correr `crear_base.sql`. Una base creada antes de que los umbrales fueran configurables se actualiza, sin borrar nada, con `agregar_umbrales.sql`.
4. Elegir `ProyectoFinal-PPII-G6` como proyecto de inicio y **Windows Machine** como destino, y ejecutar.

La conexión se toma de la variable de entorno `PPII_DB`. Si no existe, se usa `Server=.\SQLEXPRESS` con autenticación de Windows.

Para correr las pruebas: *Test Explorer* en Visual Studio, o `dotnet test ProyectoFinal-PPII-G6.Tests` desde la raíz del repositorio.

## Cómo trabajamos

- `main` es la rama que se evalúa: tiene que compilar y pasar las pruebas siempre.
- Una rama por tarea (`feat/...`, `fix/...`, `docs/...`) y se integra a `main` con un pull request con las pruebas de GitHub Actions en verde.
- Las versiones se marcan con etiquetas (`v0.1.0-demo`, `v0.2.0`, `v0.3.0`, `v0.4.0`...).
- Cuando trabajamos en pareja, el commit lleva una línea `Co-authored-by: Nombre Apellido <email>` por cada compañero.
- El backlog, las minutas y los accionables están en Trello.

## 👥 Equipo de Desarrollo — Práctica Profesional II (Grupo 6)

```text
Integrantes/
├── Gianluca Zarrelli
├── Maia Celeste Mazza
├── Facundo Araujo
└── Gabriel Bergamini
```

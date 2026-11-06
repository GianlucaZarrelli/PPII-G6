# Arquitectura actual

Describe el sistema **como está construido hoy** (última revisión: 6/11/2026). Si el código cambia, este documento se actualiza.

## Contexto

```text
  ┌───────────────┐      usa      ┌────────────────────────────┐   guarda en   ┌────────────┐
  │     Tutor     │ ────────────► │  Seguimiento de cursada    │ ────────────► │ SQL Server │
  │ (una sola     │               │  (aplicación .NET MAUI)    │               └────────────┘
  │  persona)     │               └────────────────────────────┘
  └───────────────┘
```

Una sola persona opera el sistema y no hay integraciones con otros sistemas.

## Componentes

```text
  ┌──────────────────────────────────────────────────────────────┐
  │  Pantallas                                                   │
  │  Views (XAML) ──► ViewModels          Presentacion           │
  └───────────────────────────────┬──────────────────────────────┘
                                  │ usan
  ┌───────────────────────────────▼──────────────────────────────┐
  │  Dominio (carpetas Models y Services)                        │
  │  Entidades · Reglas: riesgo, seguimiento, calendario,        │
  │  validaciones                                                │
  │  Interfaces: IDataService, IDialogoService,                  │
  │              INavegacion, ITema                              │
  └───────────────────────────────▲──────────────────────────────┘
                                  │ implementan
  ┌───────────────────────────────┴──────────────────────────────┐
  │  Infraestructura                                             │
  │  SqlServerDataService ──► AppDbContext ──► SQL Server        │
  └──────────────────────────────────────────────────────────────┘
```

Las flechas son la decisión de arquitectura más importante: **el dominio no conoce ni las pantallas ni la base de datos**. Las pantallas y la infraestructura dependen del dominio, y nunca al revés. `Presentacion` también implementa interfaces del dominio: `DialogoService` (avisos), `NavegacionShell` (navegación) y `TemaApp` (tema claro u oscuro).

## Qué hay en cada carpeta

| Carpeta | Contenido | Puede usar MAUI o Entity Framework |
|---|---|---|
| `Models` | Entidades: `Estudiante`, `Comision`, `Clase`, `Asistencia`, `TrabajoPractico`, `Entrega`, `Contacto`, `UmbralesRiesgo` | No |
| `Services` | Reglas: `RiesgoService`, `SeguimientoService`, `ConsultaSeguimiento`, `CalendarioComision`, validaciones, lectura de CSV, e interfaces | No |
| `Presentacion` | Textos y colores de cada nivel, medio y estado; `DialogoService`, `NavegacionShell`, `TemaApp` y las rutas | MAUI, sí |
| `ViewModels` | Lógica de cada pantalla | MAUI (`BindableObject`), sí |
| `Views` | Pantallas en XAML | MAUI, sí |
| `Infraestructura` | `SqlServerDataService` y `AppDbContext` | Entity Framework, sí |

## Cómo se verifica la regla de dependencias

- Una prueba automática (`DependenciasDelDominioTests`) revisa que ningún tipo de `Models` ni de `Services` use Entity Framework ni MAUI.
- Se puede comprobar también con una búsqueda de texto: `using Microsoft.Maui` y `using Microsoft.EntityFrameworkCore` no aparecen en esas dos carpetas.
- Ningún ViewModel crea un acceso a datos: lo recibe por el constructor (inyección de dependencias, registrada en `MauiProgram.cs`).
- El código de cada pantalla (`.xaml.cs`) solo crea la página, le asigna su ViewModel y le avisa que apareció. No tiene controladores de eventos: la navegación, el cambio de tema y los toques se conectan por comandos del ViewModel.
- GitHub Actions compila y corre todas las pruebas en cada cambio.

## Dos casos de uso de punta a punta

**Registrar un contacto y verlo en el panel**

1. `ContactosPage.xaml` muestra el formulario y se conecta a `ContactosViewModel` por bindings.
2. `ContactosViewModel` pide el primer problema del formulario a `ValidacionContacto` (dominio).
3. Si está bien, llama a `IDataService.SaveContactoAsync`, que implementa `SqlServerDataService`.
4. `SqlServerDataService` guarda en la tabla `Contactos` a través de `AppDbContext`.
5. Al abrir el panel, `PanelRiesgoViewModel` pide la comisión a `ConsultaSeguimiento`, que reúne los datos y le pide el cálculo a `SeguimientoService` y `RiesgoService`.
6. El resumen de cada alumno incluye el último contacto, y `PresentacionContacto` decide el texto y el color con que se muestra.
7. Al tocar un alumno, su comando anota cuál abrir y le pide a `INavegacion` ir al Historial.

**Cambiar los umbrales y ver el efecto**

1. `UmbralesPage.xaml` muestra los cuatro porcentajes y se conecta a `UmbralesViewModel`.
2. Al guardar, el ViewModel lee los cuatro campos y le pide el primer problema a `ValidacionUmbrales` (dominio): tienen que ser porcentajes de 0 a 100 y, en cada señal, el de alto riesgo menor que el de riesgo moderado.
3. Si está bien, llama a `IDataService.SaveUmbralesAsync`, que guarda la fila única de la tabla `UmbralesRiesgo`.
4. El ViewModel vuelve a evaluar a todos los alumnos con `ConsultaSeguimiento` y muestra cuántos quedan en cada nivel.
5. `ConsultaSeguimiento` lee los umbrales con `IDataService.GetUmbralesAsync` cada vez que evalúa y se los pasa a `SeguimientoService` y `RiesgoService`. Como el panel, Inicio y la ficha usan la misma consulta, todos muestran los niveles nuevos en su siguiente carga, sin reiniciar la aplicación.

## Base de datos

Tablas: `Comisiones`, `Estudiantes`, `Clases`, `TrabajosPracticos`, `Asistencias`, `Entregas`, `Contactos` y `UmbralesRiesgo`, esta última con una sola fila. Los scripts están en `ProyectoFinal-PPII-G6/scripts/`. El nivel de riesgo no se guarda: se calcula cada vez, con los umbrales guardados en ese momento.

## Pruebas

- **Unitarias (xUnit):** reglas de negocio, validaciones, textos y flujo de los ViewModels, con servicios falsos: una base en memoria (`DatosEnMemoria`), avisos, navegación y tema falsos.
- **De integración y de aceptación:** pendientes (sprint 8).

## Limitaciones conocidas

- Es un único proyecto de MAUI: el dominio se separa por carpetas y por una prueba, no por referencias entre proyectos.
- Las entregas se consultan alumno por alumno; para comisiones grandes convendría una sola consulta.
- Si una consulta a la base falla, algunas pantallas, como el panel y Inicio, muestran la lista vacía y escriben el error en el registro de depuración, sin avisarle al usuario.
- Los umbrales son únicos para todo el sistema: no se pueden definir distintos por comisión.
- Una clase cargada en el calendario no se puede editar ni borrar desde la aplicación.

# Especificación complementaria: requerimientos no funcionales

Reúne los requerimientos no funcionales del sistema, escritos una sola vez y como medidas que se pueden comprobar. Las historias de usuario los referencian por su número (por ejemplo, H7 referencia RNF-1). Última revisión: 6/11/2026.

Cada requerimiento sigue la forma *en [situación], cuando [estímulo], el sistema [respuesta] en [medida]*, y se clasifica con la grilla FURPS+. **Atributo elegido** es un nivel que el equipo decidió alcanzar; **restricción** es algo que viene impuesto por el entorno.

| N.º | Categoría | Tipo | Requerimiento | Cómo se verifica | Estado |
|---|---|---|---|---|---|
| RNF-1 | Soporte: configurabilidad | Atributo elegido (cambio de requerimiento de la clase 9, H7) | Cuando el tutor guarda otros umbrales de riesgo desde la pantalla Umbrales, el sistema evalúa a todos los alumnos con los valores nuevos en la siguiente carga del panel, de la ficha y de Inicio, sin reiniciar ni recompilar. | Prueba automática `ConsultaSeguimiento_AlGuardarOtrosUmbrales_RecalculaDeInmediato`, y probando la pantalla con los datos de demostración. | Verificado |
| RNF-2 | Confiabilidad | Atributo elegido | Si falla la conexión con la base de datos al guardar un contacto, una comisión o los umbrales, el sistema avisa que no se pudo guardar y conserva lo escrito en el formulario. | Pruebas automáticas `Guardar_ConLaBaseCaida_AvisaElErrorYConservaLoEscrito` (contactos y umbrales) y `CrearComision_ConLaBaseCaida_AvisaElErrorYConservaLoEscrito`. | Verificado para los tres casos. La carga de clases tiene el mismo comportamiento, sin prueba automática |
| RNF-3 | Rendimiento | Atributo elegido | Con 60 alumnos repartidos en dos comisiones, cuando el tutor abre el panel de riesgo de una comisión, el sistema lo presenta completo en menos de 3 segundos. | Cronometrar la apertura del panel con los datos masivos cargados. | Pendiente |
| RNF-4 | Usabilidad | Atributo elegido | Cuando una persona que nunca usó el sistema toma la asistencia de una clase de 12 alumnos, la registra en menos de 2 minutos y sin ayuda. | Prueba cronometrada con un compañero que no haya usado la aplicación. | Pendiente |
| RNF-5 | Soporte: mantenibilidad | Atributo elegido (observación de la devolución de la demo) | Ningún método del sistema supera las 20 líneas de código ejecutable ni una complejidad ciclomática de 10. | "Calcular métricas de código" de Visual Studio (líneas de código ejecutable y complejidad ciclomática). | Verificado el 6/11/2026: líneas en toda la aplicación y complejidad en `Models`, `Services`, `ViewModels` y `Presentacion`. Falta confirmar `Infraestructura` y las pantallas en Visual Studio |
| RNF-6 | Restricción de implementación | Restricción | La aplicación se ejecuta en Windows y necesita una base SQL Server accesible; la conexión se toma de la variable de entorno `PPII_DB`. | GitHub Actions compila y corre las pruebas en Windows en cada cambio; los pasos están en el README. | Verificado |
| RNF-7 | Funcionalidad: datos personales | Restricción impuesta (Ley 25.326) | Cuando se carga o se importa un estudiante, el sistema guarda solo los datos necesarios para el seguimiento: DNI, nombre, apellido, email, comisión y fecha de alta. | Revisar la entidad `Estudiante` y la tabla `Estudiantes`. | Parcial: se cumple en los datos que se guardan. **No hay control de acceso en la aplicación ni un plazo de conservación definido** |

## Limitaciones conocidas de esta especificación

- La aplicación no tiene autenticación: se instala en el equipo del tutor y el acceso a los datos se limita con el usuario de la base. Es una limitación del alcance, no una decisión de que el control de acceso no importe.
- Los requerimientos RNF-3 y RNF-4 están formulados pero todavía no medidos. Cuando se midan, se registra el resultado en la columna de estado.

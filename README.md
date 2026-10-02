# Sistema de Seguimiento de Cursada y Alerta Temprana — Grupo 6 (PPII)

Aplicación desarrollada en **.NET MAUI** bajo el patrón de arquitectura **MVVM**. El proyecto tiene como objetivo brindar a los docentes una herramienta ágil para el seguimiento de la cursada y la detección temprana de alumnos en riesgo pedagógico, respondiendo a la pregunta central del sistema: **"¿A quién llamo esta semana?"**.

---

## 🚀 Características Principales

El sistema centraliza la gestión académica enfocándose en la prevención del abandono escolar a través de los siguientes módulos funcionales:

* **Gestión de Estudiantes:** Registro, visualización y administración de la nómina de alumnos vinculados a la cursada.
* **Control de Asistencias:** Interfaz rápida para la carga de registros de clases, presentes e inasistencias.
* **Motor de Indicador de Riesgo:** Lógica de negocio automatizada que evalúa en tiempo real el porcentaje acumulado de inasistencias de cada estudiante.
* **Panel de Alerta Temprana (Dashboard):** Vista centralizada que prioriza y resalta de forma visual a los alumnos con ausentismo crítico, facilitando la intervención y el contacto oportuno por parte del docente.

---

## 📂 Estructura del Proyecto

El código está organizado siguiendo la separación de responsabilidades propia del patrón **MVVM** y la inyección de dependencias para el acceso a datos:

    ProyectoFinal-PPII-G6/
    ├── Models/              # Entidades del dominio puras (Estudiante, Clase, Asistencia)
    ├── ViewModels/          # Lógica de presentación, notificaciones y comandos
    ├── Views/               # Vistas e interfaces de usuario en XAML
    └── Services/            # Servicios de negocio (CalculadorRiesgo) y capa de persistencia (IDataService)

---

## 🛠️ Tecnologías y Base de Datos

* **Framework:** .NET MAUI
* **Lenguaje:** C# 10+
* **Arquitectura:** MVVM (Model-View-ViewModel)
* **Motor de Base de Datos:** Microsoft SQL Server
* **Lógica de Persistencia:** Esquema relacional desacoplado mediante `IDataService`
* **Paquetes clave:**
  * `CommunityToolkit.Mvvm`: Implementación de notificaciones de cambios de propiedades y comandos.
  * `Microsoft.Data.SqlClient`: Conector oficial para comunicación con SQL Server.

---

## 📋 Requisitos para Ejecutar el Proyecto

1. **Visual Studio 2022** con la carga de trabajo de **Desarrollo de .NET MAUI** instalada.
2. **Microsoft SQL Server** (LocalDB, Express o la instancia definida en la cadena de conexión) con el script de base de datos `SistemaAlertaTemprana` ejecutado.
3. Clonar el repositorio mediante la terminal o tu cliente Git favorito:

   ```bash
   git clone https://github.com/GianlucaZarrelli/PPII-G6.git
4. Abrir la solución ProyectoFinal-PPII-G6.sln en Visual Studio.
5. Seleccionar el destino de ejecución (recomendado: Windows Machine) y presionar F5.

## 👥 Equipo de Desarrollo — Práctica Profesional II (Grupo 6)

```text
Integrantes/
├── Gianluca Zarrelli     (G.zarrelli96@gmail.com)
├── Maia Celeste Mazza    (Maiacmazza@gmail.com)
├── Facundo Araujo        (Somonte.facundo2010@gmail.com)
└── Gabriel Bergamini     (Gabrieleze90@gmail.com)

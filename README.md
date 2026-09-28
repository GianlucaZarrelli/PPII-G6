# PPII-G6
# Sistema de Seguimiento de Cursada y Alerta Temprana — Grupo 6 (PPII)

Aplicación desarrollada en **.NET MAUI** bajo el patrón de arquitectura **MVVM**. El proyecto tiene como objetivo brindar a los docentes una herramienta ágil para el seguimiento de la cursada y la detección temprana de alumnos en riesgo pedagógico, respondiendo a la pregunta central del sistema: **"¿A quién llamo esta semana?"**.

---

## 🚀 Alcance del Sprint 2 (Esqueleto Mínimo)

En este Sprint se implementan las Historias de Usuario prioritarias para resolver el indicador de riesgo antes que la infraestructura compleja:

* **H1 - Nómina de Estudiantes:** Registro y visualización de los datos básicos de los alumnos.
* **H3 - Registro de Asistencia:** Carga periódica de la asistencia por clase.
* **H6 - Indicador de Riesgo:** Motor de reglas que evalúa las inasistencias acumuladas por estudiante.
* **H8 - Panel de Alerta Temprana:** Vista priorizada que ordena a los estudiantes según su nivel de riesgo.

---

## 📂 Estructura del Proyecto

El código está organizado siguiendo la separación de responsabilidades propia del patrón **MVVM**:

    ProyectoFinal-PPII-G6/
    ├── Models/              # Entidades del dominio puras (Estudiante, Clase, Asistencia)
    ├── ViewModels/          # Lógica de presentación, notificaciones y comandos
    ├── Views/               # Vistas e interfaz de usuario en XAML
    └── Services/            # Servicios de negocio (CalculadorRiesgo) y persistencia SQLite

---

## 🛠️ Tecnologías y Paquetes NuGet

* **Framework:** .NET MAUI
* **Lenguaje:** C#
* **Patrón:** MVVM (Model-View-ViewModel)
* **Paquetes clave:**
  * `CommunityToolkit.Mvvm`: Generación de notificaciones de cambios de propiedades y comandos.
  * `sqlite-net-pcl`: Motor de base de datos relacional local en el dispositivo.

---

## 📋 Requisitos para Ejecutar el Proyecto

1. **Visual Studio 2022** con la carga de trabajo de **Desarrollo de .NET MAUI** instalada.
2. Clonar el repositorio:

       git clone https://github.com/GianlucaZarrelli/PPII-G6.git

3. Abrir la solución `ProyectoFinal-PPII-G6.sln` en Visual Studio.
4. Seleccionar el destino de ejecución (**Windows Machine** o **Emulador Android**) y presionar `F5`.

---

## 👥 Equipo de Desarrollo — Práctica Profesional II (Grupo 6)

```text
Integrantes/
├── Gianluca Zarrelli     (G.zarrelli96@gmail.com)
├── Maia Celeste Mazza    (Maiacmazza@gmail.com)
├── Facundo Araujo        (Somonte.facundo2010@gmail.com)
└── Gabriel Bergamini     (Gabrieleze90@gmail.com)

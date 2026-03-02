# 📜 Changelog

Todos los cambios notables en este proyecto serán documentados en este archivo siguiendo un estándar **Premium** de visualización.

---

## v1.1.3 - 2026-03-02

### ✨ Añadido (Validación)

- **✅ Validación en tiempo real**: Las rutas de proyecto/publicación/ISCC.exe ahora muestran feedback visual inmediato (borde verde/amarillo/rojo).
- **⚠️ Advertencia .csproj**: Se muestra aviso si la carpeta del proyecto no contiene un archivo `.csproj`.
- **🧵 Validación asíncrona**: La validación se ejecuta en segundo plano para no bloquear la interfaz.

---

## v1.1.2 - 2026-03-02

### ✨ Añadido (Logs)

- **📋 Logs persistentes**: El historial de logs ahora se guarda en disco en `%AppData%\Instalador\logs\instalador_YYYYMMDD.log`.
- **🔁 Carga automática**: Al iniciar la aplicación se cargan los logs existentes del día.
- **🧯 Rotación básica**: Si el archivo supera cierto tamaño, se rota a `.bak` manteniendo un número limitado de respaldos.

### 🛠️ Mejorado (v1.1.2)

- **💾 Guardado seguro**: Se guarda automáticamente al primer log, cada cierto número de entradas y al cerrar la aplicación.

---

## v1.1.1 - 2026-03-02

### ✨ Añadido (Control de Compilación)

- **🔄 Selector Release/Debug**: Implementación completa del selector dinámico para alternar entre configuraciones de compilación Release y Debug en tiempo real.
- **📡 Integración con BuildService**: El ComboBox ahora controla directamente el parámetro `-c` del comando `dotnet publish`.
- **📊 Feedback Visual**: El log muestra la configuración seleccionada: "Publicando proyecto (Release)..." o "Publicando proyecto (Debug)...".

### 🛠️ Mejorado (v1.1.1)

- **🔧 Binding de Datos**: Conectado el ComboBox `ComboConfig` con la propiedad `BuildConfiguration` usando `SelectedValue` y `SelectedValuePath="Tag"`.
- **⚙️ Arquitectura Extendida**: Modificada la interfaz `IBuildService` para aceptar configuración dinámica con valor por defecto "Release".

### 🐛 Corregido (v1.1.1)

- **🎯 Configuración Dinámica**: Resuelto el hardcodeo de "Release" en `BuildService.RunPublishAsync()` para permitir cambio en tiempo de ejecución.

---

## v1.1.0 - 2026-02-28

### ✨ Añadido (Power User Update)

- **🔔 Notificaciones Nativa**: Implementación de avisos Toast de Windows al finalizar tareas (Build, Publish, Installer).
- **🔍 Auto-detección Inteligente**: El sistema ahora localiza automáticamente el compilador `ISCC.exe` de Inno Setup.
- **⚡ Publicación Avanzada**: Nuevas opciones en UI para configurar `ReadyToRun`, `Trimmed` y `Compression`.
- **✅ Validación en Tiempo Real**: Los campos de configuración validan la existencia de rutas instantáneamente.
- **💎 Refactorización MVVM**: Estructura modular completa (Models, Views, ViewModels, Services).

---

## v1.0.5 - 2026-02-28

### ✨ Añadido (v1.0.5)

- **🔗 Integración con Git**: Nuevo panel en la barra de estado que muestra la rama actual y alerta de cambios pendientes con un código de colores inteligente (Verde/Naranja).
- **❓ Menú de Ayuda Profesional**: Nueva sección en la barra superior con Manual de Uso rápido, enlace al perfil del Autor y créditos del sistema "Acerca de".
- **🔄 Refresco Manual de Git**: Posibilidad de actualizar instantáneamente el estado del repositorio haciendo clic en el panel inferior.

### 🛠️ Mejorado (v1.0.5)

- **💎 Refactorización de Build**: Resolución de ambigüedades técnicas y limpieza profunda de errores de compilación XAML/C# para un despliegue sin fallos.
- **🎨 UI Modernizada**: Rediseño completo de la barra inferior siguiendo estándares de Visual Studio y VS Code.

---

## v1.0.4 - 2026-02-28

### ✨ Añadido (v1.0.4)

- **📂 Gestión de Proyectos Recientes**: Nuevo selector inteligente en la barra superior para alternar instantáneamente entre diferentes proyectos configurados.
- **🏗️ Arquitectura Multi-Perfil**: El sistema ahora guarda una lista de configuraciones independientes para cada proyecto en `config.json`.
- **➕ Nuevo Botón en Configuración**: Añadido botón "NUEVO PROYECTO" para crear perfiles desde cero sin borrar los anteriores.
- **🤖 Migración Automática**: Lógica inteligente que detecta configuraciones de versiones anteriores y las migra al nuevo formato sin pérdida de datos.

### 🛠️ Mejorado (v1.0.4)

- **🧹 Limpieza de Código**: Eliminación total de advertencias de compilación para un "Clean Build" perfecto.
- **📡 Consola Dinámica**: Feedback visual mejorado al cambiar de proyecto en la consola de log.

---

## v1.0.3 - 2026-02-28

### ✨ Añadido (v1.0.3)

- **🔍 Auto-detección de Inno Setup**: El programa ahora busca automáticamente `ISCC.exe` en rutas estándar de Windows para facilitar la configuración inicial.
- **🎨 Validación en Tiempo Real**: Los campos de ruta ahora muestran un borde **Verde** (Válido) o **Rojo** (Inválido) dinámicamente.
- **📋 Sistema de Logs Persistentes**: Registro automático de toda la actividad en archivos `.txt` dentro de la carpeta `/logs`.
- **🖼️ Iconografía Robusta**: Implementación de Pack URIs para asegurar que los iconos se carguen correctamente en cualquier entorno.

### 🛠️ Mejorado (v1.0.3)

- **💎 Estabilidad Visual**: Mejor manejo de excepciones durante la carga de recursos y estilos.
- **📈 UX en Configuración**: Mejoras en la navegación y validación de la ventana de ajustes.

---

## v1.0.2 - 2026-02-28

### ✨ Añadido (v1.0.2)

- **⚙️ Ventana de Configuración Premium**: Rediseño total con iconografía vectorial, mejores márgenes y espaciado elegante.
- **🔘 Selector de Perfiles con Iconos**: Inclusión de iconos visuales (Play/Gear) para una identificación rápida de Release/Debug.
- **🔵 Estilos de Botones Acentuados**: Implementación de `AccentButton` y `SecondaryButton` para una jerarquía visual clara.
- **🆔 Identidad Visual**: Asignación de icono personalizado (`Installer.ico`) a todas las ventanas del sistema.

### 🐛 Corregido (v1.0.2)

- **⚡ Error de Crash Crítico**: Solucionado el cierre inesperado por recursos de texto faltantes.
- **🌓 Contraste en ComboBox**: Corregido el fondo blanco forzando un estilo oscuro en todos los desplegables.
- **🔍 Advertencias de Código**: Limpieza de lógica asíncrona innecesaria para un flujo más predecible.

---

## v1.0.0 - 2026-02-28

### ✨ Añadido (v1.0.0)

- **🚀 Lanzamiento Inicial**: Migración completa a arquitectura **WPF** sobre **.NET 8**.
- **🌑 Tema Deep Charcoal**: Interfaz moderna con modo oscuro nativo y micro-animaciones.
- **📐 Iconografía SVG**: Sistema centralizado de iconos vectoriales escalables en `App.xaml`.
- **📦 Integración Inno Setup**: Motor nativo para la generación automatizada de instaladores profesionales.
- **📖 Documentación Pro**: Creación del manual técnico y guía de inicio en `README.md`.

### 🐛 Corregido

- **👁️ Visibilidad en Menús**: Corrección de texto blanco sobre fondo claro en menús estándar de Windows.
- **🏗️ Superposición de Layout**: Ajustes en los encabezados del menú principal para evitar colisiones de elementos.

### 🔄 Cambiado (v1.0.0)

- **🧩 Refactorización Core**: Reestructuración de la lógica de negocio para una mayor modularidad y mantenibilidad.

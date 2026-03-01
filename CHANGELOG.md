# 📜 Changelog

Todos los cambios notables en este proyecto serán documentados en este archivo siguiendo un estándar **Premium** de visualización.

---

## v1.1.1 - 2026-03-01

### ✨ Añadido (Automatización)

- **Auto-rellenado Inteligente**: Al seleccionar la carpeta del proyecto, el sistema extrae automáticamente el **Nombre del Proyecto** y configura el **Directorio de Publicación** (`/publish`) por defecto.

### 🐛 Corregido

- **Persistencia de Proyectos**: Asegurado el guardado correcto de nuevos proyectos al cerrar la ventana de configuración.
- **Empaquetado Universal**: El Instalador ahora copia automáticamente carpetas estáticas como `img` y archivos como `README.md` a la carpeta de publicación independientemente del `.csproj` del proyecto destino. Además, genera correctamente tanto el `.zip` Portable como el `.zip` Single-File tal y como específica el flujo de automatización de Inno Setup.
- **Botones de menú ZIP Separados**: Resolvimos un error donde los botones "Crear ZIP Portable" y "Crear ZIP Single-File" en la interfaz disparaban la misma función y creaban ambos archivos simultáneamente. Ahora cada opción generará específicamente el fichero que indica.
- **Rutas Absolutas de Publicación**: Se arregló un bug donde operaciones de Limpieza, ZIP o InnoSetup en C# usaban la ruta parcial interna del programa (ej: `publish`) pensando que estaba vacía en vez de localizar la carpeta de publicación auténtica adscrita al directorio del proyecto seleccionado.
- **Crash Fix de Archivo Único (.exe)**: Se subsanó un conflicto en `.csproj` donde la carpeta `img\**` generaba una doble asignación para los archivos WPF (`Resource` vs `Content`), lo que ocasionaba que el ejecutable "Single-File" de la aplicación hiciera _crash_ instantáneo al iniciar por no poder descifrar los símbolos XAML del icono en memoria.
- **Sincronización de UI**: Los campos de ruta ahora se actualizan instantáneamente al elegir una carpeta mediante el diálogo de selección (implementado `INotifyPropertyChanged`).

## v1.1.0 - 2026-02-28

### ✨ Añadido (Power User Update)

- **🔔 Notificaciones Nativa**: Implementación de avisos Toast de Windows al finalizar tareas (Build, Publish, Installer).
- **🔍 Auto-detección Inteligente**: El sistema ahora localiza automáticamente el compilador `ISCC.exe` de Inno Setup.
- **⚡ Publicación Avanzada**: Nuevas opciones en UI para configurar `ReadyToRun`, `Trimmed` y `Compression`.
- **✅ Validación en Tiempo Real**: Los campos de configuración validan la existencia de rutas instantáneamente.
- **💎 Refactorización MVVM**: Estructura modular completa (Models, Views, ViewModels, Services).

---

## v1.0.5 - 2026-02-28

### ✨ Añadido

- **🔗 Integración con Git**: Nuevo panel en la barra de estado que muestra la rama actual y alerta de cambios pendientes con un código de colores inteligente (Verde/Naranja).
- **❓ Menú de Ayuda Profesional**: Nueva sección en la barra superior con Manual de Uso rápido, enlace al perfil del Autor y créditos del sistema "Acerca de".
- **🔄 Refresco Manual de Git**: Posibilidad de actualizar instantáneamente el estado del repositorio haciendo clic en el panel inferior.

### 🛠️ Mejorado

- **💎 Refactorización de Build**: Resolución de ambigüedades técnicas y limpieza profunda de errores de compilación XAML/C# para un despliegue sin fallos.
- **🎨 UI Modernizada**: Rediseño completo de la barra inferior siguiendo estándares de Visual Studio y VS Code.

---

## v1.0.4 - 2026-02-28

### ✨ Añadido

- **📂 Gestión de Proyectos Recientes**: Nuevo selector inteligente en la barra superior para alternar instantáneamente entre diferentes proyectos configurados.
- **🏗️ Arquitectura Multi-Perfil**: El sistema ahora guarda una lista de configuraciones independientes para cada proyecto en `config.json`.
- **➕ Nuevo Botón en Configuración**: Añadido botón "NUEVO PROYECTO" para crear perfiles desde cero sin borrar los anteriores.
- **🤖 Migración Automática**: Lógica inteligente que detecta configuraciones de versiones anteriores y las migra al nuevo formato sin pérdida de datos.

### 🛠️ Mejorado

- **🧹 Limpieza de Código**: Eliminación total de advertencias de compilación para un "Clean Build" perfecto.
- **📡 Consola Dinámica**: Feedback visual mejorado al cambiar de proyecto en la consola de log.

---

## v1.0.3 - 2026-02-28

### ✨ Añadido

- **🔍 Auto-detección de Inno Setup**: El programa ahora busca automáticamente `ISCC.exe` en rutas estándar de Windows para facilitar la configuración inicial.
- **🎨 Validación en Tiempo Real**: Los campos de ruta ahora muestran un borde **Verde** (Válido) o **Rojo** (Inválido) dinámicamente.
- **📋 Sistema de Logs Persistentes**: Registro automático de toda la actividad en archivos `.txt` dentro de la carpeta `/logs`.
- **🖼️ Iconografía Robusta**: Implementación de Pack URIs para asegurar que los iconos se carguen correctamente en cualquier entorno.

### 🛠️ Mejorado

- **💎 Estabilidad Visual**: Mejor manejo de excepciones durante la carga de recursos y estilos.
- **📈 UX en Configuración**: Mejoras en la navegación y validación de la ventana de ajustes.

---

## v1.0.2 - 2026-02-28

### ✨ Añadido

- **⚙️ Ventana de Configuración Premium**: Rediseño total con iconografía vectorial, mejores márgenes y espaciado elegante.
- **🔘 Selector de Perfiles con Iconos**: Inclusión de iconos visuales (Play/Gear) para una identificación rápida de Release/Debug.
- **🔵 Estilos de Botones Acentuados**: Implementación de `AccentButton` y `SecondaryButton` para una jerarquía visual clara.
- **🆔 Identidad Visual**: Asignación de icono personalizado (`Installer.ico`) a todas las ventanas del sistema.

### 🐛 Corregido

- **⚡ Error de Crash Crítico**: Solucionado el cierre inesperado por recursos de texto faltantes.
- **🌓 Contraste en ComboBox**: Corregido el fondo blanco forzando un estilo oscuro en todos los desplegables.
- **🔍 Advertencias de Código**: Limpieza de lógica asíncrona innecesaria para un flujo más predecible.

---

## v1.0.0 - 2026-02-28

### ✨ Añadido

- **🚀 Lanzamiento Inicial**: Migración completa a arquitectura **WPF** sobre **.NET 8**.
- **🌑 Tema Deep Charcoal**: Interfaz moderna con modo oscuro nativo y micro-animaciones.
- **📐 Iconografía SVG**: Sistema centralizado de iconos vectoriales escalables en `App.xaml`.
- **📦 Integración Inno Setup**: Motor nativo para la generación automatizada de instaladores profesionales.
- **📖 Documentación Pro**: Creación del manual técnico y guía de inicio en `README.md`.

### 🐛 Corregido

- **👁️ Visibilidad en Menús**: Corrección de texto blanco sobre fondo claro en menús estándar de Windows.
- **🏗️ Superposición de Layout**: Ajustes en los encabezados del menú principal para evitar colisiones de elementos.

### 🔄 Cambiado

- **🧩 Refactorización Core**: Reestructuración de la lógica de negocio para una mayor modularidad y mantenibilidad.

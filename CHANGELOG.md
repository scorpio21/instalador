# 📜 Changelog

Todos los cambios notables en este proyecto serán documentados en este archivo siguiendo un estándar **Premium** de visualización.

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

# Changelog

Todos los cambios notables en este proyecto serán documentados en este archivo.

El formato está basado en [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
y este proyecto se adhiere a [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [v1.0.4] - 2026-02-28

### Añadido

- **Gestión de Proyectos Recientes**: Nuevo selector en la barra superior para alternar instantáneamente entre diferentes proyectos configurados.
- **Arquitectura Multi-Perfil**: El sistema ahora guarda una lista de configuraciones independientes para cada proyecto en `config.json`.
- **Nuevo Botón en Configuración**: Añadido botón "NUEVO PROYECTO" para crear perfiles desde cero sin borrar los anteriores.
- **Migración Automática**: Lógica inteligente que detecta configuraciones de versiones anteriores y las migra al nuevo formato sin pérdida de datos.

### Mejorado

- Limpieza total de advertencias de compilación (Clean Build).
- Feedback visual al cambiar de proyecto en la consola.

## v1.0.3 - 2026-02-28

### Añadido

- **Auto-detección de Inno Setup**: El programa ahora busca automáticamente `ISCC.exe` en rutas estándar de Windows.
- **Validación en Tiempo Real**: Los campos de ruta en la ventana de configuración ahora muestran un borde **Verde** (Válido) o **Rojo** (Inválido) mientras escribes.
- **Sistema de Logs Persistentes**: Toda la actividad del programa se guarda ahora automáticamente en archivos `.txt` dentro de la carpeta `/logs`.
- **Iconografía Profesional**: Los iconos de las ventanas ahora usan un sistema de carga robusto (Pack URIs) para evitar fallos de inicio.

### Mejorado

- Estabilidad general y manejo de excepciones en la carga de recursos.
- Feedback visual en la ventana de configuración.

## [v1.0.2] - 2026-02-28

### Añadido

- **Ventana de Configuración Premium**: Rediseño total con iconografía vectorial, mejores márgenes y espaciado.
- **Selector de Perfiles con Iconos**: Inclusión de iconos visuales (Play/Gear) en el selector de Release/Debug para una identificación rápida.
- **Estilos de Botones Acentuados**: Implementación de `AccentButton` (azul) y `SecondaryButton` (gris) para una jerarquía visual clara.
- **Icono de Aplicación**: Asignación de icono personalizado (`Installer.ico`) a las ventanas principal y de configuración.
- **Validación de Datos**: Sistema de alertas para evitar guardar configuraciones con rutas inexistentes o campos vacíos.

### Corregido

- **Error de Crash Crítico**: Solucionado el cierre inesperado de la aplicación por el recurso `TextLight` faltante.
- **Problema de Contraste en ComboBox**: Corregido el fondo blanco forzando un `ControlTemplate` oscuro en todos los desplegables.
- **Error de Compilación XAML**: Eliminación de propiedades no soportadas (`LetterSpacing`) que impedían el build.
- **Advertencias de Código**: Limpieza de advertencias `async` innecesarias en `MainWindow.xaml.cs`.

## [v1.0.0] - 2026-02-28

### Añadido

- Migración completa a arquitectura WPF en .NET 8.
- Interfaz Premium con Modo Oscuro "Deep Charcoal".
- Sistema de iconos vectoriales SVG integrados en `App.xaml`.
- ControlTemplate personalizado para `MenuItem` que soluciona problemas de visibilidad en submenús.
- Soporte para iconos en todos los niveles del menú (principales, submenús y acciones internas).
- Integración automática con Inno Setup para generación de instaladores.
- Documentación técnica profesional en `README.md`.

### Corregido

- Error de visibilidad de texto blanco sobre fondo claro en menús de Windows.
- Superposición de iconos y texto en los encabezados del menú principal.
- Bloqueo de archivos durante la fase de compilación por procesos abiertos.

### Cambiado

- Refactorización de la lógica de negocio para mejorar la modularidad.
- Mejora de la consola de log con feedback visual de errores y éxito.

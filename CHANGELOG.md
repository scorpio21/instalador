# Changelog

Todos los cambios notables en este proyecto serán documentados en este archivo.

El formato está basado en [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
y este proyecto se adhiere a [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## v1.0.0 - 2026-02-28

### Added

- Migración completa a arquitectura WPF en .NET 8.
- Interfaz Premium con Modo Oscuro "Deep Charcoal".
- Sistema de iconos vectoriales SVG integrados en `App.xaml`.
- ControlTemplate personalizado para `MenuItem` que soluciona problemas de visibilidad en submenús.
- Soporte para iconos en todos los niveles del menú (principales, submenús y acciones internas).
- Integración automática con Inno Setup para generación de instaladores.
- Documentación técnica profesional en `README.md`.

### Fixed

- Error de visibilidad de texto blanco sobre fondo claro en menús de Windows.
- Superposición de iconos y texto en los encabezados del menú principal.
- Bloqueo de archivos durante la fase de compilación por procesos abiertos.

### Changed

- Refactorización de la lógica de negocio para mejorar la modularidad.
- Mejora de la consola de log con feedback visual de errores y éxito.

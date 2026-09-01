# StayEasy

Sistema de administración de hoteles y reservas desarrollado como Proyecto 5 de la cátedra **Desarrollo y Arquitectura de Software** (Ingeniería en Sistemas de Información).

Arquitectura en capas en **C# / .NET Framework** con **SQL Server**, aplicando patrones de diseño (Composite, Observer, Singleton), seguridad con hash de contraseñas, bitácora de auditoría e idioma dinámico.

---

## Tabla de contenidos

- [Dominio del sistema](#dominio-del-sistema)
- [Arquitectura](#arquitectura)
- [Estructura del repositorio](#estructura-del-repositorio)
- [Patrones de diseño aplicados](#patrones-de-diseño-aplicados)
- [Requisitos previos](#requisitos-previos)
- [Puesta en marcha](#puesta-en-marcha)
- [Roles del equipo](#roles-del-equipo)
- [Estado del proyecto](#estado-del-proyecto)
- [Licencia](#licencia)

---

## Dominio del sistema

StayEasy gestiona el ciclo de vida completo de una reserva hotelera:

- Alta y administración de habitaciones (estándar y suites), huéspedes y reservas.
- Check-in y check-out transaccional, con liberación/ocupación automática de habitaciones.
- Paquetes de estadía combinables (servicios simples y combos compuestos).
- Notificación automática al personal de limpieza al finalizar una estadía.
- Gestión de usuarios, roles y permisos del personal del hotel.
- Bitácora de auditoría de acciones críticas del sistema.
- Cambio de idioma dinámico (Español / Inglés) sin reiniciar la aplicación.
- Estadísticas de ocupación y consumos mediante gráficos y grillas parametrizadas.

## Arquitectura

Arquitectura multicapa estricta, separada en 5 proyectos independientes dentro de la misma solución:

```
UI (WPF)  →  BLL  →  DAL  →  MPP  →  BE
                                ↓
                          SQL Server (StayEasyDB)
```

| Capa | Responsabilidad | Referencia a |
|---|---|---|
| **BE** | Entidades de negocio puras (herencia, `IComparable`, `ICloneable`) | — (es la base) |
| **MPP** | Traducción entre filas de base de datos y objetos `BE` | BE |
| **DAL** | Persistencia en SQL Server (modo conectado/desconectado, transacciones, `IDisposable`) | BE, MPP |
| **BLL** | Reglas de negocio y validaciones | DAL, BE |
| **UI** | Interfaz Windows Forms (MDI, controles personalizados, `Chart`) | BLL, BE |

## Estructura del repositorio

```
StayEasy/
├── StayEasy.BE/                # Entidades de negocio
├── StayEasy.MPP/                # Mapeo BD ↔ entidades
├── StayEasy.DAL/                # Acceso a datos
├── StayEasy.BLL/                # Lógica de negocio
├── StayEasy.UI/                 # Interfaz WinForms
├── database/                    # Scripts SQL versionados (tablas, SPs, migraciones)
├── docs/                        # Diagramas, requisitos funcionales, planificación
├── LICENSE
└── README.md
```

## Patrones de diseño aplicados

- **Composite** — jerarquía de paquetes de estadía (`ServicioHotel`) y de permisos del sistema (`Patente`/familias de roles).
- **Observer** — notificación automática al panel de limpieza al registrarse un check-out.
- **Singleton** — sesión activa del usuario logueado (`GestorSesion`), con sus permisos resueltos.
- **Herencia y polimorfismo** — `Habitacion` como clase abstracta con `HabitacionEstandar` y `SuitePresidencial`.
- **IABM\<T\>** — interfaz genérica de CRUD compartida entre BLL y DAL.

## Requisitos previos

- Visual Studio 2022 (o superior) con carga de trabajo .NET Desktop.
- SQL Server 2019+ (o SQL Server Express) y SQL Server Management Studio.
- .NET Framework 4.8.

## Puesta en marcha

1. Clonar el repositorio.
2. Ejecutar el script consolidado en `database/StayEasyDB_Script.sql` sobre una instancia de SQL Server.
3. Configurar la cadena de conexión en `StayEasy.DAL` (`App.config` del proyecto `StayEasy.UI`).
4. Abrir `StayEasy.sln` en Visual Studio y establecer `StayEasy.UI` como proyecto de inicio.
5. Compilar y ejecutar.

## Roles del equipo

| Integrante | Capas a cargo |
|---|---|
| Juan Chiappa | Base de Datos, MPP, DAL |
| Liam Leguizamon | BLL, UI |
| Leandro Tabares | BLL, BE |

## Estado del proyecto

🚧 En desarrollo — proyecto académico 2026.

## Licencia

Este proyecto se distribuye bajo la licencia MIT. Ver [LICENSE](LICENSE) para más detalles.

USE [master]
GO
/****** Objeto: Database [StayEasyDB] Fecha de script: 14/09/2026 22:14:44 ******/
CREATE DATABASE [StayEasyDB]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'StayEasyDB', FILENAME = N'E:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\StayEasyDB.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'StayEasyDB_log', FILENAME = N'E:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\StayEasyDB_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [StayEasyDB] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [StayEasyDB].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [StayEasyDB] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [StayEasyDB] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [StayEasyDB] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [StayEasyDB] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [StayEasyDB] SET ARITHABORT OFF 
GO
ALTER DATABASE [StayEasyDB] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [StayEasyDB] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [StayEasyDB] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [StayEasyDB] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [StayEasyDB] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [StayEasyDB] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [StayEasyDB] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [StayEasyDB] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [StayEasyDB] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [StayEasyDB] SET  ENABLE_BROKER 
GO
ALTER DATABASE [StayEasyDB] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [StayEasyDB] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [StayEasyDB] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [StayEasyDB] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [StayEasyDB] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [StayEasyDB] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [StayEasyDB] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [StayEasyDB] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [StayEasyDB] SET  MULTI_USER 
GO
ALTER DATABASE [StayEasyDB] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [StayEasyDB] SET DB_CHAINING OFF 
GO
ALTER DATABASE [StayEasyDB] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [StayEasyDB] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [StayEasyDB] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [StayEasyDB] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [StayEasyDB] SET QUERY_STORE = ON
GO
ALTER DATABASE [StayEasyDB] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [StayEasyDB]
GO
/****** Objeto: Table [dbo].[Reserva] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Reserva](
	[ID_Reserva] [int] IDENTITY(1,1) NOT NULL,
	[HuespedID] [int] NOT NULL,
	[HabitacionID] [int] NOT NULL,
	[FechaCheckIn] [date] NOT NULL,
	[FechaCheckOut] [date] NOT NULL,
	[Estado] [varchar](20) NOT NULL,
	[Total] [decimal](18, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID_Reserva] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: UserDefinedFunction [dbo].[fn_ReservasEnConflicto] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* ============================================================================
   3. fn_ReservasEnConflicto
   ----------------------------------------------------------------------------
   Unica fuente de verdad de la regla de solapamiento. Es una funcion de tabla
   en linea (iTVF): el motor la incrusta en el plan de la consulta que la
   llama, asi que no tiene el costo de una funcion escalar fila por fila.

   Regla de solapamiento de dos rangos semiabiertos [In, Out):
        conflicto  <=>  A.In < B.Out  AND  A.Out > B.In

   Con el "<" y el ">" estrictos, un check-out y un check-in el mismo dia NO
   se pisan: la 204 puede salir el 10/03 y entrar otro huesped el 10/03.

   @ReservaExcluidaID sirve para editar una reserva existente sin que choque
   contra si misma. Pasar NULL cuando es un alta.
   ============================================================================ */
CREATE   FUNCTION [dbo].[fn_ReservasEnConflicto]
(
    @HabitacionID       INT,
    @FechaCheckIn       DATE,
    @FechaCheckOut      DATE,
    @ReservaExcluidaID  INT
)
RETURNS TABLE
AS
RETURN
(
    SELECT  r.ID_Reserva,
            r.HuespedID,
            r.FechaCheckIn,
            r.FechaCheckOut,
            r.Estado
    FROM    dbo.Reserva r
    WHERE   r.HabitacionID = @HabitacionID
      AND   r.Estado IN ('Confirmada', 'EnCurso')   -- Cancelada y Finalizada no bloquean
      AND   (@ReservaExcluidaID IS NULL OR r.ID_Reserva <> @ReservaExcluidaID)
      AND   r.FechaCheckIn  < @FechaCheckOut
      AND   r.FechaCheckOut > @FechaCheckIn
);

GO
/****** Objeto: Table [dbo].[Bitacora] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Bitacora](
	[BitacoraID] [int] IDENTITY(1,1) NOT NULL,
	[UsuarioID] [int] NULL,
	[Fecha] [datetime] NOT NULL,
	[Criticidad] [varchar](10) NOT NULL,
	[Accion] [varchar](100) NOT NULL,
	[Descripcion] [varchar](500) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[BitacoraID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[ConsumoReserva] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ConsumoReserva](
	[ConsumoID] [int] IDENTITY(1,1) NOT NULL,
	[ID_Reserva] [int] NOT NULL,
	[ID_Servicio] [int] NOT NULL,
	[Cantidad] [int] NOT NULL,
	[PrecioUnitario] [decimal](18, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ConsumoID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Habitacion] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Habitacion](
	[ID_habitacion] [int] IDENTITY(1,1) NOT NULL,
	[Numero] [int] NOT NULL,
	[TipoHabitacion] [char](1) NOT NULL,
	[PrecioBase] [decimal](18, 2) NOT NULL,
	[NivelDeServicio] [char](1) NOT NULL,
	[Estado] [char](1) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID_habitacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Numero] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Huesped] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Huesped](
	[HuespedID] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](100) NOT NULL,
	[Apellido] [varchar](100) NOT NULL,
	[DNI] [int] NOT NULL,
	[Email] [varchar](150) NULL,
	[Telefono] [varchar](50) NULL,
	[UsuarioID] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[HuespedID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[DNI] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Idioma] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Idioma](
	[IdiomaID] [int] IDENTITY(1,1) NOT NULL,
	[Codigo] [varchar](10) NOT NULL,
	[Nombre] [varchar](50) NOT NULL,
	[Activo] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdiomaID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Paquete] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Paquete](
	[ID_Paquete] [int] NOT NULL,
	[ID_Servicio] [int] NOT NULL,
	[Cantidad] [int] NOT NULL,
	[PrecioUnitario] [decimal](18, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID_Paquete] ASC,
	[ID_Servicio] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Patente] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Patente](
	[PatenteID] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](100) NOT NULL,
	[Descripcion] [varchar](255) NULL,
	[EsFamilia] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[PatenteID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[PatenteJerarquia] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PatenteJerarquia](
	[FamiliaPadreID] [int] NOT NULL,
	[PatenteHijaID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[FamiliaPadreID] ASC,
	[PatenteHijaID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[ServicioLimpieza] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ServicioLimpieza](
	[AlertaID] [int] IDENTITY(1,1) NOT NULL,
	[HabitacionID] [int] NOT NULL,
	[FechaHora] [datetime] NOT NULL,
	[Prioridad] [varchar](20) NOT NULL,
	[Atendida] [bit] NOT NULL,
	[UsuarioAtendioID] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[AlertaID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[ServiciosPaquete] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ServiciosPaquete](
	[ID_Servicio] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](100) NOT NULL,
	[Precio] [decimal](18, 2) NOT NULL,
	[EsCombo] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID_Servicio] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SesionUsuario] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SesionUsuario](
	[SesionID] [int] IDENTITY(1,1) NOT NULL,
	[UsuarioID] [int] NOT NULL,
	[FechaLogin] [datetime] NOT NULL,
	[FechaLogout] [datetime] NULL,
	[Exitoso] [bit] NOT NULL,
	[DireccionIP] [varchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[SesionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[TokensRecuperacion] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TokensRecuperacion](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Email] [nvarchar](255) NOT NULL,
	[Token] [nvarchar](50) NOT NULL,
	[FechaExpiracion] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Traduccion] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Traduccion](
	[TraduccionID] [int] IDENTITY(1,1) NOT NULL,
	[IdiomaID] [int] NOT NULL,
	[Clave] [varchar](100) NOT NULL,
	[Valor] [nvarchar](255) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[TraduccionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Traduccion] UNIQUE NONCLUSTERED 
(
	[IdiomaID] ASC,
	[Clave] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Usuario] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Usuario](
	[UsuarioID] [int] IDENTITY(1,1) NOT NULL,
	[NombreUsuario] [varchar](50) NOT NULL,
	[PasswordHash] [varbinary](64) NOT NULL,
	[NombreCompleto] [varchar](150) NOT NULL,
	[Email] [varchar](150) NOT NULL,
	[IdiomaPreferido] [varchar](10) NOT NULL,
	[Activo] [bit] NOT NULL,
	[FechaCreacion] [datetime] NOT NULL,
	[UltimoLogin] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[UsuarioID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[NombreUsuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[UsuarioPatente] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UsuarioPatente](
	[UsuarioID] [int] NOT NULL,
	[PatenteID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UsuarioID] ASC,
	[PatenteID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [IX_Bitacora_Criticidad] Fecha de script: 14/09/2026 22:14:45 ******/
CREATE NONCLUSTERED INDEX [IX_Bitacora_Criticidad] ON [dbo].[Bitacora]
(
	[Criticidad] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Objeto: Index [IX_Bitacora_Fecha] Fecha de script: 14/09/2026 22:14:45 ******/
CREATE NONCLUSTERED INDEX [IX_Bitacora_Fecha] ON [dbo].[Bitacora]
(
	[Fecha] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [IX_Reserva_Estado] Fecha de script: 14/09/2026 22:14:45 ******/
CREATE NONCLUSTERED INDEX [IX_Reserva_Estado] ON [dbo].[Reserva]
(
	[Estado] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Objeto: Index [IX_Reserva_FechaCheckIn] Fecha de script: 14/09/2026 22:14:45 ******/
CREATE NONCLUSTERED INDEX [IX_Reserva_FechaCheckIn] ON [dbo].[Reserva]
(
	[FechaCheckIn] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Objeto: Index [IX_Reserva_Habitacion_Fechas] Fecha de script: 14/09/2026 22:14:45 ******/
CREATE NONCLUSTERED INDEX [IX_Reserva_Habitacion_Fechas] ON [dbo].[Reserva]
(
	[HabitacionID] ASC,
	[FechaCheckIn] ASC,
	[FechaCheckOut] ASC
)
INCLUDE([Estado],[HuespedID],[Total]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Bitacora] ADD  DEFAULT (getdate()) FOR [Fecha]
GO
ALTER TABLE [dbo].[ConsumoReserva] ADD  DEFAULT ((1)) FOR [Cantidad]
GO
ALTER TABLE [dbo].[Habitacion] ADD  DEFAULT ('D') FOR [Estado]
GO
ALTER TABLE [dbo].[Idioma] ADD  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [dbo].[Paquete] ADD  DEFAULT ((1)) FOR [Cantidad]
GO
ALTER TABLE [dbo].[Patente] ADD  DEFAULT ((0)) FOR [EsFamilia]
GO
ALTER TABLE [dbo].[Reserva] ADD  DEFAULT ('Confirmada') FOR [Estado]
GO
ALTER TABLE [dbo].[ServicioLimpieza] ADD  DEFAULT (getdate()) FOR [FechaHora]
GO
ALTER TABLE [dbo].[ServicioLimpieza] ADD  DEFAULT ('Normal') FOR [Prioridad]
GO
ALTER TABLE [dbo].[ServicioLimpieza] ADD  DEFAULT ((0)) FOR [Atendida]
GO
ALTER TABLE [dbo].[SesionUsuario] ADD  DEFAULT (getdate()) FOR [FechaLogin]
GO
ALTER TABLE [dbo].[Usuario] ADD  DEFAULT ('ES') FOR [IdiomaPreferido]
GO
ALTER TABLE [dbo].[Usuario] ADD  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [dbo].[Usuario] ADD  DEFAULT (getdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[Bitacora]  WITH CHECK ADD FOREIGN KEY([UsuarioID])
REFERENCES [dbo].[Usuario] ([UsuarioID])
GO
ALTER TABLE [dbo].[Bitacora]  WITH CHECK ADD FOREIGN KEY([UsuarioID])
REFERENCES [dbo].[Usuario] ([UsuarioID])
GO
ALTER TABLE [dbo].[ConsumoReserva]  WITH CHECK ADD FOREIGN KEY([ID_Reserva])
REFERENCES [dbo].[Reserva] ([ID_Reserva])
GO
ALTER TABLE [dbo].[ConsumoReserva]  WITH CHECK ADD FOREIGN KEY([ID_Reserva])
REFERENCES [dbo].[Reserva] ([ID_Reserva])
GO
ALTER TABLE [dbo].[ConsumoReserva]  WITH CHECK ADD FOREIGN KEY([ID_Servicio])
REFERENCES [dbo].[ServiciosPaquete] ([ID_Servicio])
GO
ALTER TABLE [dbo].[ConsumoReserva]  WITH CHECK ADD FOREIGN KEY([ID_Servicio])
REFERENCES [dbo].[ServiciosPaquete] ([ID_Servicio])
GO
ALTER TABLE [dbo].[Huesped]  WITH CHECK ADD  CONSTRAINT [FK_Huesped_Usuario] FOREIGN KEY([UsuarioID])
REFERENCES [dbo].[Usuario] ([UsuarioID])
GO
ALTER TABLE [dbo].[Huesped] CHECK CONSTRAINT [FK_Huesped_Usuario]
GO
ALTER TABLE [dbo].[Paquete]  WITH CHECK ADD FOREIGN KEY([ID_Paquete])
REFERENCES [dbo].[ServiciosPaquete] ([ID_Servicio])
GO
ALTER TABLE [dbo].[Paquete]  WITH CHECK ADD FOREIGN KEY([ID_Paquete])
REFERENCES [dbo].[ServiciosPaquete] ([ID_Servicio])
GO
ALTER TABLE [dbo].[Paquete]  WITH CHECK ADD FOREIGN KEY([ID_Servicio])
REFERENCES [dbo].[ServiciosPaquete] ([ID_Servicio])
GO
ALTER TABLE [dbo].[Paquete]  WITH CHECK ADD FOREIGN KEY([ID_Servicio])
REFERENCES [dbo].[ServiciosPaquete] ([ID_Servicio])
GO
ALTER TABLE [dbo].[PatenteJerarquia]  WITH CHECK ADD FOREIGN KEY([FamiliaPadreID])
REFERENCES [dbo].[Patente] ([PatenteID])
GO
ALTER TABLE [dbo].[PatenteJerarquia]  WITH CHECK ADD FOREIGN KEY([FamiliaPadreID])
REFERENCES [dbo].[Patente] ([PatenteID])
GO
ALTER TABLE [dbo].[PatenteJerarquia]  WITH CHECK ADD FOREIGN KEY([PatenteHijaID])
REFERENCES [dbo].[Patente] ([PatenteID])
GO
ALTER TABLE [dbo].[PatenteJerarquia]  WITH CHECK ADD FOREIGN KEY([PatenteHijaID])
REFERENCES [dbo].[Patente] ([PatenteID])
GO
ALTER TABLE [dbo].[Reserva]  WITH CHECK ADD FOREIGN KEY([HabitacionID])
REFERENCES [dbo].[Habitacion] ([ID_habitacion])
GO
ALTER TABLE [dbo].[Reserva]  WITH CHECK ADD FOREIGN KEY([HabitacionID])
REFERENCES [dbo].[Habitacion] ([ID_habitacion])
GO
ALTER TABLE [dbo].[Reserva]  WITH CHECK ADD FOREIGN KEY([HuespedID])
REFERENCES [dbo].[Huesped] ([HuespedID])
GO
ALTER TABLE [dbo].[Reserva]  WITH CHECK ADD FOREIGN KEY([HuespedID])
REFERENCES [dbo].[Huesped] ([HuespedID])
GO
ALTER TABLE [dbo].[ServicioLimpieza]  WITH CHECK ADD FOREIGN KEY([HabitacionID])
REFERENCES [dbo].[Habitacion] ([ID_habitacion])
GO
ALTER TABLE [dbo].[ServicioLimpieza]  WITH CHECK ADD FOREIGN KEY([HabitacionID])
REFERENCES [dbo].[Habitacion] ([ID_habitacion])
GO
ALTER TABLE [dbo].[ServicioLimpieza]  WITH CHECK ADD FOREIGN KEY([UsuarioAtendioID])
REFERENCES [dbo].[Usuario] ([UsuarioID])
GO
ALTER TABLE [dbo].[ServicioLimpieza]  WITH CHECK ADD FOREIGN KEY([UsuarioAtendioID])
REFERENCES [dbo].[Usuario] ([UsuarioID])
GO
ALTER TABLE [dbo].[SesionUsuario]  WITH CHECK ADD FOREIGN KEY([UsuarioID])
REFERENCES [dbo].[Usuario] ([UsuarioID])
GO
ALTER TABLE [dbo].[SesionUsuario]  WITH CHECK ADD FOREIGN KEY([UsuarioID])
REFERENCES [dbo].[Usuario] ([UsuarioID])
GO
ALTER TABLE [dbo].[Traduccion]  WITH CHECK ADD FOREIGN KEY([IdiomaID])
REFERENCES [dbo].[Idioma] ([IdiomaID])
GO
ALTER TABLE [dbo].[Traduccion]  WITH CHECK ADD FOREIGN KEY([IdiomaID])
REFERENCES [dbo].[Idioma] ([IdiomaID])
GO
ALTER TABLE [dbo].[UsuarioPatente]  WITH CHECK ADD FOREIGN KEY([PatenteID])
REFERENCES [dbo].[Patente] ([PatenteID])
GO
ALTER TABLE [dbo].[UsuarioPatente]  WITH CHECK ADD FOREIGN KEY([PatenteID])
REFERENCES [dbo].[Patente] ([PatenteID])
GO
ALTER TABLE [dbo].[UsuarioPatente]  WITH CHECK ADD FOREIGN KEY([UsuarioID])
REFERENCES [dbo].[Usuario] ([UsuarioID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Habitacion]  WITH CHECK ADD  CONSTRAINT [CK_Habitacion_Estado] CHECK  (([Estado]='F' OR [Estado]='L' OR [Estado]='O' OR [Estado]='D'))
GO
ALTER TABLE [dbo].[Habitacion] CHECK CONSTRAINT [CK_Habitacion_Estado]
GO
ALTER TABLE [dbo].[Paquete]  WITH CHECK ADD  CONSTRAINT [CK_Paquete_NoAutoReferencia] CHECK  (([ID_Paquete]<>[ID_Servicio]))
GO
ALTER TABLE [dbo].[Paquete] CHECK CONSTRAINT [CK_Paquete_NoAutoReferencia]
GO
ALTER TABLE [dbo].[Reserva]  WITH CHECK ADD  CONSTRAINT [CK_Reserva_Estado] CHECK  (([Estado]='Cancelada' OR [Estado]='Finalizada' OR [Estado]='EnCurso' OR [Estado]='Confirmada'))
GO
ALTER TABLE [dbo].[Reserva] CHECK CONSTRAINT [CK_Reserva_Estado]
GO
/****** Objeto: StoredProcedure [dbo].[sp_ActualizarContrasena] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_ActualizarContrasena]
    @Email NVARCHAR(255),
    @PasswordHash VARBINARY(MAX) -- Cámbialo a VARBINARY(MAX) si guardas el hash como arreglo de bytes en vez de texto
AS
BEGIN
    UPDATE Usuario 
    SET PasswordHash = @PasswordHash 
    WHERE Email = @Email;
END
GO
/****** Objeto: StoredProcedure [dbo].[sp_AtenderServicioLimpieza] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* ============================================================================
   9. sp_AtenderServicioLimpieza  (REEMPLAZA LA VERSION ANTERIOR)
   ----------------------------------------------------------------------------
   Agrega el paso que cierra el ciclo: al atender la alerta, la habitacion
   vuelve a estar disponible ('D') y por lo tanto vuelve a venderse.
   ============================================================================ */
CREATE   PROCEDURE [dbo].[sp_AtenderServicioLimpieza]
    @AlertaID  INT,
    @UsuarioID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;
    BEGIN TRY

        DECLARE @HabitacionID INT, @Atendida BIT;

        SELECT  @HabitacionID = HabitacionID, @Atendida = Atendida
        FROM    dbo.ServicioLimpieza
        WHERE   AlertaID = @AlertaID;

        IF @HabitacionID IS NULL
            THROW 52041, 'La alerta de limpieza indicada no existe.', 1;

        IF @Atendida = 1
            THROW 52042, 'Esa alerta de limpieza ya fue atendida.', 1;

        UPDATE dbo.ServicioLimpieza
        SET    Atendida = 1, UsuarioAtendioID = @UsuarioID
        WHERE  AlertaID = @AlertaID;

        -- La habitacion vuelve al circuito comercial solo si no esta ocupada
        -- ni fuera de servicio.
        UPDATE dbo.Habitacion
        SET    Estado = 'D'
        WHERE  ID_habitacion = @HabitacionID
          AND  Estado = 'L';

        INSERT INTO dbo.Bitacora (UsuarioID, Criticidad, Accion, Descripcion)
        VALUES (@UsuarioID, 'Baja', 'LIMPIEZA_ATENDIDA',
                'Servicio de limpieza #' + CAST(@AlertaID AS VARCHAR(10)) + ' atendido.');

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;

GO
/****** Objeto: StoredProcedure [dbo].[sp_BackupBaseDatos] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_BackupBaseDatos]
    @RutaArchivo VARCHAR(260)
AS
BEGIN
    SET NOCOUNT ON;
    BACKUP DATABASE StayEasyDB TO DISK = @RutaArchivo WITH FORMAT, INIT, NAME = 'Backup completo de StayEasyDB';
END;
GO
/****** Objeto: StoredProcedure [dbo].[sp_BuscarHuesped] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_BuscarHuesped]
    @Texto VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Texto IS NULL OR LTRIM(RTRIM(@Texto)) = ''
        THROW 52038, 'Ingresa un texto para buscar.', 1;

    SET @Texto = LTRIM(RTRIM(@Texto));

    SELECT TOP (50)
            h.HuespedID,
            h.Nombre,
            h.Apellido,
            h.DNI,
            h.Email,
            h.Telefono,
            h.UsuarioID
    FROM    dbo.Huesped h
    WHERE   h.Nombre   LIKE '%' + @Texto + '%'
       OR   h.Apellido LIKE '%' + @Texto + '%'
       OR   h.Email    LIKE '%' + @Texto + '%'
       OR   CAST(h.DNI AS VARCHAR(20)) LIKE @Texto + '%'
    ORDER BY h.Apellido, h.Nombre;
END;
GO
/****** Objeto: StoredProcedure [dbo].[sp_CancelarReserva] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* ============================================================================
   10. sp_CancelarReserva  (NUEVO)
   ----------------------------------------------------------------------------
   Cierra la maquina de estados. Al cancelar, el rango queda liberado
   automaticamente porque fn_ReservasEnConflicto ignora las 'Cancelada'.
   ============================================================================ */
CREATE   PROCEDURE [dbo].[sp_CancelarReserva]
    @ReservaID       INT,
    @UsuarioAccionID INT,
    @Motivo          VARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;
    BEGIN TRY

        DECLARE @Estado VARCHAR(20);

        SELECT @Estado = Estado FROM dbo.Reserva WHERE ID_Reserva = @ReservaID;

        IF @Estado IS NULL
            THROW 52035, 'La reserva indicada no existe.', 1;

        IF @Estado <> 'Confirmada'
            THROW 52043, 'Solo se puede cancelar una reserva confirmada.', 1;

        UPDATE dbo.Reserva SET Estado = 'Cancelada' WHERE ID_Reserva = @ReservaID;

        INSERT INTO dbo.Bitacora (UsuarioID, Criticidad, Accion, Descripcion)
        VALUES (@UsuarioAccionID, 'Media', 'RESERVA_CANCELADA',
                'Reserva #' + CAST(@ReservaID AS VARCHAR(10)) + ' cancelada.'
                + ISNULL(' Motivo: ' + @Motivo, ''));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;

GO
/****** Objeto: StoredProcedure [dbo].[sp_GuardarToken] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_GuardarToken]
    @Email NVARCHAR(255),
    @Token NVARCHAR(50),
    @Exp DATETIME
AS
BEGIN
    -- 1. Insertamos el nuevo token en la tabla
    INSERT INTO TokensRecuperacion (Email, Token, FechaExpiracion)
    VALUES (@Email, @Token, @Exp);

    -- 2. Devolvemos el ID autoincremental que se acaba de generar
    SELECT CAST(SCOPE_IDENTITY() AS INT);
END
GO
/****** Objeto: StoredProcedure [dbo].[sp_ListarHabitacionesDisponibles] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* ============================================================================
   4. sp_ListarHabitacionesDisponibles
   ----------------------------------------------------------------------------
   Alimenta la grilla de habitaciones de Reservas.xaml. Reemplaza los radio
   buttons hardcodeados (118, 204, 210, 301, 405, 512).

   Devuelve el subtotal de alojamiento sin recargos ni descuentos: el precio
   final lo arma ReservaBLL (nivel de servicio, servicios, combos).
   ============================================================================ */
CREATE   PROCEDURE [dbo].[sp_ListarHabitacionesDisponibles]
    @FechaCheckIn   DATE,
    @FechaCheckOut  DATE,
    @TipoHabitacion CHAR(1) = NULL      -- NULL = todos los tipos
AS
BEGIN
    SET NOCOUNT ON;

    IF @FechaCheckIn IS NULL OR @FechaCheckOut IS NULL
        THROW 52030, 'Las fechas de check-in y check-out son obligatorias.', 1;

    IF @FechaCheckOut <= @FechaCheckIn
        THROW 52031, 'La fecha de check-out debe ser posterior a la de check-in.', 1;

    DECLARE @Noches INT = DATEDIFF(DAY, @FechaCheckIn, @FechaCheckOut);

    SELECT  h.ID_habitacion,
            h.Numero,
            h.TipoHabitacion,
            h.NivelDeServicio,
            h.PrecioBase,
            h.Estado                        AS EstadoFisico,
            @Noches                         AS Noches,
            h.PrecioBase * @Noches          AS SubtotalAlojamiento
    FROM    dbo.Habitacion h
    WHERE   h.Estado <> 'F'                 -- fuera de servicio: no se vende
      AND   (@TipoHabitacion IS NULL OR h.TipoHabitacion = @TipoHabitacion)
      AND   NOT EXISTS (
                SELECT 1
                FROM   dbo.fn_ReservasEnConflicto(h.ID_habitacion, @FechaCheckIn, @FechaCheckOut, NULL)
            )
    ORDER BY h.TipoHabitacion, h.Numero;
END;

GO
/****** Objeto: StoredProcedure [dbo].[sp_ListarReservas] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_ListarReservas]
    @ReservaID    INT         = NULL,
    @Desde        DATE        = NULL,
    @Hasta        DATE        = NULL,
    @Estado       VARCHAR(20) = NULL,
    @HuespedID    INT         = NULL,
    @HabitacionID INT         = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Desde IS NOT NULL AND @Hasta IS NOT NULL AND @Hasta <= @Desde
        THROW 52044, 'Rango de fechas invalido: Hasta debe ser posterior a Desde.', 1;

    IF @Estado IS NOT NULL AND @Estado NOT IN ('Confirmada','EnCurso','Finalizada','Cancelada')
        THROW 52045, 'Estado de reserva invalido.', 1;

    SELECT
            r.ID_Reserva,
            r.FechaCheckIn,
            r.FechaCheckOut,
            r.Estado                                        AS EstadoReserva,
            r.Total,
            DATEDIFF(DAY, r.FechaCheckIn, r.FechaCheckOut)  AS Noches,

            hu.HuespedID,
            hu.Nombre,
            hu.Apellido,
            hu.DNI,
            hu.Email,
            hu.Telefono,
            hu.UsuarioID,

            ha.ID_habitacion,
            ha.Numero,
            ha.TipoHabitacion,
            ha.PrecioBase,
            ha.NivelDeServicio,
            ha.Estado                                       AS EstadoFisico
    FROM    dbo.Reserva    r
    JOIN    dbo.Huesped    hu ON hu.HuespedID     = r.HuespedID
    JOIN    dbo.Habitacion ha ON ha.ID_habitacion = r.HabitacionID
    WHERE   (@ReservaID    IS NULL OR r.ID_Reserva   = @ReservaID)
      AND   (@Estado       IS NULL OR r.Estado       = @Estado)
      AND   (@HuespedID    IS NULL OR r.HuespedID    = @HuespedID)
      AND   (@HabitacionID IS NULL OR r.HabitacionID = @HabitacionID)
      AND   (@Desde IS NULL OR @Hasta IS NULL
             OR (r.FechaCheckIn < @Hasta AND r.FechaCheckOut > @Desde))
    ORDER BY r.FechaCheckIn DESC, r.ID_Reserva DESC
    OPTION (RECOMPILE);
END;
GO
/****** Objeto: StoredProcedure [dbo].[sp_Login] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ============================================================================
-- 7. PROCEDIMIENTOS ALMACENADOS TRANSACCIONALES
-- ============================================================================

CREATE PROCEDURE [dbo].[sp_Login]
    @NombreUsuario  VARCHAR(50),
    @PasswordHash   VARBINARY(64),
    @DireccionIP    VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @UsuarioID INT, @HashAlmacenado VARBINARY(64), @Activo BIT;

    SELECT @UsuarioID = UsuarioID, @HashAlmacenado = PasswordHash, @Activo = Activo
    FROM Usuario
    WHERE NombreUsuario = @NombreUsuario;

    IF @UsuarioID IS NULL OR @HashAlmacenado <> @PasswordHash OR @Activo = 0
    BEGIN
        -- SOLO guardamos el registro de sesión si el usuario EXISTE (evita el error NULL)
        IF @UsuarioID IS NOT NULL
        BEGIN
            INSERT INTO SesionUsuario (UsuarioID, Exitoso, DireccionIP) VALUES (@UsuarioID, 0, @DireccionIP);
        END
        
        INSERT INTO Bitacora (UsuarioID, Criticidad, Accion, Descripcion)
        VALUES (@UsuarioID, 'Alta', 'LOGIN_FALLIDO', 'Intento de inicio de sesion fallido desde IP: ' + @DireccionIP);
        
        THROW 52000, 'Usuario o contrasena invalidos, o usuario inactivo.', 1;
    END

    UPDATE Usuario SET UltimoLogin = GETDATE() WHERE UsuarioID = @UsuarioID;

    INSERT INTO SesionUsuario (UsuarioID, Exitoso, DireccionIP) VALUES (@UsuarioID, 1, @DireccionIP);
    
    INSERT INTO Bitacora (UsuarioID, Criticidad, Accion, Descripcion)
    VALUES (@UsuarioID, 'Baja', 'LOGIN', 'Inicio de sesion exitoso.');

    SELECT UsuarioID, NombreUsuario, NombreCompleto,Email, IdiomaPreferido FROM Usuario WHERE UsuarioID = @UsuarioID;
END;
GO
/****** Objeto: StoredProcedure [dbo].[sp_Logout] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_Logout]
    @SesionID INT,
    @UsuarioID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        UPDATE SesionUsuario SET FechaLogout = GETDATE() WHERE SesionID = @SesionID AND UsuarioID = @UsuarioID;
        INSERT INTO Bitacora (UsuarioID, Criticidad, Accion, Descripcion) VALUES (@UsuarioID, 'Baja', 'LOGOUT', 'Cierre de sesion.');
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW 52001, 'Error al cerrar la sesion.', 1;
    END CATCH
END;
GO
/****** Objeto: StoredProcedure [dbo].[sp_ObtenerReserva] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_ObtenerReserva]
    @ReservaID INT
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.sp_ListarReservas @ReservaID = @ReservaID;
END;
GO
/****** Objeto: StoredProcedure [dbo].[sp_RegistrarCheckIn] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* ============================================================================
   7. sp_RegistrarCheckIn  (NUEVO)
   ----------------------------------------------------------------------------
   Es la contraparte que faltaba del check-out. Aca -y solo aca- la habitacion
   pasa a estar fisicamente ocupada.
   ============================================================================ */
CREATE   PROCEDURE [dbo].[sp_RegistrarCheckIn]
    @ReservaID       INT,
    @UsuarioAccionID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;
    BEGIN TRY

        DECLARE @Estado       VARCHAR(20),
                @HabitacionID INT,
                @FechaIn      DATE;

        SELECT  @Estado       = Estado,
                @HabitacionID = HabitacionID,
                @FechaIn      = FechaCheckIn
        FROM    dbo.Reserva
        WHERE   ID_Reserva = @ReservaID;

        IF @Estado IS NULL
            THROW 52035, 'La reserva indicada no existe.', 1;

        IF @Estado <> 'Confirmada'
            THROW 52036, 'Solo se puede hacer check-in de una reserva confirmada.', 1;

        IF @FechaIn > CAST(GETDATE() AS DATE)
            THROW 52037, 'Todavia no es la fecha de ingreso de esta reserva.', 1;

        UPDATE dbo.Reserva    SET Estado = 'EnCurso' WHERE ID_Reserva    = @ReservaID;
        UPDATE dbo.Habitacion SET Estado = 'O'       WHERE ID_habitacion = @HabitacionID;

        INSERT INTO dbo.Bitacora (UsuarioID, Criticidad, Accion, Descripcion)
        VALUES (@UsuarioAccionID, 'Media', 'CHECKIN',
                'Check-in de la reserva #' + CAST(@ReservaID AS VARCHAR(10)) + '.');

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;

GO
/****** Objeto: StoredProcedure [dbo].[sp_RegistrarCheckOut] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* ============================================================================
   8. sp_RegistrarCheckOut  (REEMPLAZA LA VERSION ANTERIOR)
   ----------------------------------------------------------------------------
   Cambios:
     - Ya no recibe @HabitacionID: lo deduce de la reserva (el original
       confiaba en que la UI mandara el ID correcto).
     - Valida que la reserva este 'EnCurso'.
     - La habitacion queda en 'L' (en limpieza), no en 'D'. Recien vuelve a
       'D' cuando el personal cierra la alerta: eso completa el Observer.
   ============================================================================ */
CREATE   PROCEDURE [dbo].[sp_RegistrarCheckOut]
    @ReservaID       INT,
    @UsuarioAccionID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;
    BEGIN TRY

        DECLARE @Estado VARCHAR(20), @HabitacionID INT;

        SELECT  @Estado = Estado, @HabitacionID = HabitacionID
        FROM    dbo.Reserva
        WHERE   ID_Reserva = @ReservaID;

        IF @Estado IS NULL
            THROW 52035, 'La reserva indicada no existe.', 1;

        IF @Estado <> 'EnCurso'
            THROW 52040, 'Solo se puede hacer check-out de una reserva en curso.', 1;

        UPDATE dbo.Reserva    SET Estado = 'Finalizada' WHERE ID_Reserva    = @ReservaID;
        UPDATE dbo.Habitacion SET Estado = 'L'          WHERE ID_habitacion = @HabitacionID;

        -- Observer: el check-out notifica automaticamente al panel de limpieza.
        INSERT INTO dbo.ServicioLimpieza (HabitacionID, Prioridad)
        VALUES (@HabitacionID, 'Normal');

        INSERT INTO dbo.Bitacora (UsuarioID, Criticidad, Accion, Descripcion)
        VALUES (@UsuarioAccionID, 'Media', 'CHECKOUT',
                'Check-out de la reserva #' + CAST(@ReservaID AS VARCHAR(10)) + '.');

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;

GO
/****** Objeto: StoredProcedure [dbo].[sp_RegistrarReserva] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* ============================================================================
   6. sp_RegistrarReserva  (REEMPLAZA LA VERSION ANTERIOR)
   ----------------------------------------------------------------------------
   Cambios respecto del original:
     - Valida solapamiento de fechas en lugar de Habitacion.Estado = 'D'.
     - YA NO marca la habitacion como ocupada: eso pasa en el check-in.
     - SERIALIZABLE: el motor toma range locks sobre el predicado de
       solapamiento, asi dos recepcionistas no pueden reservar la misma
       habitacion para el mismo rango al mismo tiempo. Es la parte que evita
       el overbooking por concurrencia.
     - ROLLBACK protegido con XACT_STATE() (el original tiraba error si la
       transaccion ya estaba deshecha).
     - Se conserva el codigo de error 52010 para "no disponible" porque
       AccesoDatos.EscribirEscalar ya lo trata como mensaje de negocio.

   La firma de parametros NO cambia: ReservaMPP.RegistrarReserva sigue
   funcionando tal cual esta hoy.
   ============================================================================ */
CREATE   PROCEDURE [dbo].[sp_RegistrarReserva]
    @HuespedID       INT,
    @HabitacionID    INT,
    @FechaCheckIn    DATE,
    @FechaCheckOut   DATE,
    @Total           DECIMAL(18,2),
    @UsuarioAccionID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

    IF @FechaCheckOut <= @FechaCheckIn
        THROW 52031, 'La fecha de check-out debe ser posterior a la de check-in.', 1;

    IF @FechaCheckIn < CAST(GETDATE() AS DATE)
        THROW 52032, 'No se pueden registrar reservas con fecha de ingreso pasada.', 1;

    IF @Total < 0
        THROW 52039, 'El total de la reserva no puede ser negativo.', 1;

    BEGIN TRANSACTION;
    BEGIN TRY

        IF NOT EXISTS (SELECT 1 FROM dbo.Huesped WHERE HuespedID = @HuespedID)
            THROW 52033, 'El huesped indicado no existe.', 1;

        DECLARE @EstadoHab CHAR(1);
        SELECT @EstadoHab = Estado
        FROM   dbo.Habitacion
        WHERE  ID_habitacion = @HabitacionID;

        IF @EstadoHab IS NULL
            THROW 52034, 'La habitacion indicada no existe.', 1;

        IF @EstadoHab = 'F'
            THROW 52010, 'La habitacion se encuentra fuera de servicio.', 1;

        -- Regla de negocio central: nadie mas ocupa ese rango.
        IF EXISTS (SELECT 1 FROM dbo.fn_ReservasEnConflicto(@HabitacionID, @FechaCheckIn, @FechaCheckOut, NULL))
            THROW 52010, 'La habitacion ya tiene una reserva en ese rango de fechas.', 1;

        INSERT INTO dbo.Reserva (HuespedID, HabitacionID, FechaCheckIn, FechaCheckOut, Estado, Total)
        VALUES (@HuespedID, @HabitacionID, @FechaCheckIn, @FechaCheckOut, 'Confirmada', @Total);

        DECLARE @NuevaReservaID INT = SCOPE_IDENTITY();

        INSERT INTO dbo.Bitacora (UsuarioID, Criticidad, Accion, Descripcion)
        VALUES (@UsuarioAccionID, 'Media', 'RESERVA_ALTA',
                'Reserva #' + CAST(@NuevaReservaID AS VARCHAR(10))
                + ' registrada - habitacion ' + CAST(@HabitacionID AS VARCHAR(10))
                + ' del ' + CONVERT(VARCHAR(10), @FechaCheckIn, 103)
                + ' al ' + CONVERT(VARCHAR(10), @FechaCheckOut, 103) + '.');

        COMMIT TRANSACTION;

        -- EscribirEscalar hace ExecuteScalar: devuelve el ID de la reserva nueva.
        SELECT @NuevaReservaID AS ID_Reserva;

    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;

GO
/****** Objeto: StoredProcedure [dbo].[sp_RegistrarUsuario] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_RegistrarUsuario]
    @NombreUsuario  VARCHAR(50),
    @PasswordHash   VARBINARY(64),
    @NombreCompleto VARCHAR(150),
    @Email          VARCHAR(150),
    @IdiomaPreferido VARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Usuario WHERE NombreUsuario = @NombreUsuario)
    BEGIN
        THROW 52002, 'El nombre de usuario ya está registrado. Elegí otro.', 1;
    END

    INSERT INTO Usuario (NombreUsuario, PasswordHash, NombreCompleto, Email, IdiomaPreferido, Activo, FechaCreacion)
    VALUES (@NombreUsuario, @PasswordHash, @NombreCompleto, @Email, ISNULL(@IdiomaPreferido, 'ES'), 1, GETDATE());
END;
GO
/****** Objeto: StoredProcedure [dbo].[sp_RegistrarUsuarioHuesped] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_RegistrarUsuarioHuesped]
    @NombreUsuario VARCHAR(50),  @PasswordHash VARBINARY(64),
    @Nombre        VARCHAR(100), @Apellido     VARCHAR(100),
    @DNI           INT,          @Email        VARCHAR(150),
    @Telefono      VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NuevoUsuarioID INT, @NuevoHuespedID INT, @PatenteHuespedID INT;

    BEGIN TRANSACTION;
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM Usuario WHERE NombreUsuario = @NombreUsuario)
            THROW 52020, 'El nombre de usuario ya esta registrado. Elegi otro.', 1;

        IF EXISTS (SELECT 1 FROM Huesped WHERE DNI = @DNI)
            THROW 52021, 'Ya existe un huesped registrado con ese DNI.', 1;

        INSERT INTO Usuario (NombreUsuario, PasswordHash, NombreCompleto, Email)
        VALUES (@NombreUsuario, @PasswordHash, @Nombre + ' ' + @Apellido, @Email);
        SET @NuevoUsuarioID = SCOPE_IDENTITY();

        SELECT @PatenteHuespedID = PatenteID
        FROM   Patente WHERE Nombre = 'Huesped' AND EsFamilia = 1;

        IF @PatenteHuespedID IS NULL
            THROW 52022, 'No se encuentra configurada la Patente Huesped.', 1;

        INSERT INTO UsuarioPatente (UsuarioID, PatenteID)
        VALUES (@NuevoUsuarioID, @PatenteHuespedID);

        INSERT INTO Huesped (Nombre, Apellido, DNI, Email, Telefono, UsuarioID)
        VALUES (@Nombre, @Apellido, @DNI, @Email, @Telefono, @NuevoUsuarioID);
        SET @NuevoHuespedID = SCOPE_IDENTITY();

        INSERT INTO Bitacora (UsuarioID, Criticidad, Accion, Descripcion)
        VALUES (@NuevoUsuarioID, 'Media', 'REGISTRO_HUESPED',
                'Autorregistro de huesped #' + CAST(@NuevoHuespedID AS VARCHAR(10))
                + ' con usuario "' + @NombreUsuario + '".');

        COMMIT TRANSACTION;
        SELECT @NuevoHuespedID AS HuespedID;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
/****** Objeto: StoredProcedure [dbo].[sp_RestoreBaseDatos] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_RestoreBaseDatos]
    @RutaArchivo VARCHAR(260)
AS
BEGIN
    SET NOCOUNT ON;
    ALTER DATABASE StayEasyDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    RESTORE DATABASE StayEasyDB FROM DISK = @RutaArchivo WITH REPLACE;
    ALTER DATABASE StayEasyDB SET MULTI_USER;
END;
GO
/****** Objeto: StoredProcedure [dbo].[sp_ValidarEmail] Fecha de script: 14/09/2026 22:14:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_ValidarEmail]
    @Email NVARCHAR(255)
AS
BEGIN
    -- Seleccionamos el ID o un 1 para hacerlo ligero, solo queremos saber si existe
    SELECT 1 
    FROM Usuarios 
    WHERE Email = @Email
END
GO
USE [master]
GO
ALTER DATABASE [StayEasyDB] SET  READ_WRITE 
GO

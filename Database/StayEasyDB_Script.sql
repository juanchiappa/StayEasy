USE [StayEasyDB]
GO
/****** Object:  Table [dbo].[Bitacora]    Script Date: 08/09/2026 09:57:00 ******/
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
/****** Object:  Table [dbo].[ConsumoReserva]    Script Date: 08/09/2026 09:57:00 ******/
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
/****** Object:  Table [dbo].[Habitacion]    Script Date: 08/09/2026 09:57:00 ******/
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
/****** Object:  Table [dbo].[Huesped]    Script Date: 08/09/2026 09:57:00 ******/
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
/****** Object:  Table [dbo].[Idioma]    Script Date: 08/09/2026 09:57:00 ******/
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
/****** Object:  Table [dbo].[Paquete]    Script Date: 08/09/2026 09:57:00 ******/
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
/****** Object:  Table [dbo].[Patente]    Script Date: 08/09/2026 09:57:00 ******/
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
/****** Object:  Table [dbo].[PatenteJerarquia]    Script Date: 08/09/2026 09:57:00 ******/
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
/****** Object:  Table [dbo].[Reserva]    Script Date: 08/09/2026 09:57:00 ******/
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
/****** Object:  Table [dbo].[ServicioLimpieza]    Script Date: 08/09/2026 09:57:00 ******/
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
/****** Object:  Table [dbo].[ServiciosPaquete]    Script Date: 08/09/2026 09:57:00 ******/
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
/****** Object:  Table [dbo].[SesionUsuario]    Script Date: 08/09/2026 09:57:00 ******/
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
/****** Object:  Table [dbo].[Traduccion]    Script Date: 08/09/2026 09:57:00 ******/
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
/****** Object:  Table [dbo].[Usuario]    Script Date: 08/09/2026 09:57:00 ******/
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
	[PasswordReset] [varbinary](50) NULL,
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
/****** Object:  Table [dbo].[UsuarioPatente]    Script Date: 08/09/2026 09:57:00 ******/
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
ALTER TABLE [dbo].[ConsumoReserva]  WITH CHECK ADD FOREIGN KEY([ID_Reserva])
REFERENCES [dbo].[Reserva] ([ID_Reserva])
GO
ALTER TABLE [dbo].[ConsumoReserva]  WITH CHECK ADD FOREIGN KEY([ID_Servicio])
REFERENCES [dbo].[ServiciosPaquete] ([ID_Servicio])
GO
ALTER TABLE [dbo].[Huesped]  WITH CHECK ADD CONSTRAINT [FK_Huesped_Usuario] FOREIGN KEY([UsuarioID])
REFERENCES [dbo].[Usuario] ([UsuarioID])
GO
ALTER TABLE [dbo].[Paquete]  WITH CHECK ADD FOREIGN KEY([ID_Paquete])
REFERENCES [dbo].[ServiciosPaquete] ([ID_Servicio])
GO
ALTER TABLE [dbo].[Paquete]  WITH CHECK ADD FOREIGN KEY([ID_Servicio])
REFERENCES [dbo].[ServiciosPaquete] ([ID_Servicio])
GO
ALTER TABLE [dbo].[PatenteJerarquia]  WITH CHECK ADD FOREIGN KEY([FamiliaPadreID])
REFERENCES [dbo].[Patente] ([PatenteID])
GO
ALTER TABLE [dbo].[PatenteJerarquia]  WITH CHECK ADD FOREIGN KEY([PatenteHijaID])
REFERENCES [dbo].[Patente] ([PatenteID])
GO
ALTER TABLE [dbo].[Reserva]  WITH CHECK ADD FOREIGN KEY([HabitacionID])
REFERENCES [dbo].[Habitacion] ([ID_habitacion])
GO
ALTER TABLE [dbo].[Reserva]  WITH CHECK ADD FOREIGN KEY([HuespedID])
REFERENCES [dbo].[Huesped] ([HuespedID])
GO
ALTER TABLE [dbo].[ServicioLimpieza]  WITH CHECK ADD FOREIGN KEY([HabitacionID])
REFERENCES [dbo].[Habitacion] ([ID_habitacion])
GO
ALTER TABLE [dbo].[ServicioLimpieza]  WITH CHECK ADD FOREIGN KEY([UsuarioAtendioID])
REFERENCES [dbo].[Usuario] ([UsuarioID])
GO
ALTER TABLE [dbo].[SesionUsuario]  WITH CHECK ADD FOREIGN KEY([UsuarioID])
REFERENCES [dbo].[Usuario] ([UsuarioID])
GO
ALTER TABLE [dbo].[Traduccion]  WITH CHECK ADD FOREIGN KEY([IdiomaID])
REFERENCES [dbo].[Idioma] ([IdiomaID])
GO
ALTER TABLE [dbo].[UsuarioPatente]  WITH CHECK ADD FOREIGN KEY([PatenteID])
REFERENCES [dbo].[Patente] ([PatenteID])
GO
ALTER TABLE [dbo].[UsuarioPatente]  WITH CHECK ADD FOREIGN KEY([UsuarioID])
REFERENCES [dbo].[Usuario] ([UsuarioID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Paquete]  WITH CHECK ADD  CONSTRAINT [CK_Paquete_NoAutoReferencia] CHECK  (([ID_Paquete]<>[ID_Servicio]))
GO
ALTER TABLE [dbo].[Paquete] CHECK CONSTRAINT [CK_Paquete_NoAutoReferencia]
GO
ALTER TABLE [dbo].[Reserva]  WITH CHECK ADD  CONSTRAINT [CK_Reserva_Estado] CHECK  (([Estado]='Cancelada' OR [Estado]='Finalizada' OR [Estado]='EnCurso' OR [Estado]='Confirmada'))
GO
ALTER TABLE [dbo].[Reserva] CHECK CONSTRAINT [CK_Reserva_Estado]
GO
-- ============================================================================
-- 6. DATOS INICIALES (SEED)
-- ============================================================================
-- Patente-Familia "Huesped": rol de autogestion para huespedes registrados
-- desde la app (patron Composite). Se usa sin tilde para ser consistente
-- con el enum Rol de StayEasy.BE y evitar problemas de collation en varchar.
IF NOT EXISTS (SELECT 1 FROM [dbo].[Patente] WHERE [Nombre] = 'Huesped' AND [EsFamilia] = 1)
BEGIN
    INSERT INTO [dbo].[Patente] ([Nombre], [Descripcion], [EsFamilia])
    VALUES ('Huesped', 'Rol de autogestion para huespedes registrados desde la app.', 1);
END
GO
/****** Object:  StoredProcedure [dbo].[sp_AtenderServicioLimpieza]    Script Date: 08/09/2026 09:57:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_AtenderServicioLimpieza]
    @AlertaID   INT,
    @UsuarioID  INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        UPDATE ServicioLimpieza
        SET Atendida = 1, UsuarioAtendioID = @UsuarioID
        WHERE AlertaID = @AlertaID;

        INSERT INTO Bitacora (UsuarioID, Criticidad, Accion, Descripcion)
        VALUES (@UsuarioID, 'Baja', 'LIMPIEZA_ATENDIDA', 'Servicio de limpieza #' + CAST(@AlertaID AS VARCHAR(10)) + ' atendido.');

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW 52012, 'Error al marcar el servicio de limpieza.', 1;
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_BackupBaseDatos]    Script Date: 08/09/2026 09:57:00 ******/
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
/****** Object:  StoredProcedure [dbo].[sp_Login]    Script Date: 08/09/2026 09:57:00 ******/
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

    SELECT UsuarioID, NombreUsuario, NombreCompleto,Email,IdiomaPreferido FROM Usuario WHERE UsuarioID = @UsuarioID;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_Logout]    Script Date: 08/09/2026 09:57:00 ******/
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
/****** Object:  StoredProcedure [dbo].[sp_RegistrarCheckOut]    Script Date: 08/09/2026 09:57:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_RegistrarCheckOut]
    @ReservaID      INT,
    @HabitacionID   INT,
    @UsuarioAccionID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        UPDATE Reserva SET Estado = 'Finalizada' WHERE ID_Reserva = @ReservaID;
        UPDATE Habitacion SET Estado = 'D' WHERE ID_habitacion = @HabitacionID;

        -- Genera la alerta mediante patrón Observer
        INSERT INTO ServicioLimpieza (HabitacionID, Prioridad) VALUES (@HabitacionID, 'Normal');

        INSERT INTO Bitacora (UsuarioID, Criticidad, Accion, Descripcion)
        VALUES (@UsuarioAccionID, 'Media', 'CHECKOUT', 'Check-out de la reserva #' + CAST(@ReservaID AS VARCHAR(10)) + '.');

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW 50005, 'Error al registrar el check-out.', 1;
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_RegistrarReserva]    Script Date: 08/09/2026 09:57:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_RegistrarReserva]
    @HuespedID      INT,
    @HabitacionID   INT,
    @FechaCheckIn   DATE,
    @FechaCheckOut  DATE,
    @Total          DECIMAL(18,2),
    @UsuarioAccionID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        IF (SELECT Estado FROM Habitacion WHERE ID_habitacion = @HabitacionID) <> 'D'
        BEGIN
            THROW 52010, 'La habitacion no se encuentra disponible.', 1;
        END

        INSERT INTO Reserva (HuespedID, HabitacionID, FechaCheckIn, FechaCheckOut, Total)
        VALUES (@HuespedID, @HabitacionID, @FechaCheckIn, @FechaCheckOut, @Total);
        DECLARE @NuevaReservaID INT = SCOPE_IDENTITY();

        UPDATE Habitacion SET Estado = 'O' WHERE ID_habitacion = @HabitacionID;
        INSERT INTO Bitacora (UsuarioID, Criticidad, Accion, Descripcion)
        VALUES (@UsuarioAccionID, 'Media', 'CHECKIN', 'Reserva #' + CAST(@NuevaReservaID AS VARCHAR(10)) + ' registrada.');

        COMMIT TRANSACTION;
        SELECT @NuevaReservaID AS ID_Reserva;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_RegistrarUsuario]    Script Date: 08/09/2026 09:57:00 ******/
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
/****** Object:  StoredProcedure [dbo].[sp_RegistrarUsuarioHuesped]    Script Date: 08/09/2026 09:57:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Autorregistro del Huesped como actor directo del sistema: da de alta el
-- Usuario (Seguridad), le asigna la Patente-Familia "Huesped" y crea el
-- registro de Huesped (Dominio) enlazado, todo dentro de una unica transaccion.
CREATE PROCEDURE [dbo].[sp_RegistrarUsuarioHuesped]
    @NombreUsuario  VARCHAR(50),
    @PasswordHash   VARBINARY(64),
    @Nombre         VARCHAR(100),
    @Apellido       VARCHAR(100),
    @DNI            INT,
    @Email          VARCHAR(150),
    @Telefono       VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NuevoUsuarioID INT;
    DECLARE @NuevoHuespedID INT;
    DECLARE @PatenteHuespedID INT;

    BEGIN TRANSACTION;
    BEGIN TRY

        -- Validaciones de negocio previas al INSERT (mismo estilo que sp_RegistrarUsuario)
        IF EXISTS (SELECT 1 FROM Usuario WHERE NombreUsuario = @NombreUsuario)
        BEGIN
            THROW 52020, 'El nombre de usuario ya esta registrado. Elegi otro.', 1;
        END

        IF EXISTS (SELECT 1 FROM Huesped WHERE DNI = @DNI)
        BEGIN
            THROW 52021, 'Ya existe un huesped registrado con ese DNI.', 1;
        END

        -- 1) Alta de Usuario (Seguridad). IdiomaPreferido, Activo y FechaCreacion
        --    toman sus valores DEFAULT ya definidos en la tabla.
        INSERT INTO Usuario (NombreUsuario, PasswordHash, NombreCompleto, Email)
        VALUES (@NombreUsuario, @PasswordHash, @Nombre + ' ' + @Apellido, @Email);

        SET @NuevoUsuarioID = SCOPE_IDENTITY();

        -- 2) Asignacion del rol "Huesped" (patron Composite -> UsuarioPatente)
        SELECT @PatenteHuespedID = PatenteID
        FROM Patente
        WHERE Nombre = 'Huesped' AND EsFamilia = 1;

        IF @PatenteHuespedID IS NULL
        BEGIN
            THROW 52022, 'No se encuentra configurada la Patente Huesped.', 1;
        END

        INSERT INTO UsuarioPatente (UsuarioID, PatenteID)
        VALUES (@NuevoUsuarioID, @PatenteHuespedID);

        -- 3) Alta de Huesped (Dominio) enlazado al Usuario recien creado
        INSERT INTO Huesped (Nombre, Apellido, DNI, Email, Telefono, UsuarioID)
        VALUES (@Nombre, @Apellido, @DNI, @Email, @Telefono, @NuevoUsuarioID);

        SET @NuevoHuespedID = SCOPE_IDENTITY();

        -- 4) Bitacora, igual que el resto de los SP transaccionales del sistema
        INSERT INTO Bitacora (UsuarioID, Criticidad, Accion, Descripcion)
        VALUES (@NuevoUsuarioID, 'Media', 'REGISTRO_HUESPED',
                'Autorregistro de huesped #' + CAST(@NuevoHuespedID AS VARCHAR(10)) + ' con usuario "' + @NombreUsuario + '".');

        COMMIT TRANSACTION;

        -- EscribirEscalar (AccesoDatos) hace ExecuteScalar: se devuelve el HuespedID nuevo
        SELECT @NuevoHuespedID AS HuespedID;

    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_RestoreBaseDatos]    Script Date: 08/09/2026 09:57:00 ******/
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
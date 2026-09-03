-- ============================================================================
-- PROYECTO 5: STAYEASY — Administración de Hoteles y Reservas
-- ============================================================================

CREATE DATABASE StayEasyDB;
GO
USE StayEasyDB;
GO

-- ============================================================================
-- 1. MÓDULO DE SEGURIDAD Y PERMISOS (Patrón Composite)
-- ============================================================================

CREATE TABLE Patente (
    PatenteID       INT IDENTITY(1,1) PRIMARY KEY,
    Nombre          VARCHAR(100) NOT NULL,
    Descripcion     VARCHAR(255) NULL,
    EsFamilia       BIT NOT NULL DEFAULT 0 -- 0: patente simple, 1: familia (rol)
);
GO

CREATE TABLE PatenteJerarquia (
    FamiliaPadreID  INT NOT NULL,
    PatenteHijaID   INT NOT NULL,
    PRIMARY KEY (FamiliaPadreID, PatenteHijaID),
    FOREIGN KEY (FamiliaPadreID) REFERENCES Patente(PatenteID),
    FOREIGN KEY (PatenteHijaID)  REFERENCES Patente(PatenteID)
);
GO

CREATE TABLE Usuario (
    UsuarioID       INT IDENTITY(1,1) PRIMARY KEY,
    NombreUsuario   VARCHAR(50) UNIQUE NOT NULL,
    PasswordHash    VARBINARY(64) NOT NULL, -- SHA-256
    NombreCompleto  VARCHAR(150) NOT NULL,
    Email           VARCHAR(150) NOT NULL,
    IdiomaPreferido VARCHAR(10) NOT NULL DEFAULT 'ES',
    Activo          BIT NOT NULL DEFAULT 1,
    FechaCreacion   DATETIME NOT NULL DEFAULT GETDATE(),
    UltimoLogin     DATETIME NULL
);
GO

CREATE TABLE UsuarioPatente (
    UsuarioID   INT NOT NULL,
    PatenteID   INT NOT NULL,
    PRIMARY KEY (UsuarioID, PatenteID),
    FOREIGN KEY (UsuarioID) REFERENCES Usuario(UsuarioID) ON DELETE CASCADE,
    FOREIGN KEY (PatenteID) REFERENCES Patente(PatenteID)
);
GO

-- ============================================================================
-- 2. CONTROL DE SESIÓN Y BITÁCORA
-- ============================================================================

CREATE TABLE SesionUsuario (
    SesionID        INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioID       INT NOT NULL,
    FechaLogin      DATETIME NOT NULL DEFAULT GETDATE(),
    FechaLogout     DATETIME NULL,
    Exitoso         BIT NOT NULL,
    DireccionIP     VARCHAR(50) NULL,
    FOREIGN KEY (UsuarioID) REFERENCES Usuario(UsuarioID)
);
GO

CREATE TABLE Bitacora (
    BitacoraID      INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioID       INT NULL,
    Fecha           DATETIME NOT NULL DEFAULT GETDATE(),
    Criticidad      VARCHAR(10) NOT NULL, -- 'Alta', 'Media', 'Baja'
    Accion          VARCHAR(100) NOT NULL,
    Descripcion     VARCHAR(500) NOT NULL,
    FOREIGN KEY (UsuarioID) REFERENCES Usuario(UsuarioID)
);
GO

-- ============================================================================
-- 3. IDIOMA DINÁMICO
-- ============================================================================

CREATE TABLE Idioma (
    IdiomaID    INT IDENTITY(1,1) PRIMARY KEY,
    Codigo      VARCHAR(10) UNIQUE NOT NULL,
    Nombre      VARCHAR(50) NOT NULL,
    Activo      BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE Traduccion (
    TraduccionID    INT IDENTITY(1,1) PRIMARY KEY,
    IdiomaID        INT NOT NULL,
    Clave           VARCHAR(100) NOT NULL,
    Valor           NVARCHAR(255) NOT NULL,
    FOREIGN KEY (IdiomaID) REFERENCES Idioma(IdiomaID),
    CONSTRAINT UQ_Traduccion UNIQUE (IdiomaID, Clave)
);
GO

-- ============================================================================
-- 4. DOMINIO HOTELERO
-- ============================================================================

CREATE TABLE Habitacion (
    ID_habitacion    INT IDENTITY(1,1) PRIMARY KEY,
    Numero           INT UNIQUE NOT NULL,
    TipoHabitacion   CHAR(1) NOT NULL, 
    PrecioBase       DECIMAL(18,2) NOT NULL,
    NivelDeServicio  CHAR(1) NOT NULL,
    Estado           CHAR(1) NOT NULL DEFAULT 'D' 
);
GO

CREATE TABLE ServiciosPaquete (
    ID_Servicio     INT IDENTITY(1,1) PRIMARY KEY,
    Nombre          VARCHAR(100) NOT NULL,
    Precio          DECIMAL(18,2) NOT NULL,
    EsCombo         BIT NOT NULL -- Representa el Composite
);
GO

CREATE TABLE Paquete (
    ID_Paquete      INT NOT NULL,
    ID_Servicio     INT NOT NULL,
    Cantidad        INT NOT NULL DEFAULT 1,
    PrecioUnitario  DECIMAL(18,2) NOT NULL,
    PRIMARY KEY (ID_Paquete, ID_Servicio),
    FOREIGN KEY (ID_Paquete)  REFERENCES ServiciosPaquete(ID_Servicio),
    FOREIGN KEY (ID_Servicio) REFERENCES ServiciosPaquete(ID_Servicio),
    CONSTRAINT CK_Paquete_NoAutoReferencia CHECK (ID_Paquete <> ID_Servicio)
);
GO

CREATE TABLE Huesped (
    HuespedID   INT IDENTITY(1,1) PRIMARY KEY,
    Nombre      VARCHAR(100) NOT NULL,
    Apellido    VARCHAR(100) NOT NULL,
    DNI         INT UNIQUE NOT NULL,
    Email       VARCHAR(150) NULL,
    Telefono    VARCHAR(50) NULL
);
GO

CREATE TABLE Reserva (
    ID_Reserva      INT IDENTITY(1,1) PRIMARY KEY,
    HuespedID       INT NOT NULL,
    HabitacionID    INT NOT NULL,
    FechaCheckIn    DATE NOT NULL,
    FechaCheckOut   DATE NOT NULL,
    Estado          VARCHAR(20) NOT NULL DEFAULT 'Confirmada'
        CONSTRAINT CK_Reserva_Estado CHECK (Estado IN ('Confirmada','EnCurso','Finalizada','Cancelada')), 
    Total           DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (HuespedID)    REFERENCES Huesped(HuespedID),
    FOREIGN KEY (HabitacionID) REFERENCES Habitacion(ID_habitacion)
);
GO

-- ============================================================================
-- 5. CONSUMOS Y SERVICIOS ADICIONALES
-- ============================================================================

CREATE TABLE ConsumoReserva (
    ConsumoID       INT IDENTITY(1,1) PRIMARY KEY,
    ID_Reserva      INT NOT NULL,
    ID_Servicio     INT NOT NULL,
    Cantidad        INT NOT NULL DEFAULT 1,
    PrecioUnitario  DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (ID_Reserva)  REFERENCES Reserva(ID_Reserva),
    FOREIGN KEY (ID_Servicio) REFERENCES ServiciosPaquete(ID_Servicio)
);
GO

CREATE TABLE ServicioLimpieza (
    AlertaID         INT IDENTITY(1,1) PRIMARY KEY,
    HabitacionID     INT NOT NULL,
    FechaHora        DATETIME NOT NULL DEFAULT GETDATE(),
    Prioridad        VARCHAR(20) NOT NULL DEFAULT 'Normal',
    Atendida         BIT NOT NULL DEFAULT 0,
    UsuarioAtendioID INT NULL,
    FOREIGN KEY (HabitacionID)     REFERENCES Habitacion(ID_habitacion),
    FOREIGN KEY (UsuarioAtendioID) REFERENCES Usuario(UsuarioID)
);
GO

-- ============================================================================
-- 6. ÍNDICES DE APOYO Y OPTIMIZACIÓN
-- ============================================================================
CREATE INDEX IX_Reserva_FechaCheckIn  ON Reserva(FechaCheckIn);
CREATE INDEX IX_Reserva_Estado        ON Reserva(Estado);
CREATE INDEX IX_Bitacora_Fecha        ON Bitacora(Fecha);
CREATE INDEX IX_Bitacora_Criticidad   ON Bitacora(Criticidad);
GO

-- ============================================================================
-- 7. PROCEDIMIENTOS ALMACENADOS TRANSACCIONALES
-- ============================================================================

CREATE PROCEDURE sp_Login
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

    SELECT UsuarioID, NombreUsuario, NombreCompleto, IdiomaPreferido FROM Usuario WHERE UsuarioID = @UsuarioID;
END;
GO

CREATE PROCEDURE sp_Logout
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

CREATE PROCEDURE sp_RegistrarReserva
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

CREATE PROCEDURE sp_RegistrarCheckOut
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

CREATE PROCEDURE sp_AtenderServicioLimpieza
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

CREATE PROCEDURE sp_BackupBaseDatos
    @RutaArchivo VARCHAR(260)
AS
BEGIN
    SET NOCOUNT ON;
    BACKUP DATABASE StayEasyDB TO DISK = @RutaArchivo WITH FORMAT, INIT, NAME = 'Backup completo de StayEasyDB';
END;
GO

CREATE PROCEDURE sp_RestoreBaseDatos
    @RutaArchivo VARCHAR(260)
AS
BEGIN
    SET NOCOUNT ON;
    ALTER DATABASE StayEasyDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    RESTORE DATABASE StayEasyDB FROM DISK = @RutaArchivo WITH REPLACE;
    ALTER DATABASE StayEasyDB SET MULTI_USER;
END;
GO

-- ============================================================================
-- 8. DATOS INICIALES MÍNIMOS
-- ============================================================================
INSERT INTO Idioma (Codigo, Nombre) VALUES ('ES', 'Español'), ('EN', 'English');
GO

INSERT INTO Patente (Nombre, Descripcion, EsFamilia) VALUES ('Administrador', 'Acceso total al sistema', 1);
GO
-- ================================================================
-- MEDICAL REMEMBER - Base de Datos (version en capas)

-- ================================================================

CREATE DATABASE MedicalRememberDB;
GO
USE MedicalRememberDB;
GO

-- ── Pacientes ────────────────────────────────────────────────
CREATE TABLE Pacientes (
    PacienteID      INT IDENTITY(1,1) PRIMARY KEY,
    Nombre          NVARCHAR(100) NOT NULL,
    Apellido        NVARCHAR(100) NOT NULL,
    FechaNacimiento DATE          NOT NULL,
    Telefono        NVARCHAR(20)  NULL,
    Correo          NVARCHAR(150) NULL,
    Direccion       NVARCHAR(200) NULL,
    TipoPaciente    INT           NOT NULL DEFAULT 3,
    FechaRegistro   DATETIME      NOT NULL DEFAULT GETDATE(),
    Activo          BIT           NOT NULL DEFAULT 1
);
GO

-- ── Cuidadores ───────────────────────────────────────────────
CREATE TABLE Cuidadores (
    CuidadorID  INT IDENTITY(1,1) PRIMARY KEY,
    PacienteID  INT           NOT NULL REFERENCES Pacientes(PacienteID),
    Nombre      NVARCHAR(100) NOT NULL,
    Apellido    NVARCHAR(100) NOT NULL,
    Telefono    NVARCHAR(20)  NULL,
    Correo      NVARCHAR(150) NULL,
    Relacion    NVARCHAR(50)  NULL,
    Activo      BIT           NOT NULL DEFAULT 1
);
GO

-- ── Medicamentos ─────────────────────────────────────────────
-- ViaAdministracion es nuevo: Oral, Inyectable, Sublingual, Topica, Otra
CREATE TABLE Medicamentos (
    MedicamentoID     INT IDENTITY(1,1) PRIMARY KEY,
    Nombre            NVARCHAR(150) NOT NULL,
    Descripcion       NVARCHAR(500) NULL,
    Unidad            NVARCHAR(50)  NOT NULL DEFAULT 'mg',
    ViaAdministracion NVARCHAR(20)  NOT NULL DEFAULT 'Oral',
    Activo            BIT           NOT NULL DEFAULT 1
);
GO

-- ── Tratamientos ─────────────────────────────────────────────
CREATE TABLE Tratamientos (
    TratamientoID INT           IDENTITY(1,1) PRIMARY KEY,
    PacienteID    INT           NOT NULL REFERENCES Pacientes(PacienteID),
    MedicamentoID INT           NOT NULL REFERENCES Medicamentos(MedicamentoID),
    Dosis         NVARCHAR(100) NOT NULL,
    FechaInicio   DATE          NOT NULL,
    FechaFin      DATE          NULL,
    Instrucciones NVARCHAR(500) NULL,
    Activo        BIT           NOT NULL DEFAULT 1,
    FechaRegistro DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

-- ── Horarios ─────────────────────────────────────────────────
CREATE TABLE Horarios (
    HorarioID     INT IDENTITY(1,1) PRIMARY KEY,
    TratamientoID INT NOT NULL REFERENCES Tratamientos(TratamientoID),
    HoraToma      TIME NOT NULL,
    DiaSemana     INT  NULL,
    Activo        BIT  NOT NULL DEFAULT 1
);
GO

-- ── Historial de Cumplimiento ────────────────────────────────
CREATE TABLE HistorialCumplimiento (
    HistorialID         INT IDENTITY(1,1) PRIMARY KEY,
    HorarioID           INT      NOT NULL REFERENCES Horarios(HorarioID),
    FechaHoraProgramada DATETIME NOT NULL,
    FechaHoraTomado     DATETIME NULL,
    Estado              INT      NOT NULL DEFAULT 4,
    Observacion         NVARCHAR(300) NULL
);
GO

-- ── Usuarios (cuentas de acceso al sistema) ─────────────────
-- Contrasena y respuestas de seguridad se guardan como hash SHA-256
-- (nunca en texto plano). El programa se encarga de hashear antes de
-- guardar o comparar; aqui solo se define la estructura.
CREATE TABLE Usuarios (
    UsuarioID           INT IDENTITY(1,1) NOT NULL,
    NombreUsuario        NVARCHAR(50)  NOT NULL,
    Contrasena           NVARCHAR(100) NOT NULL,   -- hash SHA-256 (64 caracteres hex)
    NombreCompleto        NVARCHAR(150) NOT NULL,
    Rol                  NVARCHAR(30)  NOT NULL DEFAULT 'Cuidador',
    RespuestaMejorAmigo  NVARCHAR(100) NULL,        -- hash SHA-256
    RespuestaColorFav    NVARCHAR(100) NULL,        -- hash SHA-256
    Activo               BIT           NOT NULL DEFAULT 1,
    FechaRegistro        DATETIME      NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_Usuarios PRIMARY KEY (UsuarioID),
    CONSTRAINT UQ_Usuarios_NombreUsuario UNIQUE (NombreUsuario)
);
GO

-- Usuarios de prueba. Las contrasenas y respuestas van con hash SHA-256,

INSERT INTO Usuarios
    (NombreUsuario, Contrasena, NombreCompleto, Rol, RespuestaMejorAmigo, RespuestaColorFav)
VALUES
    ('admin', '03ac674216f3e15c761ee1a5e255f067953623c8b388b4459e13f978d7c846f4',
     'Administrador General', 'Administrador',
     'ed08c290d7e22f7bb324b15cbadce35b0b348564fd2d5f95752388d86d71bcca',
     'a4bd1d3a69aa0ea6ffb1298c8c26be4b333526cae7d27f2362f89857157701ce'),
    ('carlos', 'ac9c2c34c9f7ad52528c3422af40a66e2e24aaf2a727831255413c9470158984',
     'Carlos Garcia', 'Cuidador',
     'ee5cd7d5d96c8874117891b2c92a036f96918e66c102bc698ae77542c186f981',
     'ec061fafb777f2943027f2deb3c17961556c386cad45065f343a74aea1177ad0');
GO

-- ── Datos de prueba (pacientes / tratamientos / horarios) ────
INSERT INTO Pacientes (Nombre, Apellido, FechaNacimiento, Telefono, Correo, TipoPaciente)
VALUES ('Maria', 'Garcia Lopez', '1955-03-15', '8888-1234', 'maria@email.com', 1);
GO

INSERT INTO Cuidadores (PacienteID, Nombre, Apellido, Telefono, Relacion)
VALUES (1, 'Carlos', 'Garcia', '7777-5678', 'Hijo');
GO

-- ================================================================
-- TRIGGERS
-- ================================================================
-- Tabla de auditoria usada por el trigger de Usuarios
CREATE TABLE AuditoriaUsuarios (
    AuditoriaID    INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioID      INT           NOT NULL,
    NombreUsuario  NVARCHAR(50)  NOT NULL,
    Accion         NVARCHAR(50)  NOT NULL,
    FechaHora      DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

-- ── 1) Pacientes: evita el borrado fisico (protege el historico) ──
-- Si alguien ejecuta DELETE FROM Pacientes, en lugar de borrar la fila
-- el trigger la desactiva (Activo = 0) y desactiva en cascada sus
-- tratamientos y horarios, para no perder el historial de cumplimiento.
CREATE TRIGGER TR_Pacientes_NoEliminar
ON Pacientes
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE p SET p.Activo = 0
    FROM Pacientes p
    INNER JOIN deleted d ON p.PacienteID = d.PacienteID;

    UPDATE t SET t.Activo = 0
    FROM Tratamientos t
    INNER JOIN deleted d ON t.PacienteID = d.PacienteID;

    UPDATE h SET h.Activo = 0
    FROM Horarios h
    INNER JOIN Tratamientos t ON h.TratamientoID = t.TratamientoID
    INNER JOIN deleted d      ON t.PacienteID     = d.PacienteID;
END;
GO

-- ── 2) HistorialCumplimiento: marca automaticamente una toma como  ──
-- "Tardio" (Estado = 3) si se registro mas de 30 minutos despues de
-- la hora programada. Refuerza la regla aunque la aplicacion falle
-- en aplicarla.
CREATE TRIGGER TR_Historial_MarcarTardio
ON HistorialCumplimiento
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE hc SET hc.Estado = 3
    FROM HistorialCumplimiento hc
    INNER JOIN inserted i ON hc.HistorialID = i.HistorialID
    WHERE i.Estado = 1
      AND i.FechaHoraTomado IS NOT NULL
      AND DATEDIFF(MINUTE, i.FechaHoraProgramada, i.FechaHoraTomado) > 30;
END;
GO

-- ── 3) Usuarios: audita cada vez que cambia la contrasena ──────────
-- Util para trazabilidad de seguridad (por ejemplo tras usar
-- "Recuperar Contrasena").
CREATE TRIGGER TR_Usuarios_AuditarCambioClave
ON Usuarios
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF UPDATE(Contrasena)
    BEGIN
        INSERT INTO AuditoriaUsuarios (UsuarioID, NombreUsuario, Accion)
        SELECT i.UsuarioID, i.NombreUsuario, 'Cambio de contrasena'
        FROM inserted i
        INNER JOIN deleted d ON i.UsuarioID = d.UsuarioID
        WHERE i.Contrasena <> d.Contrasena;
    END
END;
GO

-- ── 4) Tratamientos: valida a nivel de base de datos que la fecha  ──
-- de fin no sea anterior a la fecha de inicio (defensa adicional,
-- aunque la capa de Negocio ya lo valida antes de llegar aqui).
CREATE TRIGGER TR_Tratamientos_ValidarFechas
ON Tratamientos
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted WHERE FechaFin IS NOT NULL AND FechaFin < FechaInicio)
    BEGIN
        RAISERROR('La fecha de fin no puede ser anterior a la fecha de inicio.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;
GO

PRINT 'Base de datos MedicalRememberDB creada correctamente.';
PRINT 'Ahora ejecuta Database\ImportarMedicamentos.sql para cargar el catalogo de medicamentos comunes.';
GO

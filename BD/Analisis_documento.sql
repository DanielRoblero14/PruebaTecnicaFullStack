USE master;
GO

CREATE DATABASE ControlAsistenciaPrueba;
GO

USE ControlAsistenciaPrueba;
GO

-- TABLAS DEL SISTEMA ACTUAL

CREATE TABLE dbo.Departamentos
(
    Id INT IDENTITY(1,1) NOT NULL,
    Codigo NVARCHAR(20) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(500) NULL,

    CONSTRAINT PK_Departamentos PRIMARY KEY (Id),
    CONSTRAINT UQ_Departamentos_Codigo UNIQUE (Codigo),

    CONSTRAINT CK_Departamentos_Campos CHECK
    (
        LEN(LTRIM(RTRIM(Codigo))) > 0
        AND LEN(LTRIM(RTRIM(Nombre))) > 0
    )
);
GO

CREATE SEQUENCE dbo.SecuenciaCodigoEmpleado
    AS BIGINT
    START WITH 1
    INCREMENT BY 1
    NO CYCLE;
GO

CREATE TABLE dbo.Empleados
(
    Id INT IDENTITY(1,1) NOT NULL,
    Codigo NVARCHAR(30) NOT NULL,
    Nombres NVARCHAR(100) NOT NULL,
    Apellidos NVARCHAR(100) NOT NULL,
    FechaNacimiento DATE NULL,
    DepartamentoId INT NOT NULL,

    CONSTRAINT PK_Empleados PRIMARY KEY (Id),
    CONSTRAINT UQ_Empleados_Codigo UNIQUE (Codigo),

    CONSTRAINT FK_Empleados_Departamentos
        FOREIGN KEY (DepartamentoId)
        REFERENCES dbo.Departamentos(Id),

    CONSTRAINT CK_Empleados_Campos CHECK
    (
        LEN(LTRIM(RTRIM(Nombres))) > 0
        AND LEN(LTRIM(RTRIM(Apellidos))) > 0
    )
);
GO

-- TABLAS PROPUESTAS PARA ASISTENCIA

CREATE TABLE dbo.CentrosTrabajo
(
    Id INT IDENTITY(1,1) NOT NULL,
    Codigo NVARCHAR(20) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Ubicacion NVARCHAR(250) NULL,
    NombreEncargado NVARCHAR(150) NOT NULL,
    Telefono NVARCHAR(30) NOT NULL,
    Activo BIT NOT NULL
        CONSTRAINT DF_CentrosTrabajo_Activo DEFAULT (1),

    CONSTRAINT PK_CentrosTrabajo PRIMARY KEY (Id),
    CONSTRAINT UQ_CentrosTrabajo_Codigo UNIQUE (Codigo),

    CONSTRAINT CK_CentrosTrabajo_Campos CHECK
    (
        LEN(LTRIM(RTRIM(Codigo))) > 0
        AND LEN(LTRIM(RTRIM(Nombre))) > 0
        AND LEN(LTRIM(RTRIM(NombreEncargado))) > 0
        AND LEN(LTRIM(RTRIM(Telefono))) > 0
    )
);
GO

CREATE TABLE dbo.PlanificacionesMensuales
(
    Id INT IDENTITY(1,1) NOT NULL,
    Anio SMALLINT NOT NULL,
    Mes TINYINT NOT NULL,
    Observaciones NVARCHAR(500) NULL,

    CONSTRAINT PK_PlanificacionesMensuales PRIMARY KEY (Id),

    -- Una planificación por año y mes.
    CONSTRAINT UQ_PlanificacionesMensuales_Periodo
        UNIQUE (Anio, Mes),

    CONSTRAINT CK_PlanificacionesMensuales_Anio
        CHECK (Anio BETWEEN 1 AND 9999),

    CONSTRAINT CK_PlanificacionesMensuales_Mes
        CHECK (Mes BETWEEN 1 AND 12)
);
GO

CREATE TABLE dbo.Asignaciones
(
    Id INT IDENTITY(1,1) NOT NULL,
    PlanificacionId INT NOT NULL,
    EmpleadoId INT NOT NULL,
    CentroTrabajoId INT NOT NULL,
    Observaciones NVARCHAR(500) NULL,

    CONSTRAINT PK_Asignaciones PRIMARY KEY (Id),

    -- Un centro por empleado dentro de cada mes.
    CONSTRAINT UQ_Asignaciones_PlanificacionEmpleado
        UNIQUE (PlanificacionId, EmpleadoId),

    CONSTRAINT FK_Asignaciones_Planificaciones
        FOREIGN KEY (PlanificacionId)
        REFERENCES dbo.PlanificacionesMensuales(Id),

    CONSTRAINT FK_Asignaciones_Empleados
        FOREIGN KEY (EmpleadoId)
        REFERENCES dbo.Empleados(Id),

    CONSTRAINT FK_Asignaciones_Centros
        FOREIGN KEY (CentroTrabajoId)
        REFERENCES dbo.CentrosTrabajo(Id)
);
GO

CREATE TABLE dbo.VerificacionesTelefonicas
(
    Id INT IDENTITY(1,1) NOT NULL,
    CentroTrabajoId INT NOT NULL,
    FechaAsistencia DATE NOT NULL,
    FechaHoraLlamada DATETIME2(0) NOT NULL
        CONSTRAINT DF_Verificaciones_FechaHora
        DEFAULT (SYSDATETIME()),
    PersonaContactada NVARCHAR(150) NULL,
    Resultado NVARCHAR(20) NOT NULL,
    Observaciones NVARCHAR(500) NULL,

    CONSTRAINT PK_VerificacionesTelefonicas PRIMARY KEY (Id),

    CONSTRAINT FK_Verificaciones_Centros
        FOREIGN KEY (CentroTrabajoId)
        REFERENCES dbo.CentrosTrabajo(Id),

    CONSTRAINT CK_Verificaciones_Resultado CHECK
    (
        Resultado IN
        (
            N'Contactado',
            N'Sin respuesta',
            N'No disponible'
        )
    ),

    CONSTRAINT CK_Verificaciones_Persona CHECK
    (
        Resultado <> N'Contactado'
        OR
        (
            PersonaContactada IS NOT NULL
            AND LEN(LTRIM(RTRIM(PersonaContactada))) > 0
        )
    )
);
GO

CREATE TABLE dbo.Asistencias
(
    Id INT IDENTITY(1,1) NOT NULL,
    AsignacionId INT NOT NULL,
    FechaAsistencia DATE NOT NULL,
    Estado NVARCHAR(25) NOT NULL
        CONSTRAINT DF_Asistencias_Estado
        DEFAULT (N'Pendiente de confirmar'),
    MotivoAusencia NVARCHAR(200) NULL,
    Observaciones NVARCHAR(500) NULL,
    VerificacionTelefonicaId INT NULL,
    FechaActualizacion DATETIME2(0) NOT NULL
        CONSTRAINT DF_Asistencias_Actualizacion
        DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_Asistencias PRIMARY KEY (Id),

    -- Un registro por asignación y día.
    CONSTRAINT UQ_Asistencias_AsignacionFecha
        UNIQUE (AsignacionId, FechaAsistencia),

    CONSTRAINT FK_Asistencias_Asignaciones
        FOREIGN KEY (AsignacionId)
        REFERENCES dbo.Asignaciones(Id),

    CONSTRAINT FK_Asistencias_Verificaciones
        FOREIGN KEY (VerificacionTelefonicaId)
        REFERENCES dbo.VerificacionesTelefonicas(Id),

    CONSTRAINT CK_Asistencias_Estado CHECK
    (
        Estado IN
        (
            N'Pendiente de confirmar',
            N'Presente',
            N'Ausente'
        )
    ),

    CONSTRAINT CK_Asistencias_Confirmacion CHECK
    (
        (
            Estado = N'Pendiente de confirmar'
            AND VerificacionTelefonicaId IS NULL
        )
        OR
        (
            Estado IN (N'Presente', N'Ausente')
            AND VerificacionTelefonicaId IS NOT NULL
        )
    ),

    CONSTRAINT CK_Asistencias_Motivo CHECK
    (
        Estado = N'Ausente'
        OR MotivoAusencia IS NULL
    )
);

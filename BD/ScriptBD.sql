USE master;
GO

CREATE DATABASE ControlEmpleados;
GO

USE ControlEmpleados;
GO

CREATE TABLE Departamentos
(
    Id INT IDENTITY(1,1) NOT NULL,
    Codigo NVARCHAR(20) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(500) NULL,

    CONSTRAINT PK_Departamentos PRIMARY KEY (Id),

    CONSTRAINT UQ_Departamentos_Codigo UNIQUE (Codigo),

    CONSTRAINT CK_Departamentos_Codigo
        CHECK (LEN(LTRIM(RTRIM(Codigo))) > 0),

    CONSTRAINT CK_Departamentos_Nombre
        CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0)
);
GO

-- Genera el número que el backend usará para formar EMP-0001, y ect.
CREATE SEQUENCE dbo.SecuenciaCodigoEmpleado
    AS BIGINT
    START WITH 1
    INCREMENT BY 1
    NO CYCLE;
GO

CREATE TABLE Empleados
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
        REFERENCES Departamentos(Id)
        ON DELETE NO ACTION,

    CONSTRAINT CK_Empleados_Nombres
        CHECK (LEN(LTRIM(RTRIM(Nombres))) > 0),

    CONSTRAINT CK_Empleados_Apellidos
        CHECK (LEN(LTRIM(RTRIM(Apellidos))) > 0)
);
GO

CREATE INDEX IX_Empleados_DepartamentoId
    ON Empleados(DepartamentoId);
GO

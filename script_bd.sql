USE master;
GO

ALTER DATABASE SistemaAlertaTemprana SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

DROP DATABASE SistemaAlertaTemprana;
GO

CREATE DATABASE SistemaAlertaTemprana;
GO

USE SistemaAlertaTemprana;
GO

CREATE TABLE Comisiones (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL UNIQUE
);
GO

CREATE TABLE Estudiantes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    DNI VARCHAR(20) NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Apellido VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NULL,
    ComisionId INT NOT NULL,
    CONSTRAINT UQ_Estudiantes_DNI_Comision UNIQUE (DNI, ComisionId),
    CONSTRAINT FK_Estudiantes_Comisiones FOREIGN KEY (ComisionId) REFERENCES Comisiones(Id)
);
GO

CREATE TABLE Clases (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Fecha DATE NOT NULL,
    ComisionId INT NOT NULL,
    Descripcion VARCHAR(200) NULL,
    CONSTRAINT UQ_Clases_Comision_Fecha UNIQUE (ComisionId, Fecha),
    CONSTRAINT FK_Clases_Comisiones FOREIGN KEY (ComisionId) REFERENCES Comisiones(Id)
);
GO

CREATE TABLE TrabajosPracticos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ComisionId INT NOT NULL,
    Titulo VARCHAR(150) NOT NULL,
    FechaEntrega DATE NOT NULL,
    CONSTRAINT FK_TrabajosPracticos_Comisiones FOREIGN KEY (ComisionId) REFERENCES Comisiones(Id)
);
GO

CREATE TABLE Asistencias (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EstudianteId INT NOT NULL,
    ClaseId INT NOT NULL,
    Presente BIT NOT NULL,
    Justificada BIT NOT NULL DEFAULT 0,
    CONSTRAINT UQ_Asistencias_Estudiante_Clase UNIQUE (EstudianteId, ClaseId),
    CONSTRAINT FK_Asistencias_Estudiantes FOREIGN KEY (EstudianteId) REFERENCES Estudiantes(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Asistencias_Clases FOREIGN KEY (ClaseId) REFERENCES Clases(Id) ON DELETE CASCADE
);
GO

CREATE TABLE Entregas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EstudianteId INT NOT NULL,
    TrabajoPracticoId INT NOT NULL,
    Entregado BIT NOT NULL,
    CONSTRAINT UQ_Entregas_Estudiante_Trabajo UNIQUE (EstudianteId, TrabajoPracticoId),
    CONSTRAINT FK_Entregas_Estudiantes FOREIGN KEY (EstudianteId) REFERENCES Estudiantes(Id),
    CONSTRAINT FK_Entregas_TrabajosPracticos FOREIGN KEY (TrabajoPracticoId) REFERENCES TrabajosPracticos(Id)
);
GO

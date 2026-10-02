--base de datos para el esqueleto mínimo
CREATE DATABASE SistemaAlertaTemprana;
GO

USE SistemaAlertaTemprana;
GO

-- Tabla Estudiantes (Para la historia H1)
CREATE TABLE Estudiantes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Apellido VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NULL
);
GO

-- Tabla Clases (Para las fechas del calendario)
CREATE TABLE Clases (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Fecha DATE NOT NULL
);
GO

--Tabla Asistencias (Para la historia H3)
CREATE TABLE Asistencias (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EstudianteId INT NOT NULL,
    ClaseId INT NOT NULL,
    Presente BIT NOT NULL, -- 0 = Ausente, 1 = Presente
    
    CONSTRAINT FK_Asistencias_Estudiantes FOREIGN KEY (EstudianteId) REFERENCES Estudiantes(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Asistencias_Clases FOREIGN KEY (ClaseId) REFERENCES Clases(Id) ON DELETE CASCADE
);
GO
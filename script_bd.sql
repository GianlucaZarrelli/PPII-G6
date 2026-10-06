USE master;
GO

CREATE TABLE Comisiones (
    Id INT IDENTITY PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE Estudiantes (
    Id INT IDENTITY PRIMARY KEY,
    DNI VARCHAR(20) NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Apellido VARCHAR(100) NOT NULL,
    ComisionId INT NOT NULL,
    UNIQUE (DNI, ComisionId),
    FOREIGN KEY (ComisionId) REFERENCES Comisiones(Id)
);

CREATE TABLE Clases (
    Id INT IDENTITY PRIMARY KEY,
    Fecha DATE NOT NULL,
    ComisionId INT NOT NULL,
    Descripcion VARCHAR(200) NULL,
    UNIQUE (ComisionId, Fecha),
    FOREIGN KEY (ComisionId) REFERENCES Comisiones(Id)
);

CREATE TABLE TrabajosPracticos (
    Id INT IDENTITY PRIMARY KEY,
    ComisionId INT NOT NULL,
    Titulo VARCHAR(150) NOT NULL,
    FechaEntrega DATE NOT NULL,
    FOREIGN KEY (ComisionId) REFERENCES Comisiones(Id)
);

CREATE TABLE Asistencias (
    Id INT IDENTITY PRIMARY KEY,
    EstudianteId INT NOT NULL,
    ClaseId INT NOT NULL,
    Justificada BIT NOT NULL DEFAULT 0,
    UNIQUE (EstudianteId, ClaseId),
    FOREIGN KEY (EstudianteId) REFERENCES Estudiantes(Id),
    FOREIGN KEY (ClaseId) REFERENCES Clases(Id)
);

CREATE TABLE Entregas (
    Id INT IDENTITY PRIMARY KEY,
    EstudianteId INT NOT NULL,
    TrabajoPracticoId INT NOT NULL,
    Entregado BIT NOT NULL,
    UNIQUE (EstudianteId, TrabajoPracticoId),
    FOREIGN KEY (EstudianteId) REFERENCES Estudiantes(Id),
    FOREIGN KEY (TrabajoPracticoId) REFERENCES TrabajosPracticos(Id)
);
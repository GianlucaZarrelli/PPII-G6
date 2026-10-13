-- ============================================================
-- Datos de demostración: 12 alumnos en las comisiones A, B y C,
-- con 6 clases y 3 trabajos prácticos por comisión.
-- Se puede volver a correr: borra y recrea SOLO los datos de A, B y C.
-- Las fechas se calculan hacia atrás desde hoy: conviene correrlo
-- el día de la demo o el anterior.
-- ============================================================
USE SistemaAlertaTemprana;
SET NOCOUNT ON;
SET XACT_ABORT ON;  -- si algo falla, se deshace todo

BEGIN TRANSACTION;

-- 1. Comisiones A, B y C (se crean solo si faltan)
INSERT INTO Comisiones (Nombre)
SELECT v.Nombre
FROM (VALUES ('A'), ('B'), ('C')) v(Nombre)
WHERE NOT EXISTS (SELECT 1 FROM Comisiones c WHERE c.Nombre = v.Nombre);

DECLARE @Ids TABLE (Id INT);
INSERT INTO @Ids SELECT Id FROM Comisiones WHERE Nombre IN ('A', 'B', 'C');

-- 2. Limpia los datos de A, B y C, para poder correr el script más de una vez
DELETE FROM Entregas
WHERE EstudianteId IN (SELECT Id FROM Estudiantes WHERE ComisionId IN (SELECT Id FROM @Ids))
   OR TrabajoPracticoId IN (SELECT Id FROM TrabajosPracticos WHERE ComisionId IN (SELECT Id FROM @Ids));

DELETE FROM Asistencias
WHERE EstudianteId IN (SELECT Id FROM Estudiantes WHERE ComisionId IN (SELECT Id FROM @Ids))
   OR ClaseId IN (SELECT Id FROM Clases WHERE ComisionId IN (SELECT Id FROM @Ids));

DELETE FROM Estudiantes       WHERE ComisionId IN (SELECT Id FROM @Ids);
DELETE FROM TrabajosPracticos WHERE ComisionId IN (SELECT Id FROM @Ids);
DELETE FROM Clases            WHERE ComisionId IN (SELECT Id FROM @Ids);

-- 3. Seis clases semanales ya pasadas por comisión (hoy queda libre para cargar una en vivo)
INSERT INTO Clases (Fecha, ComisionId, Descripcion)
SELECT d.Fecha, c.Id, CONCAT('Clase ', FORMAT(d.Fecha, 'dd/MM/yyyy'))
FROM Comisiones c
CROSS JOIN (SELECT CAST(DATEADD(DAY, -7 * n, GETDATE()) AS DATE) AS Fecha
            FROM (VALUES (1), (2), (3), (4), (5), (6)) v(n)) d
WHERE c.Id IN (SELECT Id FROM @Ids);

-- 4. Tres trabajos prácticos por comisión
INSERT INTO TrabajosPracticos (ComisionId, Titulo, FechaEntrega)
SELECT c.Id, t.Titulo, CAST(DATEADD(DAY, t.Dias, GETDATE()) AS DATE)
FROM Comisiones c
CROSS JOIN (VALUES ('TP1', -35), ('TP2', -21), ('TP3', -7)) t(Titulo, Dias)
WHERE c.Id IN (SELECT Id FROM @Ids);

-- 5. Alumnos: Presentes = de 6 clases, Entregados = de 3 trabajos
CREATE TABLE #Alumnos (
    Comision VARCHAR(100), Dni VARCHAR(20), Nombre VARCHAR(100),
    Apellido VARCHAR(100), Email VARCHAR(100), Presentes INT, Entregados INT);

INSERT INTO #Alumnos VALUES
 -- Comisión A
 ('A', '90000001', N'Lucía',     N'Fernández', 'lucia.fernandez@demo.test',   6, 3),  -- verde
 ('A', '90000002', N'Mateo',     N'Gómez',     'mateo.gomez@demo.test',       5, 3),  -- amarillo (asistencia)
 ('A', '90000003', N'Valentina', N'Díaz',      'valentina.diaz@demo.test',    4, 3),  -- rojo (asistencia)
 ('A', '90000004', N'Joaquín',   N'Pérez',     'joaquin.perez@demo.test',     6, 1),  -- rojo (solo entregas)
 ('A', '90000005', N'Sofía',     N'Benítez',   'sofia.benitez@demo.test',     6, 3),  -- verde
 -- Comisión B
 ('B', '90000006', N'Martina',   N'López',     'martina.lopez@demo.test',     6, 3),  -- verde
 ('B', '90000007', N'Camila',    N'Torres',    'camila.torres@demo.test',     6, 2),  -- amarillo (entregas)
 ('B', '90000008', N'Tomás',     N'Silva',     'tomas.silva@demo.test',       6, 3),  -- verde
 ('B', '90000009', N'Santiago',  N'Ruiz',      'santiago.ruiz@demo.test',     3, 0),  -- rojo, el más grave
 -- Comisión C
 ('C', '90000010', N'Nicolás',   N'Acosta',    'nicolas.acosta@demo.test',    6, 3),  -- verde
 ('C', '90000011', N'Julieta',   N'Herrera',   'julieta.herrera@demo.test',   5, 2),  -- amarillo (las dos señales)
 ('C', '90000012', N'Facundo',   N'Medina',    'facundo.medina@demo.test',    4, 2);  -- rojo (asistencia)

INSERT INTO Estudiantes (DNI, Nombre, Apellido, Email, ComisionId)
SELECT a.Dni, a.Nombre, a.Apellido, a.Email, c.Id
FROM #Alumnos a
JOIN Comisiones c ON c.Nombre = a.Comision;

-- 6.

-- ============================================================
-- Datos de demostración: 12 alumnos en las comisiones A, B y C,
-- con 6 clases y 3 trabajos prácticos por comisión, y algunos contactos.
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
DELETE FROM Contactos
WHERE EstudianteId IN (SELECT Id FROM Estudiantes WHERE ComisionId IN (SELECT Id FROM @Ids));

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

INSERT INTO Estudiantes (DNI, Nombre, Apellido, Email, ComisionId, FechaAlta)
SELECT a.Dni, a.Nombre, a.Apellido, a.Email, c.Id, CAST(DATEADD(DAY, -60, GETDATE()) AS DATE)
FROM #Alumnos a
         JOIN Comisiones c ON c.Nombre = a.Comision;

-- 6. Asistencias: las ausencias caen en las clases más recientes ("dejó de venir")
INSERT INTO Asistencias (EstudianteId, ClaseId, Presente, Justificada)
SELECT e.Id, cl.Id, CASE WHEN cl.Orden <= a.Presentes THEN 1 ELSE 0 END, 0
FROM #Alumnos a
         JOIN Comisiones c ON c.Nombre = a.Comision
         JOIN Estudiantes e ON e.DNI = a.Dni AND e.ComisionId = c.Id
         JOIN (SELECT Id, ComisionId,
                      ROW_NUMBER() OVER (PARTITION BY ComisionId ORDER BY Fecha) AS Orden
               FROM Clases) cl ON cl.ComisionId = c.Id;

-- 7. Entregas: se entregan primero los trabajos más antiguos
INSERT INTO Entregas (EstudianteId, TrabajoPracticoId, Entregado)
SELECT e.Id, t.Id, CASE WHEN t.Orden <= a.Entregados THEN 1 ELSE 0 END
FROM #Alumnos a
         JOIN Comisiones c ON c.Nombre = a.Comision
         JOIN Estudiantes e ON e.DNI = a.Dni AND e.ComisionId = c.Id
         JOIN (SELECT Id, ComisionId,
                      ROW_NUMBER() OVER (PARTITION BY ComisionId ORDER BY FechaEntrega) AS Orden
               FROM TrabajosPracticos) t ON t.ComisionId = c.Id;

-- 8. Contactos de ejemplo: algunos alumnos en riesgo ya fueron contactados y otros no
--    (Mateo, Facundo y los demás quedan "sin contactar"). Las horas son minutos desde la medianoche.
CREATE TABLE #Contactos (
                            Comision VARCHAR(100), Dni VARCHAR(20), DiasAtras INT, Minutos INT,
                            Medio VARCHAR(20), Resultado VARCHAR(500));

INSERT INTO #Contactos VALUES
                           ('A', '90000004', 9,  960, 'Mail',       N'No respondió el mail.'),
                           ('A', '90000004', 2,  630, 'Llamada',    N'Atendió. Dice que no tuvo tiempo para los TP y se compromete a entregarlos esta semana.'),
                           ('A', '90000003', 3, 1095, 'Mensaje',    N'Respondió por WhatsApp: trabaja por la tarde. Se le pasó el material de las clases perdidas.'),
                           ('B', '90000009', 1, 1200, 'Presencial', N'Habló al terminar la clase. Va a ponerse al día con el TP2.');

INSERT INTO Contactos (EstudianteId, FechaHora, Medio, Resultado)
SELECT e.Id,
       DATEADD(MINUTE, k.Minutos, CAST(DATEADD(DAY, -k.DiasAtras, CAST(GETDATE() AS DATE)) AS DATETIME2(0))),
       k.Medio, k.Resultado
FROM #Contactos k
         JOIN Comisiones c ON c.Nombre = k.Comision
         JOIN Estudiantes e ON e.DNI = k.Dni AND e.ComisionId = c.Id;

DROP TABLE #Contactos;
DROP TABLE #Alumnos;

COMMIT TRANSACTION;

-- Verificación
SELECT c.Nombre AS Comision,
       (SELECT COUNT(*) FROM Estudiantes e WHERE e.ComisionId = c.Id) AS Alumnos,
       (SELECT COUNT(*) FROM Clases cl WHERE cl.ComisionId = c.Id) AS Clases,
       (SELECT COUNT(*) FROM TrabajosPracticos t WHERE t.ComisionId = c.Id) AS Trabajos,
       (SELECT COUNT(*) FROM Contactos ct JOIN Estudiantes e ON e.Id = ct.EstudianteId WHERE e.ComisionId = c.Id) AS Contactos
FROM Comisiones c
ORDER BY c.Nombre;
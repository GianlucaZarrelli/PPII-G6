-- ============================================================
-- Mantenimiento - Vacía TODOS los datos, reinicia los ids y deja
-- las comisiones A, B y C. NO recrea la base ni el usuario.
-- Útil para ensayar la demo. No se puede deshacer.
-- Ejecutar con una cuenta administradora (app_maui no tiene
-- permiso para DBCC CHECKIDENT).
-- ============================================================
USE SistemaAlertaTemprana;
SET XACT_ABORT ON;

BEGIN TRANSACTION;
DELETE FROM Contactos;
DELETE FROM Entregas;
DELETE FROM Asistencias;
DELETE FROM Estudiantes;
DELETE FROM TrabajosPracticos;
DELETE FROM Clases;
DELETE FROM Comisiones;
COMMIT TRANSACTION;

-- Reinicia los contadores: la próxima fila de cada tabla va a tener Id = 1
DBCC CHECKIDENT ('Contactos', RESEED, 0);
DBCC CHECKIDENT ('Entregas', RESEED, 0);
DBCC CHECKIDENT ('Asistencias', RESEED, 0);
DBCC CHECKIDENT ('Estudiantes', RESEED, 0);
DBCC CHECKIDENT ('TrabajosPracticos', RESEED, 0);
DBCC CHECKIDENT ('Clases', RESEED, 0);
DBCC CHECKIDENT ('Comisiones', RESEED, 0);

INSERT INTO Comisiones (Nombre) VALUES ('A'), ('B'), ('C');

SELECT * FROM Comisiones;  -- tiene que mostrar A = 1, B = 2, C = 3
-- ============================================================
-- Crea las comisiones A, B y C (solo las que falten).
-- Se puede ejecutar más de una vez sin duplicar.
-- ============================================================
USE SistemaAlertaTemprana;
GO

INSERT INTO Comisiones (Nombre)
SELECT v.Nombre
FROM (VALUES ('A'), ('B'), ('C')) v(Nombre)
WHERE NOT EXISTS (SELECT 1 FROM Comisiones c WHERE c.Nombre = v.Nombre);
GO

SELECT * FROM Comisiones ORDER BY Nombre;
GO

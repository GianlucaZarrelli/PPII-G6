USE SistemaAlertaTemprana;
GO

-- Hace opcional la fecha de incorporación (R2 recuperatorio).
-- Si la columna ya admite NULL, el ALTER no falla: se puede correr más de una vez.

IF EXISTS (
    SELECT 1
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.Estudiantes')
      AND c.name = N'FechaAlta'
)
BEGIN
    DECLARE @nombreDefault sysname;
    SELECT @nombreDefault = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.Estudiantes')
      AND c.name = N'FechaAlta';

    EXEC(N'ALTER TABLE dbo.Estudiantes DROP CONSTRAINT [' + @nombreDefault + N']');
END
GO

ALTER TABLE dbo.Estudiantes ALTER COLUMN FechaAlta DATE NULL;
GO

-- ============================================================
-- Agrega la tabla de umbrales de riesgo a una base que ya existe.
-- NO borra nada: sirve para no tener que recrear la base con crear_base.sql.
-- Se puede correr más de una vez: si la tabla ya está, no hace nada.
-- Ejecutar con una cuenta administradora.
-- ============================================================
USE SistemaAlertaTemprana;
GO

IF OBJECT_ID(N'dbo.UmbralesRiesgo', N'U') IS NULL
BEGIN
CREATE TABLE UmbralesRiesgo (
                                Id INT NOT NULL PRIMARY KEY,
                                AsistenciaRojo INT NOT NULL,
                                EntregasRojo INT NOT NULL,
                                AsistenciaAmarillo INT NOT NULL,
                                EntregasAmarillo INT NOT NULL,
                                CONSTRAINT CK_UmbralesRiesgo_UnaSolaFila CHECK (Id = 1),
                                CONSTRAINT CK_UmbralesRiesgo_Porcentajes CHECK (
                                    AsistenciaRojo BETWEEN 0 AND 100 AND EntregasRojo BETWEEN 0 AND 100
                                        AND AsistenciaAmarillo BETWEEN 0 AND 100 AND EntregasAmarillo BETWEEN 0 AND 100),
                                CONSTRAINT CK_UmbralesRiesgo_Orden CHECK (AsistenciaRojo < AsistenciaAmarillo AND EntregasRojo < EntregasAmarillo)
);
END
GO

IF NOT EXISTS (SELECT 1 FROM UmbralesRiesgo)
    INSERT INTO UmbralesRiesgo (Id, AsistenciaRojo, EntregasRojo, AsistenciaAmarillo, EntregasAmarillo)
    VALUES (1, 75, 60, 85, 80);
GO

SELECT * FROM UmbralesRiesgo;  -- tiene que mostrar una fila: 75, 60, 85, 80
GO
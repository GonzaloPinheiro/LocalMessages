-- Crear la base de datos
CREATE DATABASE ChatMensajesDb;
GO

-- Usar la base de datos
USE ChatMensajesDb;
GO

-- Crear tabla de mensajes
CREATE TABLE MensajesChat (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Remitente NVARCHAR(100) NOT NULL,
    Receptor NVARCHAR(100) NULL, -- NULL = mensaje público
    Contenido NVARCHAR(MAX) NOT NULL,
    FechaEnvio DATETIME NOT NULL DEFAULT GETDATE()
);
GO

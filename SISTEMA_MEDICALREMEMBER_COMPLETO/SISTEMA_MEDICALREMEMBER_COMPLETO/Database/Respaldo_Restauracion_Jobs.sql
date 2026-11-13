-- =====================================================================
-- MEDICAL REMEMBER - ABD: respaldo, restauración, jobs y bitácora
-- =====================================================================

USE master;
GO
-- Modelo de recuperación COMPLETO: necesario para respaldar el log transaccional
ALTER DATABASE MedicalRememberDB SET RECOVERY FULL;
GO
-- Carpeta de respaldos (cámbiala por un disco distinto al de la base de datos)
EXEC master.dbo.xp_create_subdir N'C:\Respaldos\MedicalRemember';
GO

USE MedicalRememberDB;
GO

-- ---------------------------------------------------------------------
-- 1) BITÁCORA de respaldos
-- ---------------------------------------------------------------------
IF OBJECT_ID('dbo.BitacoraRespaldos') IS NULL
CREATE TABLE dbo.BitacoraRespaldos (
    BitacoraID INT IDENTITY(1,1) PRIMARY KEY,
    Fecha      DATETIME      NOT NULL DEFAULT GETDATE(),
    Tipo       VARCHAR(20)   NOT NULL,            -- FULL / DIFERENCIAL / LOG
    Archivo    NVARCHAR(400) NULL,
    Resultado  VARCHAR(10)   NOT NULL,            -- OK / ERROR
    Detalle    NVARCHAR(1000) NULL,
    Usuario    SYSNAME       NOT NULL DEFAULT SUSER_SNAME()
);
GO

-- ---------------------------------------------------------------------
-- 2) PROCEDIMIENTO de respaldo (valida el archivo y registra en bitácora)
-- ---------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.sp_Respaldar
    @Tipo    VARCHAR(20),
    @Carpeta NVARCHAR(260) = N'C:\Respaldos\MedicalRemember\'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @archivo NVARCHAR(400);
    DECLARE @sello   VARCHAR(20) = FORMAT(GETDATE(), 'yyyyMMdd_HHmmss');

    BEGIN TRY
        IF @Tipo = 'FULL'
        BEGIN
            SET @archivo = @Carpeta + N'MRDB_FULL_' + @sello + N'.bak';
            BACKUP DATABASE MedicalRememberDB TO DISK = @archivo
                WITH COMPRESSION, CHECKSUM, INIT, NAME = N'MedicalRememberDB - Completo';
        END
        ELSE IF @Tipo = 'DIFERENCIAL'
        BEGIN
            SET @archivo = @Carpeta + N'MRDB_DIFF_' + @sello + N'.bak';
            BACKUP DATABASE MedicalRememberDB TO DISK = @archivo
                WITH DIFFERENTIAL, COMPRESSION, CHECKSUM, INIT, NAME = N'MedicalRememberDB - Diferencial';
        END
        ELSE IF @Tipo = 'LOG'
        BEGIN
            SET @archivo = @Carpeta + N'MRDB_LOG_' + @sello + N'.trn';
            BACKUP LOG MedicalRememberDB TO DISK = @archivo
                WITH COMPRESSION, CHECKSUM, INIT, NAME = N'MedicalRememberDB - Log';
        END
        ELSE
            THROW 50001, 'Tipo de respaldo invalido. Use FULL, DIFERENCIAL o LOG.', 1;

        -- Verifica que el archivo generado sea legible y consistente
        RESTORE VERIFYONLY FROM DISK = @archivo WITH CHECKSUM;

        INSERT INTO dbo.BitacoraRespaldos (Tipo, Archivo, Resultado, Detalle)
        VALUES (@Tipo, @archivo, 'OK', N'Respaldo creado y verificado con RESTORE VERIFYONLY.');
    END TRY
    BEGIN CATCH
        INSERT INTO dbo.BitacoraRespaldos (Tipo, Archivo, Resultado, Detalle)
        VALUES (@Tipo, @archivo, 'ERROR', LEFT(ERROR_MESSAGE(), 1000));
        THROW;
    END CATCH
END
GO

-- ---------------------------------------------------------------------
-- 3) JOBS (SQL Server Agent)
--    MR_Respaldo_Completo_Diario  : todos los días 02:00
--    MR_Respaldo_Diferencial_6h   : cada 6 horas
--    MR_Respaldo_Log_1h           : cada hora (transaccional)
-- ---------------------------------------------------------------------
USE msdb;
GO

DECLARE @jobs TABLE (nombre SYSNAME, tipo VARCHAR(20), horario SYSNAME,
                     freq_subday_type INT, freq_subday_interval INT, inicio INT);
INSERT INTO @jobs VALUES
 (N'MR_Respaldo_Completo_Diario', 'FULL',        N'Diario 02:00',  1, 0, 020000),
 (N'MR_Respaldo_Diferencial_6h',  'DIFERENCIAL', N'Cada 6 horas',  8, 6, 080000),
 (N'MR_Respaldo_Log_1h',          'LOG',         N'Cada hora',     8, 1, 003000);

DECLARE @n SYSNAME, @t VARCHAR(20), @h SYSNAME, @st INT, @si INT, @ini INT, @cmd NVARCHAR(400);
DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT nombre, tipo, horario, freq_subday_type, freq_subday_interval, inicio FROM @jobs;
OPEN c;
FETCH NEXT FROM c INTO @n, @t, @h, @st, @si, @ini;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = @n)
        EXEC msdb.dbo.sp_delete_job @job_name = @n;

    SET @cmd = N'EXEC dbo.sp_Respaldar @Tipo = ''' + @t + N''';';

    EXEC msdb.dbo.sp_add_job @job_name = @n,
         @description = N'Medical Remember - respaldo automático de MedicalRememberDB';
    EXEC msdb.dbo.sp_add_jobstep @job_name = @n, @step_name = N'Respaldo ' + @t,
         @subsystem = N'TSQL', @database_name = N'MedicalRememberDB', @command = @cmd,
         @retry_attempts = 1, @retry_interval = 5;
    EXEC msdb.dbo.sp_add_jobschedule @job_name = @n, @name = @h,
         @freq_type = 4, @freq_interval = 1,                 -- diario
         @freq_subday_type = @st, @freq_subday_interval = @si,  -- 1 = una vez | 8 = cada N horas
         @active_start_time = @ini;
    EXEC msdb.dbo.sp_add_jobserver @job_name = @n;

    FETCH NEXT FROM c INTO @n, @t, @h, @st, @si, @ini;
END
CLOSE c; DEALLOCATE c;
GO

-- ---------------------------------------------------------------------
-- 4) VALIDACIÓN DE LOS JOBS (tomar capturas para la defensa)
-- ---------------------------------------------------------------------
-- 4.1 Ejecutar a mano el orden correcto: primero FULL, luego DIFERENCIAL y LOG
EXEC msdb.dbo.sp_start_job @job_name = N'MR_Respaldo_Completo_Diario';
WAITFOR DELAY '00:00:10';
EXEC msdb.dbo.sp_start_job @job_name = N'MR_Respaldo_Diferencial_6h';
EXEC msdb.dbo.sp_start_job @job_name = N'MR_Respaldo_Log_1h';
WAITFOR DELAY '00:00:10';

-- 4.2 Historial de ejecución de los jobs (run_status 1 = exitoso)
SELECT j.name AS Job,
       msdb.dbo.agent_datetime(h.run_date, h.run_time) AS Ejecutado,
       CASE h.run_status WHEN 1 THEN 'Exitoso' WHEN 0 THEN 'FALLIDO' ELSE 'Otro' END AS Estado
FROM msdb.dbo.sysjobhistory h
JOIN msdb.dbo.sysjobs j ON j.job_id = h.job_id
WHERE h.step_id = 0 AND j.name LIKE N'MR[_]%'
ORDER BY Ejecutado DESC;

-- 4.3 Bitácora propia de respaldos
SELECT TOP 20 * FROM MedicalRememberDB.dbo.BitacoraRespaldos ORDER BY BitacoraID DESC;

-- 4.4 Historial oficial de SQL Server (confirma tipo, tamaño y compresión)
SELECT TOP 10 database_name, type AS Tipo, backup_finish_date,
       CAST(backup_size/1048576.0 AS DECIMAL(10,2)) AS MB,
       CAST(compressed_backup_size/1048576.0 AS DECIMAL(10,2)) AS MB_Comprimido
FROM msdb.dbo.backupset WHERE database_name = N'MedicalRememberDB'
ORDER BY backup_finish_date DESC;   -- D = completo, I = diferencial, L = log
GO

-- ---------------------------------------------------------------------
-- 5) PRUEBA DE RESTAURACIÓN (en una base de PRUEBA, nunca sobre producción)
--    Edita @full y @diff con los nombres reales que muestra la bitácora.
-- ---------------------------------------------------------------------
USE master;
GO
DECLARE @full NVARCHAR(400) = N'C:\Respaldos\MedicalRemember\MRDB_FULL_AAAAMMDD_HHMMSS.bak';
DECLARE @diff NVARCHAR(400) = N'C:\Respaldos\MedicalRemember\MRDB_DIFF_AAAAMMDD_HHMMSS.bak';
DECLARE @dir  NVARCHAR(260) = CONVERT(NVARCHAR(260), SERVERPROPERTY('InstanceDefaultDataPath'));
DECLARE @mdf  NVARCHAR(400) = @dir + N'MedicalRememberDB_Prueba.mdf';
DECLARE @ldf  NVARCHAR(400) = @dir + N'MedicalRememberDB_Prueba_log.ldf';

-- Nombres lógicos de archivos (si tu base usa otros, mira el resultado de esta consulta)
-- RESTORE FILELISTONLY FROM DISK = @full;

RESTORE DATABASE MedicalRememberDB_Prueba FROM DISK = @full
    WITH MOVE N'MedicalRememberDB'     TO @mdf,
         MOVE N'MedicalRememberDB_log' TO @ldf,
         NORECOVERY, REPLACE;                       -- 1) completo

RESTORE DATABASE MedicalRememberDB_Prueba FROM DISK = @diff
    WITH RECOVERY;                                  -- 2) diferencial y base en línea
GO

-- Validación de la restauración
USE MedicalRememberDB_Prueba;
SELECT 'Pacientes' AS Tabla, COUNT(*) AS Filas FROM Pacientes
UNION ALL SELECT 'Medicamentos',           COUNT(*) FROM Medicamentos
UNION ALL SELECT 'Tratamientos',           COUNT(*) FROM Tratamientos
UNION ALL SELECT 'Horarios',               COUNT(*) FROM Horarios
UNION ALL SELECT 'HistorialCumplimiento',  COUNT(*) FROM HistorialCumplimiento
UNION ALL SELECT 'Usuarios',               COUNT(*) FROM Usuarios;

SELECT name AS Trigger_Presente FROM sys.triggers;   -- deben existir los 4 triggers
DBCC CHECKDB (MedicalRememberDB_Prueba) WITH NO_INFOMSGS;   -- sin errores = integridad OK
GO
-- Limpieza opcional:  DROP DATABASE MedicalRememberDB_Prueba;

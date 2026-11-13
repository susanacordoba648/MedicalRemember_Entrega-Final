-- =====================================================================
-- historial de los ultimos 7 dias (para que los reportes y graficos tengan datos)
-- =====================================================================
USE MedicalRememberDB;
SET NOCOUNT ON;

;WITH Dias AS (SELECT n FROM (VALUES (1),(2),(3),(4),(5),(6),(7)) AS d(n)),
Base AS (
    SELECT h.HorarioID,
           DATEADD(MINUTE, DATEDIFF(MINUTE, CAST('00:00' AS TIME), h.HoraToma),
                   CAST(DATEADD(DAY, -d.n, CAST(GETDATE() AS DATE)) AS DATETIME)) AS Programada,
           (h.HorarioID + d.n) % 10 AS Patron
    FROM Horarios h
    INNER JOIN Tratamientos t ON t.TratamientoID = h.TratamientoID
    CROSS JOIN Dias d
    WHERE h.Activo = 1 AND t.Activo = 1
)
INSERT INTO HistorialCumplimiento (HorarioID, FechaHoraProgramada, FechaHoraTomado, Estado, Observacion)
SELECT b.HorarioID, b.Programada,
       CASE WHEN b.Patron = 9 THEN NULL
            WHEN b.Patron = 7 THEN DATEADD(MINUTE, 45, b.Programada)
            ELSE DATEADD(MINUTE, b.Patron * 2, b.Programada) END,
       CASE WHEN b.Patron = 9 THEN 2 ELSE 1 END,
       CASE WHEN b.Patron = 9 THEN N'Dato de prueba: toma omitida' ELSE N'Dato de prueba' END
FROM Base b
WHERE NOT EXISTS (SELECT 1 FROM HistorialCumplimiento hc
                  WHERE hc.HorarioID = b.HorarioID AND hc.FechaHoraProgramada = b.Programada);

SELECT CASE Estado WHEN 1 THEN 'Tomado' WHEN 2 THEN 'Omitido' WHEN 3 THEN 'Tardio' ELSE 'Pendiente' END AS Estado,
       COUNT(*) AS Cantidad
FROM HistorialCumplimiento GROUP BY Estado;

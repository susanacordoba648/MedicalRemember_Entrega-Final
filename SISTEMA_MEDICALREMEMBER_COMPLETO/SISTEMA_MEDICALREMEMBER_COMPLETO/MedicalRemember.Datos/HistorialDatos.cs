using System;
using System.Data;
using System.Data.SqlClient;

namespace MedicalRemember.Data
{
    /// <summary>Capa de DATOS: acceso a la tabla HistorialCumplimiento.</summary>
    public class HistorialDatos
    {
        public DataTable ListarPorRango(DateTime desde, DateTime hasta)
        {
            const string sql = @"
                SELECT hc.HistorialID,
                       p.Nombre+' '+p.Apellido AS Paciente,
                       m.Nombre AS Medicamento,
                       t.Dosis,
                       CONVERT(VARCHAR(16),hc.FechaHoraProgramada,120) AS Programado,
                       ISNULL(CONVERT(VARCHAR(16),hc.FechaHoraTomado,120),'--') AS Tomado,
                       CASE hc.Estado
                           WHEN 1 THEN '✅ Tomado'
                           WHEN 2 THEN '❌ Omitido'
                           WHEN 3 THEN '⏰ Tardio'
                           ELSE       '🕐 Pendiente' END AS Estado
                FROM HistorialCumplimiento hc
                INNER JOIN Horarios     h ON hc.HorarioID      = h.HorarioID
                INNER JOIN Tratamientos t ON h.TratamientoID   = t.TratamientoID
                INNER JOIN Pacientes    p ON t.PacienteID      = p.PacienteID
                INNER JOIN Medicamentos m ON t.MedicamentoID   = m.MedicamentoID
                WHERE hc.FechaHoraProgramada BETWEEN @desde AND @hasta
                ORDER BY hc.FechaHoraProgramada DESC";

            return Conexion.Consulta(sql,
                new SqlParameter("@desde", desde),
                new SqlParameter("@hasta", hasta));
        }
    }
}

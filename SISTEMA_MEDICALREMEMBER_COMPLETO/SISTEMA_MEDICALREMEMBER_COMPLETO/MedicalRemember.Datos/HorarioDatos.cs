using System;
using System.Data;
using System.Data.SqlClient;
using MedicalRemember.Models;

namespace MedicalRemember.Data
{
    /// <summary>Capa de DATOS: acceso a la tabla Horarios.</summary>
    public class HorarioDatos
    {
        public DataTable ListarConDatos()
        {
            const string sql = @"
                SELECT h.HorarioID,
                       p.Nombre+' '+p.Apellido AS Paciente,
                       m.Nombre AS Medicamento,
                       t.Dosis,
                       CONVERT(VARCHAR(5),h.HoraToma,108) AS Hora,
                       CASE h.DiaSemana
                           WHEN 1 THEN 'Lunes'   WHEN 2 THEN 'Martes'
                           WHEN 3 THEN 'Miercoles' WHEN 4 THEN 'Jueves'
                           WHEN 5 THEN 'Viernes' WHEN 6 THEN 'Sabado'
                           WHEN 7 THEN 'Domingo'
                           ELSE 'Todos los dias' END AS Dia
                FROM Horarios h
                INNER JOIN Tratamientos t  ON h.TratamientoID=t.TratamientoID
                INNER JOIN Pacientes    p  ON t.PacienteID   =p.PacienteID
                INNER JOIN Medicamentos m  ON t.MedicamentoID=m.MedicamentoID
                WHERE h.Activo=1 ORDER BY h.HoraToma";
            return Conexion.Consulta(sql);
        }

        public DataTable ListarTratamientosParaCombo()
        {
            const string sql = @"
                SELECT t.TratamientoID,
                       p.Nombre+' '+p.Apellido+' — '+m.Nombre AS Descripcion
                FROM Tratamientos t
                INNER JOIN Pacientes    p ON t.PacienteID   = p.PacienteID
                INNER JOIN Medicamentos m ON t.MedicamentoID= m.MedicamentoID
                WHERE t.Activo=1 ORDER BY p.Nombre";
            return Conexion.Consulta(sql);
        }

        /// <summary>Medicamentos programados para hoy (dashboard), con su estado del día.</summary>
        public DataTable ListarDeHoy()
        {
            const string sql = @"
                SELECT
                    h.HorarioID,
                    p.Nombre + ' ' + p.Apellido          AS Paciente,
                    m.Nombre                             AS Medicamento,
                    t.Dosis,
                    CONVERT(VARCHAR(5), h.HoraToma, 108) AS Hora,
                    ISNULL(t.Instrucciones, '--')         AS Instrucciones,
                    CASE ISNULL(hc.Estado, 4)
                        WHEN 1 THEN '✅ Tomado'
                        WHEN 2 THEN '❌ Omitido'
                        WHEN 3 THEN '⏰ Tardio'
                        ELSE        '🕐 Pendiente'
                    END AS Estado
                FROM Horarios h
                INNER JOIN Tratamientos t  ON h.TratamientoID = t.TratamientoID
                INNER JOIN Pacientes    p  ON t.PacienteID    = p.PacienteID
                INNER JOIN Medicamentos m  ON t.MedicamentoID = m.MedicamentoID
                LEFT  JOIN HistorialCumplimiento hc
                       ON  hc.HorarioID = h.HorarioID
                       AND CAST(hc.FechaHoraProgramada AS DATE) = CAST(GETDATE() AS DATE)
                WHERE t.Activo = 1 AND h.Activo = 1 AND p.Activo = 1
                ORDER BY h.HoraToma";
            return Conexion.Consulta(sql);
        }

        /// <summary>Tomas próximas (+/- 5 min) aún no registradas, usado por el temporizador de alertas.</summary>
        public DataTable ListarProximasSinRegistrar()
        {
            const string sql = @"
                SELECT p.Nombre + ' ' + p.Apellido AS Paciente,
                       m.Nombre AS Medicamento, t.Dosis,
                       CONVERT(VARCHAR(5), h.HoraToma, 108) AS Hora,
                       h.HorarioID
                FROM Horarios h
                INNER JOIN Tratamientos t  ON h.TratamientoID = t.TratamientoID
                INNER JOIN Pacientes    p  ON t.PacienteID    = p.PacienteID
                INNER JOIN Medicamentos m  ON t.MedicamentoID = m.MedicamentoID
                LEFT  JOIN HistorialCumplimiento hc
                       ON  hc.HorarioID = h.HorarioID
                       AND CAST(hc.FechaHoraProgramada AS DATE) = CAST(GETDATE() AS DATE)
                       AND hc.Estado = 1
                WHERE t.Activo = 1 AND h.Activo = 1 AND hc.HistorialID IS NULL
                  AND ABS(DATEDIFF(MINUTE,
                        CAST(CONVERT(VARCHAR(8), GETDATE(), 108) AS TIME), h.HoraToma)) <= 5";
            return Conexion.Consulta(sql);
        }

        public int Insertar(Horario h)
        {
            const string sql = "INSERT INTO Horarios (TratamientoID, HoraToma, DiaSemana) VALUES (@trat, @hora, @dia)";
            return Conexion.EjecutarYObtenerId(sql,
                new SqlParameter("@trat", h.TratamientoID),
                new SqlParameter("@hora", h.HoraToma),
                new SqlParameter("@dia",  (object)h.DiaSemana ?? DBNull.Value));
        }

        public void Eliminar(int id)
        {
            Conexion.Ejecutar("UPDATE Horarios SET Activo=0 WHERE HorarioID=@id",
                new SqlParameter("@id", id));
        }

        /// <summary>Registra (o actualiza) la toma de hoy como "Tomado" para un horario dado.</summary>
        public void MarcarTomadoHoy(int horarioId)
        {
            const string sql = @"
                MERGE HistorialCumplimiento AS t
                USING (SELECT @hid AS HorarioID, CAST(GETDATE() AS DATE) AS Hoy) AS s
                ON t.HorarioID = s.HorarioID
                AND CAST(t.FechaHoraProgramada AS DATE) = s.Hoy
                WHEN MATCHED THEN
                    UPDATE SET Estado = 1, FechaHoraTomado = GETDATE()
                WHEN NOT MATCHED THEN
                    INSERT (HorarioID, FechaHoraProgramada, FechaHoraTomado, Estado)
                    VALUES (@hid, GETDATE(), GETDATE(), 1);";
            Conexion.Ejecutar(sql, new SqlParameter("@hid", horarioId));
        }
    }
}

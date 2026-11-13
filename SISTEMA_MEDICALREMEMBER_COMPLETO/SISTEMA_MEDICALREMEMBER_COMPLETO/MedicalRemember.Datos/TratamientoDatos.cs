using System;
using System.Data;
using System.Data.SqlClient;
using MedicalRemember.Models;

namespace MedicalRemember.Data
{
    /// <summary>Capa de DATOS: acceso a la tabla Tratamientos.</summary>
    public class TratamientoDatos
    {
        public DataTable Listar()
        {
            const string sql = @"
                SELECT t.TratamientoID,
                       p.Nombre+' '+p.Apellido AS Paciente,
                       m.Nombre AS Medicamento,
                       t.Dosis,
                       CONVERT(VARCHAR,t.FechaInicio,103) AS Inicio,
                       ISNULL(CONVERT(VARCHAR,t.FechaFin,103),'Indefinido') AS Fin,
                       ISNULL(t.Instrucciones,'--') AS Instrucciones
                FROM Tratamientos t
                INNER JOIN Pacientes    p ON t.PacienteID   = p.PacienteID
                INNER JOIN Medicamentos m ON t.MedicamentoID= m.MedicamentoID
                WHERE t.Activo=1 ORDER BY t.FechaRegistro DESC";
            return Conexion.Consulta(sql);
        }

        public DataTable ListarParaCombo()
        {
            const string sql = @"
                SELECT PacienteID, Nombre+' '+Apellido AS Nombre
                FROM Pacientes WHERE Activo=1 ORDER BY Nombre";
            return Conexion.Consulta(sql);
        }

        public int Insertar(Tratamiento t)
        {
            const string sql = @"INSERT INTO Tratamientos
                    (PacienteID,MedicamentoID,Dosis,FechaInicio,FechaFin,Instrucciones)
                VALUES (@pac,@med,@dos,@ini,@fin,@ins)";
            return Conexion.EjecutarYObtenerId(sql,
                new SqlParameter("@pac", t.PacienteID),
                new SqlParameter("@med", t.MedicamentoID),
                new SqlParameter("@dos", t.Dosis),
                new SqlParameter("@ini", t.FechaInicio),
                new SqlParameter("@fin", (object)t.FechaFin ?? DBNull.Value),
                new SqlParameter("@ins", (object)t.Instrucciones ?? DBNull.Value));
        }

        public void Eliminar(int id)
        {
            Conexion.Ejecutar("UPDATE Tratamientos SET Activo=0 WHERE TratamientoID=@id",
                new SqlParameter("@id", id));
        }
    }
}

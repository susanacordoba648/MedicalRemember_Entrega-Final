using System;
using System.Data;
using System.Data.SqlClient;
using MedicalRemember.Models;

namespace MedicalRemember.Data
{
    /// <summary>
    /// Capa de DATOS: contiene únicamente sentencias SQL para la tabla Pacientes.
    /// No valida reglas de negocio; eso corresponde a la capa de Negocio.
    /// </summary>
    public class PacienteDatos
    {
        /// <summary>Lista de pacientes activos (para grillas), con filtro opcional por nombre/apellido.</summary>
        public DataTable Listar(string filtro = "")
        {
            string sql = @"
                SELECT PacienteID,
                       Nombre + ' ' + Apellido AS NombreCompleto,
                       DATEDIFF(YEAR, FechaNacimiento, GETDATE()) AS Edad,
                       ISNULL(Telefono, '--') AS Telefono,
                       CASE TipoPaciente
                           WHEN 1 THEN 'Adulto Mayor'
                           WHEN 2 THEN 'Paciente Cronico'
                           ELSE 'Otro' END AS Tipo
                FROM Pacientes WHERE Activo = 1";

            SqlParameter[] p = null;
            if (!string.IsNullOrWhiteSpace(filtro))
            {
                sql += " AND (Nombre LIKE @f OR Apellido LIKE @f)";
                p = new[] { new SqlParameter("@f", "%" + filtro + "%") };
            }
            sql += " ORDER BY Nombre";

            return Conexion.Consulta(sql, p);
        }

        public Paciente ObtenerPorId(int id)
        {
            DataTable dt = Conexion.Consulta("SELECT * FROM Pacientes WHERE PacienteID=@id",
                new SqlParameter("@id", id));
            if (dt.Rows.Count == 0) return null;
            return Mapear(dt.Rows[0]);
        }

        public int Insertar(Paciente p)
        {
            const string sql = @"INSERT INTO Pacientes
                        (Nombre, Apellido, FechaNacimiento, Telefono, Correo, Direccion, TipoPaciente)
                    VALUES (@nom, @ape, @fnac, @tel, @cor, @dir, @tipo)";
            return Conexion.EjecutarYObtenerId(sql, Parametros(p, incluirId: false));
        }

        public int Actualizar(Paciente p)
        {
            const string sql = @"UPDATE Pacientes SET
                        Nombre=@nom, Apellido=@ape, FechaNacimiento=@fnac,
                        Telefono=@tel, Correo=@cor, Direccion=@dir, TipoPaciente=@tipo
                    WHERE PacienteID=@id";
            return Conexion.Ejecutar(sql, Parametros(p, incluirId: true));
        }

        /// <summary>Baja lógica (no se borra el registro, solo se marca Activo=0).</summary>
        public void Eliminar(int id)
        {
            Conexion.Ejecutar("UPDATE Pacientes SET Activo=0 WHERE PacienteID=@id",
                new SqlParameter("@id", id));
        }

        private SqlParameter[] Parametros(Paciente p, bool incluirId)
        {
            var lista = new System.Collections.Generic.List<SqlParameter>
            {
                new SqlParameter("@nom",  p.Nombre),
                new SqlParameter("@ape",  p.Apellido),
                new SqlParameter("@fnac", p.FechaNacimiento),
                new SqlParameter("@tel",  (object)p.Telefono ?? DBNull.Value),
                new SqlParameter("@cor",  (object)p.Correo ?? DBNull.Value),
                new SqlParameter("@dir",  (object)p.Direccion ?? DBNull.Value),
                new SqlParameter("@tipo", p.TipoPaciente)
            };
            if (incluirId) lista.Add(new SqlParameter("@id", p.PacienteID));
            return lista.ToArray();
        }

        private Paciente Mapear(DataRow r) => new Paciente
        {
            PacienteID      = Convert.ToInt32(r["PacienteID"]),
            Nombre          = r["Nombre"].ToString(),
            Apellido        = r["Apellido"].ToString(),
            FechaNacimiento = Convert.ToDateTime(r["FechaNacimiento"]),
            Telefono        = r["Telefono"]?.ToString(),
            Correo          = r["Correo"]?.ToString(),
            Direccion       = r["Direccion"]?.ToString(),
            TipoPaciente    = Convert.ToInt32(r["TipoPaciente"]),
            Activo          = Convert.ToBoolean(r["Activo"])
        };
    }
}

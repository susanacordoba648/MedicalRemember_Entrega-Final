using System;
using System.Data;
using System.Data.SqlClient;
using MedicalRemember.Models;

namespace MedicalRemember.Data
{
    /// <summary>Capa de DATOS: acceso a la tabla Medicamentos.</summary>
    public class MedicamentoDatos
    {
        public DataTable Listar()
        {
            return Conexion.Consulta(
                @"SELECT MedicamentoID, Nombre, Descripcion, Unidad, ViaAdministracion AS Via
                  FROM Medicamentos WHERE Activo=1 ORDER BY Nombre");
        }

        public Medicamento ObtenerPorId(int id)
        {
            DataTable dt = Conexion.Consulta("SELECT * FROM Medicamentos WHERE MedicamentoID=@id",
                new SqlParameter("@id", id));
            if (dt.Rows.Count == 0) return null;
            return Mapear(dt.Rows[0]);
        }

        public int Insertar(Medicamento m)
        {
            const string sql = @"INSERT INTO Medicamentos (Nombre, Descripcion, Unidad, ViaAdministracion)
                                  VALUES (@n,@d,@u,@via)";
            return Conexion.EjecutarYObtenerId(sql,
                new SqlParameter("@n", m.Nombre),
                new SqlParameter("@d", (object)m.Descripcion ?? DBNull.Value),
                new SqlParameter("@u", m.Unidad),
                new SqlParameter("@via", m.ViaAdministracion));
        }

        public int Actualizar(Medicamento m)
        {
            const string sql = @"UPDATE Medicamentos SET Nombre=@n, Descripcion=@d, Unidad=@u, ViaAdministracion=@via
                                  WHERE MedicamentoID=@id";
            return Conexion.Ejecutar(sql,
                new SqlParameter("@n", m.Nombre),
                new SqlParameter("@d", (object)m.Descripcion ?? DBNull.Value),
                new SqlParameter("@u", m.Unidad),
                new SqlParameter("@via", m.ViaAdministracion),
                new SqlParameter("@id", m.MedicamentoID));
        }

        public void Eliminar(int id)
        {
            Conexion.Ejecutar("UPDATE Medicamentos SET Activo=0 WHERE MedicamentoID=@id",
                new SqlParameter("@id", id));
        }

        private Medicamento Mapear(DataRow r) => new Medicamento
        {
            MedicamentoID     = Convert.ToInt32(r["MedicamentoID"]),
            Nombre            = r["Nombre"].ToString(),
            Descripcion       = r["Descripcion"]?.ToString(),
            Unidad            = r["Unidad"].ToString(),
            ViaAdministracion = r["ViaAdministracion"]?.ToString() ?? "Oral",
            Activo            = Convert.ToBoolean(r["Activo"])
        };
    }
}

using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace MedicalRemember.Data
{
    /// <summary>
    /// Capa de DATOS (acceso genérico). Es la única clase del proyecto que abre
    /// conexiones ADO.NET directamente. Las clases de datos de cada entidad (PacienteDatos, etc.) se apoyan
    /// en esta clase para no repetir código de conexión.
    /// </summary>
    public static class Conexion
    {
        // La cadena de conexión se lee de App.config (Data\ConnectionString en
        // App.config). Si no existe la clave, se usa un valor por defecto.
        private static readonly string CONN = ObtenerCadena();

        private static string ObtenerCadena()
        {
            var cs = ConfigurationManager.ConnectionStrings["MedicalRememberDB"];
            return cs != null
                ? cs.ConnectionString
                : "Server=.;Database=MedicalRememberDB;Integrated Security=True;";
        }

        /// <summary>Verifica que se pueda abrir conexión con la base de datos.</summary>
        public static bool ProbarConexion()
        {
            try
            {
                using (var c = new SqlConnection(CONN))
                {
                    c.Open();
                    return c.State == ConnectionState.Open;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Ejecuta un SELECT y devuelve el resultado como DataTable.</summary>
        public static DataTable Consulta(string sql, params SqlParameter[] parametros)
        {
            var dt = new DataTable();
            using (var con = new SqlConnection(CONN))
            using (var cmd = new SqlCommand(sql, con))
            {
                if (parametros != null) cmd.Parameters.AddRange(parametros);
                con.Open();
                new SqlDataAdapter(cmd).Fill(dt);
            }
            return dt;
        }

        /// <summary>Ejecuta INSERT / UPDATE / DELETE. Devuelve las filas afectadas.</summary>
        public static int Ejecutar(string sql, params SqlParameter[] parametros)
        {
            using (var con = new SqlConnection(CONN))
            using (var cmd = new SqlCommand(sql, con))
            {
                if (parametros != null) cmd.Parameters.AddRange(parametros);
                con.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Ejecuta un INSERT y devuelve el ID autogenerado (SCOPE_IDENTITY).</summary>
        public static int EjecutarYObtenerId(string sql, params SqlParameter[] parametros)
        {
            using (var con = new SqlConnection(CONN))
            using (var cmd = new SqlCommand(sql + "; SELECT CAST(SCOPE_IDENTITY() AS INT);", con))
            {
                if (parametros != null) cmd.Parameters.AddRange(parametros);
                con.Open();
                object resultado = cmd.ExecuteScalar();
                return (resultado == null || resultado == DBNull.Value) ? 0 : Convert.ToInt32(resultado);
            }
        }

        /// <summary>Ejecuta una consulta que devuelve un único valor (COUNT, MAX, etc).</summary>
        public static object Escalar(string sql, params SqlParameter[] parametros)
        {
            using (var con = new SqlConnection(CONN))
            using (var cmd = new SqlCommand(sql, con))
            {
                if (parametros != null) cmd.Parameters.AddRange(parametros);
                con.Open();
                return cmd.ExecuteScalar();
            }
        }
    }
}

using System;
using System.Data;
using System.Data.SqlClient;
using MedicalRemember.Models;

namespace MedicalRemember.Data
{
    /// <summary>Capa de DATOS: acceso a la tabla Usuarios (cuentas de acceso).</summary>
    public class UsuarioDatos
    {
        public Usuario ObtenerPorCredenciales(string nombreUsuario, string contrasenaHash)
        {
            DataTable dt = Conexion.Consulta(
                @"SELECT UsuarioID, NombreUsuario, NombreCompleto, Rol
                  FROM Usuarios
                  WHERE NombreUsuario=@u AND Contrasena=@p AND Activo=1",
                new SqlParameter("@u", nombreUsuario),
                new SqlParameter("@p", contrasenaHash));

            if (dt.Rows.Count == 0) return null;
            var r = dt.Rows[0];
            return new Usuario
            {
                UsuarioID      = Convert.ToInt32(r["UsuarioID"]),
                NombreUsuario  = r["NombreUsuario"].ToString(),
                NombreCompleto = r["NombreCompleto"].ToString(),
                Rol            = r["Rol"].ToString()
            };
        }

        public bool ExisteUsuario(string nombreUsuario)
        {
            object total = Conexion.Escalar(
                "SELECT COUNT(*) FROM Usuarios WHERE NombreUsuario = @u",
                new SqlParameter("@u", nombreUsuario));
            return Convert.ToInt32(total) > 0;
        }

        public int Insertar(Usuario u)
        {
            const string sql = @"INSERT INTO Usuarios
                    (NombreUsuario, Contrasena, NombreCompleto, Rol, RespuestaMejorAmigo, RespuestaColorFav)
                VALUES (@u, @p, @nom, @rol, @amigo, @color)";
            return Conexion.EjecutarYObtenerId(sql,
                new SqlParameter("@u",     u.NombreUsuario),
                new SqlParameter("@p",     u.Contrasena),
                new SqlParameter("@nom",   u.NombreCompleto),
                new SqlParameter("@rol",   u.Rol),
                new SqlParameter("@amigo", u.RespuestaMejorAmigo),
                new SqlParameter("@color", u.RespuestaColorFav));
        }

        /// <summary>Busca un usuario activo por nombre, para el flujo de recuperación de contraseña.</summary>
        public Usuario BuscarPorNombre(string nombreUsuario)
        {
            DataTable dt = Conexion.Consulta(
                @"SELECT UsuarioID, NombreUsuario, NombreCompleto, RespuestaMejorAmigo, RespuestaColorFav
                  FROM Usuarios WHERE NombreUsuario=@u AND Activo=1",
                new SqlParameter("@u", nombreUsuario));

            if (dt.Rows.Count == 0) return null;
            var r = dt.Rows[0];
            return new Usuario
            {
                UsuarioID           = Convert.ToInt32(r["UsuarioID"]),
                NombreUsuario       = r["NombreUsuario"].ToString(),
                NombreCompleto      = r["NombreCompleto"].ToString(),
                RespuestaMejorAmigo = r["RespuestaMejorAmigo"]?.ToString(),
                RespuestaColorFav   = r["RespuestaColorFav"]?.ToString()
            };
        }

        public void ActualizarContrasena(int usuarioId, string contrasenaHash)
        {
            Conexion.Ejecutar("UPDATE Usuarios SET Contrasena=@p WHERE UsuarioID=@id",
                new SqlParameter("@p", contrasenaHash),
                new SqlParameter("@id", usuarioId));
        }
    }
}

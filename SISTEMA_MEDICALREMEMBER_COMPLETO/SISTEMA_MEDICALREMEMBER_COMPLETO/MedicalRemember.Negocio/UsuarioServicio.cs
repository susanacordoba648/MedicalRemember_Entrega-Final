using System;
using MedicalRemember.Data;
using MedicalRemember.Models;

namespace MedicalRemember.Business
{
    /// <summary>
    /// Capa de NEGOCIO: login, registro y recuperación de contraseña.
    /// Todas las contraseñas y respuestas de seguridad se manejan como hash SHA-256;
    /// nunca se guarda ni se compara texto plano.
    /// </summary>
    public class UsuarioServicio
    {
        private readonly UsuarioDatos _datos = new UsuarioDatos();

        /// <summary>Intenta iniciar sesión. Devuelve el usuario autenticado o null si falla.</summary>
        public Usuario IniciarSesion(string nombreUsuario, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(contrasena))
                return null;

            string hash = Seguridad.Hash(contrasena);
            return _datos.ObtenerPorCredenciales(nombreUsuario.Trim(), hash);
        }

        public ResultadoOperacion Registrar(Usuario u, string contrasena, string confirmarContrasena)
        {
            if (string.IsNullOrWhiteSpace(u.NombreCompleto))
                return ResultadoOperacion.Error("El nombre completo es obligatorio.");

            if (string.IsNullOrWhiteSpace(u.NombreUsuario))
                return ResultadoOperacion.Error("El nombre de usuario es obligatorio.");

            if (u.NombreUsuario.Contains(" "))
                return ResultadoOperacion.Error("El usuario no puede tener espacios.");

            if (string.IsNullOrWhiteSpace(contrasena) || contrasena.Length < 4)
                return ResultadoOperacion.Error("La contraseña debe tener al menos 4 caracteres.");

            if (contrasena != confirmarContrasena)
                return ResultadoOperacion.Error("Las contraseñas no coinciden.");

            if (string.IsNullOrWhiteSpace(u.RespuestaMejorAmigo))
                return ResultadoOperacion.Error("Escribe el nombre de tu mejor amigo.");

            if (string.IsNullOrWhiteSpace(u.RespuestaColorFav))
                return ResultadoOperacion.Error("Escribe tu color favorito.");

            if (_datos.ExisteUsuario(u.NombreUsuario.Trim()))
                return ResultadoOperacion.Error("Ese nombre de usuario ya está en uso.");

            try
            {
                u.NombreUsuario       = u.NombreUsuario.Trim();
                u.Contrasena          = Seguridad.Hash(contrasena);
                u.RespuestaMejorAmigo = Seguridad.Hash(Seguridad.NormalizarRespuesta(u.RespuestaMejorAmigo));
                u.RespuestaColorFav   = Seguridad.Hash(Seguridad.NormalizarRespuesta(u.RespuestaColorFav));

                _datos.Insertar(u);
                return ResultadoOperacion.Ok("Cuenta creada exitosamente.");
            }
            catch (Exception ex)
            {
                return ResultadoOperacion.Error("No se pudo crear la cuenta: " + ex.Message);
            }
        }

        /// <summary>Paso 1 de recuperación: busca el usuario por nombre.</summary>
        public Usuario BuscarParaRecuperar(string nombreUsuario)
            => _datos.BuscarPorNombre((nombreUsuario ?? "").Trim());

        /// <summary>Paso 2 de recuperación: valida las respuestas de seguridad.</summary>
        public bool VerificarRespuestas(Usuario usuario, string respuestaAmigo, string respuestaColor)
        {
            if (usuario == null) return false;

            string hashAmigo = Seguridad.Hash(Seguridad.NormalizarRespuesta(respuestaAmigo));
            string hashColor = Seguridad.Hash(Seguridad.NormalizarRespuesta(respuestaColor));

            return hashAmigo == usuario.RespuestaMejorAmigo && hashColor == usuario.RespuestaColorFav;
        }

        /// <summary>Paso 3 de recuperación: guarda la nueva contraseña.</summary>
        public ResultadoOperacion CambiarContrasena(int usuarioId, string nuevaContrasena, string confirmar)
        {
            if (string.IsNullOrWhiteSpace(nuevaContrasena) || nuevaContrasena.Length < 4)
                return ResultadoOperacion.Error("La contraseña debe tener al menos 4 caracteres.");

            if (nuevaContrasena != confirmar)
                return ResultadoOperacion.Error("Las contraseñas no coinciden.");

            try
            {
                _datos.ActualizarContrasena(usuarioId, Seguridad.Hash(nuevaContrasena));
                return ResultadoOperacion.Ok("Contraseña actualizada correctamente.");
            }
            catch (Exception ex)
            {
                return ResultadoOperacion.Error("No se pudo cambiar la contraseña: " + ex.Message);
            }
        }
    }
}

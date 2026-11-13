using System;
using System.Security.Cryptography;
using System.Text;

namespace MedicalRemember.Business
{
    /// <summary>
    /// Utilidades de seguridad. Las contraseñas nunca se guardan ni se comparan
    /// en texto plano: se convierten a un hash SHA-256 antes de tocar la base de datos.
    /// </summary>
    public static class Seguridad
    {
        public static string Hash(string textoPlano)
        {
            if (textoPlano == null) textoPlano = string.Empty;

            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(textoPlano));
                var sb = new StringBuilder();
                foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        /// <summary>Normaliza una respuesta de seguridad (minúsculas, sin espacios extremos) antes de hashearla.</summary>
        public static string NormalizarRespuesta(string respuesta)
            => (respuesta ?? string.Empty).Trim().ToLowerInvariant();
    }
}

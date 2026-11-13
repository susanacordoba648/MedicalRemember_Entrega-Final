namespace MedicalRemember.Models
{
    /// <summary>Representa una cuenta de acceso al sistema (login). Capa de MODELO.</summary>
    public class Usuario
    {
        public int    UsuarioID           { get; set; }
        public string NombreUsuario       { get; set; }
        public string Contrasena          { get; set; } // hash, nunca texto plano
        public string NombreCompleto      { get; set; }
        public string Rol                 { get; set; } = "Cuidador";
        public string RespuestaMejorAmigo { get; set; }
        public string RespuestaColorFav   { get; set; }
        public bool   Activo              { get; set; } = true;
    }
}

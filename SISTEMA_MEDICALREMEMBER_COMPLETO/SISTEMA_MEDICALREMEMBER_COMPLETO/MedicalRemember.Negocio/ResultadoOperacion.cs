namespace MedicalRemember.Business
{
    /// <summary>
    /// Resultado estándar de una operación de negocio: indica si tuvo éxito y,
    /// si no, el mensaje que la pantalla debe mostrarle al usuario.
    /// Evita que las validaciones "hablen" en MessageBox desde la capa de Negocio.
    /// </summary>
    public class ResultadoOperacion
    {
        public bool   Exito   { get; }
        public string Mensaje { get; }

        private ResultadoOperacion(bool exito, string mensaje)
        {
            Exito   = exito;
            Mensaje = mensaje;
        }

        public static ResultadoOperacion Ok(string mensaje = "") => new ResultadoOperacion(true, mensaje);
        public static ResultadoOperacion Error(string mensaje)   => new ResultadoOperacion(false, mensaje);
    }
}

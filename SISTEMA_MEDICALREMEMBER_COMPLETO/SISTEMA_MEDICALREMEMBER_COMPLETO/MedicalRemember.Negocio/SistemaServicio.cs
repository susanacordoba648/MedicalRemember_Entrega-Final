using MedicalRemember.Data;

namespace MedicalRemember.Business
{
    /// <summary>
    /// Capa de NEGOCIO: operaciones generales del sistema que no pertenecen
    /// a una entidad concreta (por ejemplo, verificar la conexión al arrancar).
    /// Existe para que la capa de Presentación (UI) nunca necesite referenciar
    /// directamente la capa de Datos.
    /// </summary>
    public static class SistemaServicio
    {
        public static bool ProbarConexion() => Conexion.ProbarConexion();
    }
}

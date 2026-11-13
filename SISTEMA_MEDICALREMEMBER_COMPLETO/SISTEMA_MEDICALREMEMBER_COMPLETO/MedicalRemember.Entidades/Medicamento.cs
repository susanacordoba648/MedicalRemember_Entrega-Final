namespace MedicalRemember.Models
{
    /// <summary>Representa un medicamento del catálogo. Capa de MODELO.</summary>
    public class Medicamento
    {
        public int    MedicamentoID     { get; set; }
        public string Nombre            { get; set; }
        public string Descripcion       { get; set; }
        public string Unidad            { get; set; } = "mg";
        public string ViaAdministracion { get; set; } = "Oral"; // Oral, Inyectable, Sublingual, Tópica, Otra
        public bool   Activo            { get; set; } = true;

        public override string ToString() => Nombre;
    }
}

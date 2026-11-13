using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalRemember.Api.Models
{
    [Table("Medicamentos")]
    public class Medicamento
    {
        [Key] public int MedicamentoID { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(150)] public string Nombre { get; set; }

        [StringLength(500)] public string? Descripcion { get; set; }
        [StringLength(50)]  public string Unidad { get; set; } = "mg";
        [StringLength(20)]  public string ViaAdministracion { get; set; } = "Oral";
        public bool Activo { get; set; } = true;
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalRemember.Api.Models
{
    [Table("Tratamientos")]
    public class Tratamiento
    {
        [Key] public int TratamientoID { get; set; }
        public int PacienteID { get; set; }
        public int MedicamentoID { get; set; }

        [Required(ErrorMessage = "La dosis es obligatoria")]
        [StringLength(100)] public string Dosis { get; set; }

        [Column(TypeName = "date")] public DateTime FechaInicio { get; set; }
        [Column(TypeName = "date")] public DateTime? FechaFin { get; set; }
        [StringLength(500)] public string? Instrucciones { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; }
    }
}

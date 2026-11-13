using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalRemember.Api.Models
{
    [Table("Pacientes")]                                   // Nombre de la tabla en SQL Server
    public class Paciente
    {
        [Key] public int PacienteID { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)] public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(100)] public string Apellido { get; set; }

        [Column(TypeName = "date")] public DateTime FechaNacimiento { get; set; }
        [StringLength(20)]  public string? Telefono  { get; set; }
        [StringLength(150)] public string? Correo    { get; set; }
        [StringLength(200)] public string? Direccion { get; set; }
        public int TipoPaciente { get; set; } = 3;
        public DateTime FechaRegistro { get; set; }
        public bool Activo { get; set; } = true;           // true = activo, false = baja logica
    }
}

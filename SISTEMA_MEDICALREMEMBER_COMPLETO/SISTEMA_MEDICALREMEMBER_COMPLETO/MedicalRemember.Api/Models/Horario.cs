using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalRemember.Api.Models
{
    [Table("Horarios")]
    public class Horario
    {
        [Key] public int HorarioID { get; set; }
        public int TratamientoID { get; set; }
        public TimeSpan HoraToma { get; set; }              // columna TIME
        public int? DiaSemana { get; set; }                 // null = todos los dias
        public bool Activo { get; set; } = true;
    }
}

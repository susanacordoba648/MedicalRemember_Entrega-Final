using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalRemember.Api.Models
{
    [Table("HistorialCumplimiento")]
    public class HistorialCumplimiento
    {
        [Key] public int HistorialID { get; set; }
        public int HorarioID { get; set; }
        public DateTime FechaHoraProgramada { get; set; }
        public DateTime? FechaHoraTomado { get; set; }
        public int Estado { get; set; } = 4;                // 1 Tomado, 2 Omitido, 3 Tardio, 4 Pendiente
        [StringLength(300)] public string? Observacion { get; set; }
    }
}

using System;

namespace MedicalRemember.Models
{
    /// <summary>Estado de una toma programada.</summary>
    public enum EstadoToma
    {
        Tomado    = 1,
        Omitido   = 2,
        Tardio    = 3,
        Pendiente = 4
    }

    /// <summary>Representa un registro de cumplimiento (si el paciente tomó o no el medicamento). Capa de MODELO.</summary>
    public class HistorialCumplimiento
    {
        public int       HistorialID         { get; set; }
        public int       HorarioID           { get; set; }
        public DateTime  FechaHoraProgramada { get; set; }
        public DateTime? FechaHoraTomado     { get; set; }
        public EstadoToma Estado             { get; set; } = EstadoToma.Pendiente;
        public string     Observacion        { get; set; }
    }
}

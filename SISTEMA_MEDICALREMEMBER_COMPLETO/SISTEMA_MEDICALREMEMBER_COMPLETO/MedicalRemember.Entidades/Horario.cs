using System;

namespace MedicalRemember.Models
{
    /// <summary>Representa la hora programada de toma de un tratamiento. Capa de MODELO.</summary>
    public class Horario
    {
        public int      HorarioID     { get; set; }
        public int      TratamientoID { get; set; }
        public TimeSpan HoraToma      { get; set; }
        public int?     DiaSemana     { get; set; } // null = todos los días
        public bool     Activo        { get; set; } = true;

        public string NombrePaciente    { get; set; }
        public string NombreMedicamento { get; set; }
        public string Dosis             { get; set; }
    }
}

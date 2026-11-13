using System;

namespace MedicalRemember.Models
{
    /// <summary>Representa el tratamiento (receta) de un paciente con un medicamento. Capa de MODELO.</summary>
    public class Tratamiento
    {
        public int       TratamientoID  { get; set; }
        public int       PacienteID     { get; set; }
        public int       MedicamentoID  { get; set; }
        public string    Dosis          { get; set; }
        public DateTime  FechaInicio    { get; set; } = DateTime.Today;
        public DateTime? FechaFin       { get; set; }
        public string    Instrucciones  { get; set; }
        public bool      Activo         { get; set; } = true;

        // Campos de apoyo para mostrar en pantalla (no existen como columnas propias)
        public string NombrePaciente    { get; set; }
        public string NombreMedicamento { get; set; }
    }
}

using System;

namespace MedicalRemember.Models
{
    /// <summary>
    /// Representa un paciente del sistema.
    /// Capa de MODELO: solo datos, sin lógica de negocio ni de acceso a datos.
    /// </summary>
    public class Paciente
    {
        public int      PacienteID      { get; set; }
        public string   Nombre          { get; set; }
        public string   Apellido        { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string   Telefono        { get; set; }
        public string   Correo          { get; set; }
        public string   Direccion       { get; set; }
        public int      TipoPaciente    { get; set; } // 1=Adulto Mayor, 2=Paciente Crónico, 3=Otro
        public bool     Activo          { get; set; } = true;

        public string NombreCompleto => $"{Nombre} {Apellido}";

        public int Edad
        {
            get
            {
                int edad = DateTime.Today.Year - FechaNacimiento.Year;
                if (FechaNacimiento > DateTime.Today.AddYears(-edad)) edad--;
                return edad;
            }
        }

        public string TipoPacienteTexto
        {
            get
            {
                switch (TipoPaciente)
                {
                    case 1: return "Adulto Mayor";
                    case 2: return "Paciente Crónico";
                    default: return "Otro";
                }
            }
        }
    }
}

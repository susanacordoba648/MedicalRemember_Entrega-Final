using MedicalRemember.Api.Data;
using MedicalRemember.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace MedicalRemember.Api.Controllers
{
    // Une las tres tablas: paciente -> sus tratamientos -> los horarios de cada tratamiento.
    [ApiController]
    [Route("api/resumen")]
    public class ResumenController : ControllerBase
    {
        private readonly AppDbContext Db;
        public ResumenController(AppDbContext db) { Db = db; }

        // GET api/resumen -> todos los pacientes activos
        [HttpGet]
        public IActionResult Todos() =>
            Ok(Armar(Db.Pacientes.Where(p => p.Activo).OrderBy(p => p.Apellido).ThenBy(p => p.Nombre).ToList()));

        // GET api/resumen/3 -> un solo paciente
        [HttpGet("{pacienteId}")]
        public IActionResult Uno(int pacienteId)
        {
            var lista = Db.Pacientes.Where(p => p.PacienteID == pacienteId && p.Activo).ToList();
            if (lista.Count == 0) return NotFound("No existe el paciente " + pacienteId);
            return Ok(Armar(lista).First());
        }

        // Hace 3 consultas (pacientes, tratamientos, horarios) y las junta en memoria.
        private List<object> Armar(List<Paciente> pacientes)
        {
            var tratamientos = (from t in Db.Tratamientos
                                join m in Db.Medicamentos on t.MedicamentoID equals m.MedicamentoID
                                where t.Activo
                                select new { t.TratamientoID, t.PacienteID, Medicamento = m.Nombre, t.Dosis,
                                             t.FechaInicio, t.FechaFin, t.Instrucciones }).ToList();
            var horarios = Db.Horarios.Where(h => h.Activo).ToList();

            return pacientes.Select(p => (object)new
            {
                p.PacienteID,
                Nombre = p.Nombre + " " + p.Apellido,
                p.FechaNacimiento,
                p.Telefono,
                Tipo = p.TipoPaciente == 1 ? "Adulto mayor" : p.TipoPaciente == 2 ? "Paciente cronico" : "Otro",
                Tratamientos = tratamientos.Where(t => t.PacienteID == p.PacienteID).Select(t => new
                {
                    t.TratamientoID, t.Medicamento, t.Dosis, t.FechaInicio, t.FechaFin, t.Instrucciones,
                    Horarios = horarios.Where(h => h.TratamientoID == t.TratamientoID)
                                       .OrderBy(h => h.HoraToma)
                                       .Select(h => h.HoraToma.ToString(@"hh\:mm")).ToList()
                }).ToList()
            }).ToList();
        }
    }
}

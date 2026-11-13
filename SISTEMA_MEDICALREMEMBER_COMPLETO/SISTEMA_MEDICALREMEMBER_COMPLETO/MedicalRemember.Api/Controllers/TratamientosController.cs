using MedicalRemember.Api.Data;
using MedicalRemember.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace MedicalRemember.Api.Controllers
{
    [ApiController]
    [Route("api/tratamientos")]
    public class TratamientosController : ControllerBase
    {
        private readonly AppDbContext Db;
        public TratamientosController(AppDbContext db) { Db = db; }

        // GET api/tratamientos -> tratamientos activos con el nombre del paciente y del medicamento
        [HttpGet]
        public IActionResult Listar()
        {
            var lista = (from t in Db.Tratamientos
                         join p in Db.Pacientes on t.PacienteID equals p.PacienteID
                         join m in Db.Medicamentos on t.MedicamentoID equals m.MedicamentoID
                         where t.Activo
                         orderby p.Apellido, m.Nombre
                         select new { t.TratamientoID, Paciente = p.Nombre + " " + p.Apellido,
                                      Medicamento = m.Nombre, t.Dosis, t.FechaInicio, t.FechaFin, t.Instrucciones }).ToList();
            return Ok(lista);
        }

        // POST api/tratamientos -> asigna un medicamento a un paciente
        [HttpPost]
        public IActionResult Insertar(Tratamiento obj)
        {
            if (!Db.Pacientes.Any(p => p.PacienteID == obj.PacienteID && p.Activo)) return BadRequest("El paciente no existe");
            if (!Db.Medicamentos.Any(m => m.MedicamentoID == obj.MedicamentoID && m.Activo)) return BadRequest("El medicamento no existe");
            if (obj.FechaFin != null && obj.FechaFin < obj.FechaInicio) return BadRequest("La fecha fin no puede ser anterior al inicio");
            obj.TratamientoID = 0;
            obj.Activo = true;
            obj.FechaRegistro = DateTime.Now;
            Db.Tratamientos.Add(obj);
            Db.SaveChanges();
            return Ok("Tratamiento registrado con id " + obj.TratamientoID);
        }
    }
}

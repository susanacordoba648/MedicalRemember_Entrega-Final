using MedicalRemember.Api.Data;
using MedicalRemember.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace MedicalRemember.Api.Controllers
{
    // Horarios de toma: consulta con filtro por rango de horas, agenda de hoy, alta y baja.
    [ApiController]
    [Route("api/horarios")]
    public class HorariosController : ControllerBase
    {
        private readonly AppDbContext Db;
        public HorariosController(AppDbContext db) { Db = db; }

        static string NombreEstado(int e) => e == 1 ? "Tomado" : e == 2 ? "Omitido" : e == 3 ? "Tardio" : "Pendiente";

        // GET api/horarios                          -> todos los horarios activos
        // GET api/horarios?desde=07:00&hasta=13:00  -> solo los de ese rango de horas
        [HttpGet]
        public IActionResult Listar(TimeSpan? desde, TimeSpan? hasta)
        {
            TimeSpan d = desde ?? TimeSpan.Zero;
            TimeSpan h = hasta ?? new TimeSpan(23, 59, 59);

            var filas = (from hr in Db.Horarios
                         join t in Db.Tratamientos on hr.TratamientoID equals t.TratamientoID
                         join p in Db.Pacientes on t.PacienteID equals p.PacienteID
                         join m in Db.Medicamentos on t.MedicamentoID equals m.MedicamentoID
                         where hr.Activo && t.Activo && p.Activo && hr.HoraToma >= d && hr.HoraToma <= h
                         orderby hr.HoraToma
                         select new { hr.HorarioID, hr.TratamientoID, Paciente = p.Nombre + " " + p.Apellido,
                                      Medicamento = m.Nombre, t.Dosis, hr.HoraToma }).ToList();

            return Ok(filas.Select(f => new { f.HorarioID, f.TratamientoID, f.Paciente, f.Medicamento, f.Dosis,
                                              Hora = f.HoraToma.ToString(@"hh\:mm") }));
        }

        // GET api/horarios/agenda-hoy -> las tomas de hoy ordenadas por hora, con su estado actual
        [HttpGet("agenda-hoy")]
        public IActionResult AgendaHoy()
        {
            DateTime hoy = DateTime.Today, manana = hoy.AddDays(1);

            // Estado de cada horario hoy (si no hay registro, esta Pendiente)
            var estados = Db.Historial
                .Where(hc => hc.FechaHoraProgramada >= hoy && hc.FechaHoraProgramada < manana)
                .ToList()
                .GroupBy(hc => hc.HorarioID)
                .ToDictionary(g => g.Key, g => g.First().Estado);

            var filas = (from hr in Db.Horarios
                         join t in Db.Tratamientos on hr.TratamientoID equals t.TratamientoID
                         join p in Db.Pacientes on t.PacienteID equals p.PacienteID
                         join m in Db.Medicamentos on t.MedicamentoID equals m.MedicamentoID
                         where hr.Activo && t.Activo && p.Activo
                         orderby hr.HoraToma
                         select new { hr.HorarioID, Paciente = p.Nombre + " " + p.Apellido,
                                      Medicamento = m.Nombre, t.Dosis, hr.HoraToma }).ToList();

            return Ok(filas.Select(f => new { f.HorarioID, f.Paciente, f.Medicamento, f.Dosis,
                                              Hora = f.HoraToma.ToString(@"hh\:mm"),
                                              Estado = NombreEstado(estados.TryGetValue(f.HorarioID, out var est) ? est : 4) }));
        }

        // POST api/horarios -> {"tratamientoID": 3, "horaToma": "10:30:00"}
        [HttpPost]
        public IActionResult Insertar(Horario obj)
        {
            if (!Db.Tratamientos.Any(t => t.TratamientoID == obj.TratamientoID && t.Activo))
                return BadRequest("El tratamiento no existe");
            if (Db.Horarios.Any(x => x.TratamientoID == obj.TratamientoID && x.HoraToma == obj.HoraToma && x.Activo))
                return BadRequest("Ese horario ya existe para el tratamiento");
            obj.HorarioID = 0;
            obj.Activo = true;
            Db.Horarios.Add(obj);
            Db.SaveChanges();
            return Ok("Horario registrado con id " + obj.HorarioID);
        }

        // DELETE api/horarios/5 -> baja logica
        [HttpDelete("{id}")]
        public IActionResult Desactivar(int id)
        {
            Horario actual = Db.Horarios.Find(id);
            if (actual == null) return NotFound("No existe el horario " + id);
            actual.Activo = false;
            Db.SaveChanges();
            return Ok("Horario dado de baja");
        }
    }
}

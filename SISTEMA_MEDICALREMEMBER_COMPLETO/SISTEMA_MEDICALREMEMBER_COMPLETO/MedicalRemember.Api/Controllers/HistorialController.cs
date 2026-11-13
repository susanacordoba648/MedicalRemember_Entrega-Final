using MedicalRemember.Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace MedicalRemember.Api.Controllers
{
    [ApiController]
    [Route("api/historial")]
    public class HistorialController : ControllerBase
    {
        private readonly AppDbContext Db;
        public HistorialController(AppDbContext db) { Db = db; }

        static string NombreEstado(int e) => e == 1 ? "Tomado" : e == 2 ? "Omitido" : e == 3 ? "Tardio" : "Pendiente";

        // GET api/historial?desde=2026-10-01&hasta=2026-11-06 -> tomas del rango (por defecto, ultimos 30 dias)
        [HttpGet]
        public IActionResult Listar(DateTime? desde, DateTime? hasta)
        {
            DateTime d = (desde ?? DateTime.Today.AddDays(-30)).Date;
            DateTime h = (hasta ?? DateTime.Today).Date.AddDays(1);
            var filas = (from hc in Db.Historial
                         join hr in Db.Horarios on hc.HorarioID equals hr.HorarioID
                         join t in Db.Tratamientos on hr.TratamientoID equals t.TratamientoID
                         join p in Db.Pacientes on t.PacienteID equals p.PacienteID
                         join m in Db.Medicamentos on t.MedicamentoID equals m.MedicamentoID
                         where hc.FechaHoraProgramada >= d && hc.FechaHoraProgramada < h
                         orderby hc.FechaHoraProgramada descending
                         select new { hc.HistorialID, Paciente = p.Nombre + " " + p.Apellido, Medicamento = m.Nombre,
                                      t.Dosis, Programado = hc.FechaHoraProgramada, Tomado = hc.FechaHoraTomado, hc.Estado }).ToList();
            return Ok(filas.Select(f => new { f.HistorialID, f.Paciente, f.Medicamento, f.Dosis, f.Programado, f.Tomado, Estado = NombreEstado(f.Estado) }));
        }

        // POST api/historial/12/tomar -> confirma la toma (el trigger la marca "Tardio" si pasaron 30 min)
        [HttpPost("{id}/tomar")]
        public IActionResult Tomar(int id)
        {
            var reg = Db.Historial.Find(id);
            if (reg == null) return NotFound("No existe el registro " + id);
            reg.Estado = 1;
            reg.FechaHoraTomado = DateTime.Now;
            Db.SaveChanges();
            return Ok("Toma registrada");
        }

        // POST api/historial/12/omitir
        [HttpPost("{id}/omitir")]
        public IActionResult Omitir(int id)
        {
            var reg = Db.Historial.Find(id);
            if (reg == null) return NotFound("No existe el registro " + id);
            reg.Estado = 2;
            Db.SaveChanges();
            return Ok("Toma marcada como omitida");
        }
    }
}

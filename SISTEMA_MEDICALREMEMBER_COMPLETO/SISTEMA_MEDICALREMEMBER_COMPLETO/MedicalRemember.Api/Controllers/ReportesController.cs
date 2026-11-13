using MedicalRemember.Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace MedicalRemember.Api.Controllers
{
    // El "reporte API": devuelve el mismo resumen del reporte RDLC, pero en JSON.
    [ApiController]
    [Route("api/reportes")]
    public class ReportesController : ControllerBase
    {
        private readonly AppDbContext Db;
        public ReportesController(AppDbContext db) { Db = db; }

        // GET api/reportes/cumplimiento?desde=2026-10-01&hasta=2026-11-06
        [HttpGet("cumplimiento")]
        public IActionResult Cumplimiento(DateTime? desde, DateTime? hasta)
        {
            DateTime d = (desde ?? DateTime.Today.AddDays(-30)).Date;
            DateTime h = (hasta ?? DateTime.Today).Date.AddDays(1);

            var filas = (from hc in Db.Historial
                         join hr in Db.Horarios on hc.HorarioID equals hr.HorarioID
                         join t in Db.Tratamientos on hr.TratamientoID equals t.TratamientoID
                         join p in Db.Pacientes on t.PacienteID equals p.PacienteID
                         where hc.FechaHoraProgramada >= d && hc.FechaHoraProgramada < h
                         select new { p.PacienteID, Paciente = p.Nombre + " " + p.Apellido, hc.Estado }).ToList();

            var resumen = filas.GroupBy(f => new { f.PacienteID, f.Paciente })
                .Select(g => new
                {
                    g.Key.PacienteID,
                    g.Key.Paciente,
                    Total      = g.Count(),
                    Tomadas    = g.Count(x => x.Estado == 1),
                    Tardias    = g.Count(x => x.Estado == 3),
                    Omitidas   = g.Count(x => x.Estado == 2),
                    Pendientes = g.Count(x => x.Estado == 4),
                    Porcentaje = Math.Round(g.Count(x => x.Estado == 1) * 100.0 / g.Count(), 1)
                })
                .OrderBy(x => x.Paciente).ToList();

            return Ok(new { Desde = d, Hasta = h.AddDays(-1), Pacientes = resumen });
        }
    }
}

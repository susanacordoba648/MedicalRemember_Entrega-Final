using MedicalRemember.Api.Data;
using MedicalRemember.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace MedicalRemember.Api.Controllers
{
    // CRUD de pacientes con Entity Framework Core (ORM): trabajamos con objetos, EF genera el SQL.
    [ApiController]
    [Route("api/orm/pacientes")]                                  // URL base
    public class PacientesOrmController : ControllerBase
    {
        private readonly AppDbContext Db;
        public PacientesOrmController(AppDbContext db) { Db = db; }   // ASP.NET entrega el DbContext

        // GET api/orm/pacientes -> lista los activos
        [HttpGet]
        public List<Paciente> Listar() =>
            Db.Pacientes.Where(p => p.Activo).OrderBy(p => p.Apellido).ThenBy(p => p.Nombre).ToList();

        // GET api/orm/pacientes/5 -> uno solo
        [HttpGet("{id}")]
        public ActionResult<Paciente> Obtener(int id)
        {
            Paciente obj = Db.Pacientes.Find(id);                  // busca por llave primaria
            if (obj == null) return NotFound("No existe el paciente " + id);   // 404
            return obj;                                            // 200 + JSON
        }

        // POST api/orm/pacientes -> crea (el cuerpo es JSON)
        [HttpPost]
        public IActionResult Insertar(Paciente obj)
        {
            obj.PacienteID = 0;
            obj.FechaRegistro = DateTime.Now;
            obj.Activo = true;
            Db.Pacientes.Add(obj);
            Db.SaveChanges();                                      // aqui se ejecuta el INSERT
            return Ok("Paciente registrado con id " + obj.PacienteID);
        }

        // PUT api/orm/pacientes/5 -> modifica
        [HttpPut("{id}")]
        public IActionResult Actualizar(int id, Paciente obj)
        {
            Paciente actual = Db.Pacientes.Find(id);
            if (actual == null) return NotFound("No existe el paciente " + id);
            actual.Nombre = obj.Nombre;  actual.Apellido = obj.Apellido;
            actual.FechaNacimiento = obj.FechaNacimiento;
            actual.Telefono = obj.Telefono;  actual.Correo = obj.Correo;  actual.Direccion = obj.Direccion;
            actual.TipoPaciente = obj.TipoPaciente;
            Db.SaveChanges();                                      // UPDATE
            return Ok("Paciente actualizado");
        }

        // DELETE api/orm/pacientes/5 -> BAJA LOGICA (igual que el trigger de la base de datos)
        [HttpDelete("{id}")]
        public IActionResult Desactivar(int id)
        {
            Paciente actual = Db.Pacientes.Find(id);
            if (actual == null) return NotFound("No existe el paciente " + id);
            actual.Activo = false;
            Db.SaveChanges();
            return Ok("Paciente dado de baja");
        }
    }
}

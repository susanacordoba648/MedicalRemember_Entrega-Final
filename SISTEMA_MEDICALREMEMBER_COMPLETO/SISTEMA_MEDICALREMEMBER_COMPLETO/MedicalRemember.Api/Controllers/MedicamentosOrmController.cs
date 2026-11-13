using MedicalRemember.Api.Data;
using MedicalRemember.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace MedicalRemember.Api.Controllers
{
    [ApiController]
    [Route("api/orm/medicamentos")]
    public class MedicamentosOrmController : ControllerBase
    {
        private readonly AppDbContext Db;
        public MedicamentosOrmController(AppDbContext db) { Db = db; }

        [HttpGet]                                                  // GET api/orm/medicamentos
        public List<Medicamento> Listar() => Db.Medicamentos.Where(m => m.Activo).OrderBy(m => m.Nombre).ToList();

        [HttpGet("{id}")]                                          // GET api/orm/medicamentos/3
        public ActionResult<Medicamento> Obtener(int id)
        {
            Medicamento obj = Db.Medicamentos.Find(id);
            if (obj == null) return NotFound("No existe el medicamento " + id);
            return obj;
        }

        [HttpPost]                                                 // POST api/orm/medicamentos
        public IActionResult Insertar(Medicamento obj)
        {
            if (Db.Medicamentos.Any(m => m.Nombre == obj.Nombre && m.Activo))
                return BadRequest("El medicamento ya existe");
            obj.MedicamentoID = 0;
            obj.Activo = true;
            Db.Medicamentos.Add(obj);
            Db.SaveChanges();
            return Ok("Medicamento registrado con id " + obj.MedicamentoID);
        }

        [HttpPut("{id}")]                                          // PUT api/orm/medicamentos/3
        public IActionResult Actualizar(int id, Medicamento obj)
        {
            Medicamento actual = Db.Medicamentos.Find(id);
            if (actual == null) return NotFound("No existe el medicamento " + id);
            actual.Nombre = obj.Nombre;  actual.Descripcion = obj.Descripcion;
            actual.Unidad = obj.Unidad;  actual.ViaAdministracion = obj.ViaAdministracion;
            Db.SaveChanges();
            return Ok("Medicamento actualizado");
        }

        [HttpDelete("{id}")]                                       // DELETE api/orm/medicamentos/3 (baja logica)
        public IActionResult Desactivar(int id)
        {
            Medicamento actual = Db.Medicamentos.Find(id);
            if (actual == null) return NotFound("No existe el medicamento " + id);
            actual.Activo = false;
            Db.SaveChanges();
            return Ok("Medicamento dado de baja");
        }
    }
}

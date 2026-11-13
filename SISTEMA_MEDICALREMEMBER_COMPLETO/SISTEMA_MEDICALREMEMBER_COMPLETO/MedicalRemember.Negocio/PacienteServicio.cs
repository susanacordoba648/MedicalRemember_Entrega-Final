using System;
using System.Data;
using MedicalRemember.Data;
using MedicalRemember.Models;

namespace MedicalRemember.Business
{
    /// <summary>
    /// Capa de NEGOCIO: valida las reglas del sistema para Pacientes antes de
    /// delegar en la capa de Datos. Las pantallas (Forms) solo llaman a esta clase.
    /// </summary>
    public class PacienteServicio
    {
        private readonly PacienteDatos _datos = new PacienteDatos();

        public DataTable Listar(string filtro = "") => _datos.Listar(filtro);

        public Paciente ObtenerPorId(int id) => _datos.ObtenerPorId(id);

        public ResultadoOperacion Guardar(Paciente p)
        {
            if (string.IsNullOrWhiteSpace(p.Nombre) || string.IsNullOrWhiteSpace(p.Apellido))
                return ResultadoOperacion.Error("El nombre y el apellido son obligatorios.");

            if (p.FechaNacimiento > DateTime.Today)
                return ResultadoOperacion.Error("La fecha de nacimiento no puede ser futura.");

            if (!string.IsNullOrWhiteSpace(p.Correo) && !p.Correo.Contains("@"))
                return ResultadoOperacion.Error("El correo no tiene un formato válido.");

            try
            {
                if (p.PacienteID == 0) _datos.Insertar(p);
                else                   _datos.Actualizar(p);

                return ResultadoOperacion.Ok("Paciente guardado correctamente.");
            }
            catch (Exception ex)
            {
                return ResultadoOperacion.Error("No se pudo guardar el paciente: " + ex.Message);
            }
        }

        public ResultadoOperacion Eliminar(int id)
        {
            if (id == 0) return ResultadoOperacion.Error("Selecciona un paciente.");

            try
            {
                _datos.Eliminar(id);
                return ResultadoOperacion.Ok();
            }
            catch (Exception ex)
            {
                return ResultadoOperacion.Error("No se pudo eliminar el paciente: " + ex.Message);
            }
        }
    }
}

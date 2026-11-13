using System;
using System.Data;
using MedicalRemember.Data;
using MedicalRemember.Models;

namespace MedicalRemember.Business
{
    /// <summary>Capa de NEGOCIO: reglas de validación para Tratamientos.</summary>
    public class TratamientoServicio
    {
        private readonly TratamientoDatos _datos = new TratamientoDatos();
        private readonly MedicamentoDatos _datosMedicamentos = new MedicamentoDatos();

        public DataTable Listar() => _datos.Listar();

        public DataTable ListarPacientesParaCombo() => _datos.ListarParaCombo();

        public DataTable ListarMedicamentosParaCombo() => _datosMedicamentos.Listar();

        public ResultadoOperacion Guardar(Tratamiento t)
        {
            if (t.PacienteID == 0 || t.MedicamentoID == 0)
                return ResultadoOperacion.Error("Selecciona un paciente y un medicamento.");

            if (string.IsNullOrWhiteSpace(t.Dosis))
                return ResultadoOperacion.Error("La dosis es obligatoria.");

            if (t.FechaFin.HasValue && t.FechaFin.Value < t.FechaInicio)
                return ResultadoOperacion.Error("La fecha de fin no puede ser anterior a la fecha de inicio.");

            try
            {
                _datos.Insertar(t);
                return ResultadoOperacion.Ok("Tratamiento guardado correctamente.");
            }
            catch (Exception ex)
            {
                return ResultadoOperacion.Error("No se pudo guardar el tratamiento: " + ex.Message);
            }
        }

        public ResultadoOperacion Eliminar(int id)
        {
            if (id == 0) return ResultadoOperacion.Error("Selecciona un tratamiento.");
            try
            {
                _datos.Eliminar(id);
                return ResultadoOperacion.Ok();
            }
            catch (Exception ex)
            {
                return ResultadoOperacion.Error("No se pudo eliminar el tratamiento: " + ex.Message);
            }
        }
    }
}

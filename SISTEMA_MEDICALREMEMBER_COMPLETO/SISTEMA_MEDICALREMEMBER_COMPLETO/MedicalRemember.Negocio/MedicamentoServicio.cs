using System;
using System.Data;
using MedicalRemember.Data;
using MedicalRemember.Models;

namespace MedicalRemember.Business
{
    /// <summary>Capa de NEGOCIO: reglas de validación para Medicamentos.</summary>
    public class MedicamentoServicio
    {
        private readonly MedicamentoDatos _datos = new MedicamentoDatos();

        public DataTable Listar() => _datos.Listar();

        public Medicamento ObtenerPorId(int id) => _datos.ObtenerPorId(id);

        public ResultadoOperacion Guardar(Medicamento m)
        {
            if (string.IsNullOrWhiteSpace(m.Nombre))
                return ResultadoOperacion.Error("El nombre del medicamento es obligatorio.");

            try
            {
                if (m.MedicamentoID == 0) _datos.Insertar(m);
                else                      _datos.Actualizar(m);

                return ResultadoOperacion.Ok("Medicamento guardado correctamente.");
            }
            catch (Exception ex)
            {
                return ResultadoOperacion.Error("No se pudo guardar el medicamento: " + ex.Message);
            }
        }

        public ResultadoOperacion Eliminar(int id)
        {
            if (id == 0) return ResultadoOperacion.Error("Selecciona un medicamento.");
            try
            {
                _datos.Eliminar(id);
                return ResultadoOperacion.Ok();
            }
            catch (Exception ex)
            {
                return ResultadoOperacion.Error("No se pudo eliminar el medicamento: " + ex.Message);
            }
        }
    }
}

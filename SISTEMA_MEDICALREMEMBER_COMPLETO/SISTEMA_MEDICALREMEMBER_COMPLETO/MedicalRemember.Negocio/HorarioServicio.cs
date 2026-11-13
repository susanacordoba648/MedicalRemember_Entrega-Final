using System;
using System.Data;
using MedicalRemember.Data;
using MedicalRemember.Models;

namespace MedicalRemember.Business
{
    /// <summary>Capa de NEGOCIO: reglas de validación para Horarios y consultas del dashboard.</summary>
    public class HorarioServicio
    {
        private readonly HorarioDatos _datos = new HorarioDatos();
        private readonly DashboardDatos _datosDashboard = new DashboardDatos();

        public DataTable Listar() => _datos.ListarConDatos();

        public DataTable ListarTratamientosParaCombo() => _datos.ListarTratamientosParaCombo();

        public DataTable ListarDeHoy() => _datos.ListarDeHoy();

        public DataTable ListarProximasSinRegistrar() => _datos.ListarProximasSinRegistrar();

        public ResultadoOperacion Guardar(Horario h)
        {
            if (h.TratamientoID == 0)
                return ResultadoOperacion.Error("Selecciona un tratamiento.");

            try
            {
                _datos.Insertar(h);
                return ResultadoOperacion.Ok("Horario agregado correctamente.");
            }
            catch (Exception ex)
            {
                return ResultadoOperacion.Error("No se pudo agregar el horario: " + ex.Message);
            }
        }

        public ResultadoOperacion Eliminar(int id)
        {
            if (id == 0) return ResultadoOperacion.Error("Selecciona un horario.");
            try
            {
                _datos.Eliminar(id);
                return ResultadoOperacion.Ok();
            }
            catch (Exception ex)
            {
                return ResultadoOperacion.Error("No se pudo eliminar el horario: " + ex.Message);
            }
        }

        public ResultadoOperacion MarcarTomado(int horarioId, string estadoActual)
        {
            if (estadoActual != null && estadoActual.Contains("Tomado"))
                return ResultadoOperacion.Error("Este medicamento ya fue registrado como tomado.");

            try
            {
                _datos.MarcarTomadoHoy(horarioId);
                return ResultadoOperacion.Ok("Medicamento marcado como tomado.");
            }
            catch (Exception ex)
            {
                return ResultadoOperacion.Error("No se pudo registrar la toma: " + ex.Message);
            }
        }

        /// <summary>Contadores para las tarjetas del panel principal.</summary>
        public (int pacientes, int medicamentos, int tratamientos, int tomadosHoy) ObtenerResumen()
        {
            return (
                _datosDashboard.ContarPacientesActivos(),
                _datosDashboard.ContarMedicamentosActivos(),
                _datosDashboard.ContarTratamientosActivos(),
                _datosDashboard.ContarTomadosHoy()
            );
        }
    }
}

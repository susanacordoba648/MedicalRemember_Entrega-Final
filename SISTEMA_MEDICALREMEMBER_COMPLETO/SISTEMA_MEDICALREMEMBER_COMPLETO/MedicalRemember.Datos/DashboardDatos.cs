using System;

namespace MedicalRemember.Data
{
    /// <summary>Capa de DATOS: consultas de solo lectura para las tarjetas del panel principal.</summary>
    public class DashboardDatos
    {
        public int ContarPacientesActivos()
            => Convert.ToInt32(Conexion.Escalar("SELECT COUNT(*) FROM Pacientes WHERE Activo=1") ?? 0);

        public int ContarMedicamentosActivos()
            => Convert.ToInt32(Conexion.Escalar("SELECT COUNT(*) FROM Medicamentos WHERE Activo=1") ?? 0);

        public int ContarTratamientosActivos()
            => Convert.ToInt32(Conexion.Escalar("SELECT COUNT(*) FROM Tratamientos WHERE Activo=1") ?? 0);

        public int ContarTomadosHoy()
            => Convert.ToInt32(Conexion.Escalar(
                "SELECT COUNT(*) FROM HistorialCumplimiento WHERE Estado=1 AND CAST(FechaHoraProgramada AS DATE)=CAST(GETDATE() AS DATE)") ?? 0);
    }
}

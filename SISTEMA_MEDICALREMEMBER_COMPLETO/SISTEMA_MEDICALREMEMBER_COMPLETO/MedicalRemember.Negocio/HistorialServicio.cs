using System;
using System.Data;
using MedicalRemember.Data;

namespace MedicalRemember.Business
{
    /// <summary>Capa de NEGOCIO: consultas y cálculo del porcentaje de cumplimiento.</summary>
    public class HistorialServicio
    {
        private readonly HistorialDatos _datos = new HistorialDatos();

        public DataTable ListarPorRango(DateTime desde, DateTime hasta)
        {
            DateTime hastaFinDelDia = hasta.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            return _datos.ListarPorRango(desde.Date, hastaFinDelDia);
        }

        /// <summary>Calcula el % de cumplimiento (tomados / total) de un conjunto de resultados.</summary>
        public (int tomados, int total, double porcentaje) CalcularCumplimiento(DataTable historial)
        {
            int total = historial.Rows.Count;
            int tomados = 0;
            foreach (DataRow r in historial.Rows)
                if (r["Estado"].ToString().Contains("Tomado")) tomados++;

            double porcentaje = total > 0 ? tomados * 100.0 / total : 0;
            return (tomados, total, porcentaje);
        }
    }
}

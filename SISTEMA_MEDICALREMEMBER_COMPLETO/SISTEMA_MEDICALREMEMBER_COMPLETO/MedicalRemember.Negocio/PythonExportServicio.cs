using System;
using System.Data;
using System.IO;
using System.Text;

namespace MedicalRemember.Business
{
    /// <summary>
    /// Capa de NEGOCIO: prepara los datos del sistema para el módulo Python.
    /// Exporta a CSV (UTF-8) los tratamientos, las tomas de hoy y el historial reciente,
    /// usando los servicios que ya existen (la UI nunca toca la capa de Datos).
    /// </summary>
    public class PythonExportServicio
    {
        private readonly TratamientoServicio _tratamientos = new TratamientoServicio();
        private readonly HorarioServicio     _horarios     = new HorarioServicio();
        private readonly HistorialServicio   _historial    = new HistorialServicio();

        /// <summary>Rutas de los tres CSV generados.</summary>
        public class ArchivosExportados
        {
            public string Tratamientos { get; set; }
            public string Hoy          { get; set; }
            public string Historial    { get; set; }
        }

        public ArchivosExportados Exportar(string carpeta, int diasHistorial = 30)
        {
            Directory.CreateDirectory(carpeta);

            var archivos = new ArchivosExportados
            {
                Tratamientos = Path.Combine(carpeta, "tratamientos.csv"),
                Hoy          = Path.Combine(carpeta, "hoy.csv"),
                Historial    = Path.Combine(carpeta, "historial.csv")
            };

            EscribirCsv(_tratamientos.Listar(), archivos.Tratamientos);
            EscribirCsv(_horarios.ListarDeHoy(), archivos.Hoy);
            EscribirCsv(_historial.ListarPorRango(DateTime.Today.AddDays(-diasHistorial), DateTime.Today),
                        archivos.Historial);

            return archivos;
        }

        private static void EscribirCsv(DataTable dt, string ruta)
        {
            // UTF-8 con BOM: Python lo lee con encoding "utf-8-sig"
            using (var sw = new StreamWriter(ruta, false, new UTF8Encoding(true)))
            {
                var encabezado = new string[dt.Columns.Count];
                for (int i = 0; i < dt.Columns.Count; i++)
                    encabezado[i] = Escapar(dt.Columns[i].ColumnName);
                sw.WriteLine(string.Join(",", encabezado));

                foreach (DataRow fila in dt.Rows)
                {
                    var celdas = new string[dt.Columns.Count];
                    for (int i = 0; i < dt.Columns.Count; i++)
                        celdas[i] = Escapar(Convert.ToString(fila[i]));
                    sw.WriteLine(string.Join(",", celdas));
                }
            }
        }

        private static string Escapar(string valor)
        {
            if (valor == null) return "";
            if (valor.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0)
                return "\"" + valor.Replace("\"", "\"\"") + "\"";
            return valor;
        }
    }
}

using System;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Reporting.WinForms;
using MedicalRemember.Business;

namespace MedicalRemember.Forms
{
    /// <summary>
    /// Reporte RDLC (ReportViewer) del historial de cumplimiento.
    /// Pasos de la guía: el DISEÑO es Reportes\RptHistorial.rdlc, los DATOS vienen de la
    /// capa de Negocio (HistorialServicio) y el VISOR es el ReportViewer de este formulario.
    /// </summary>
    public class FrmReporteHistorial : Form
    {
        private ReportViewer rvReporte;
        private DateTimePicker dtpDesde, dtpHasta;
        private Button btnVer;

        private readonly string _usuario;
        private readonly HistorialServicio _servicio = new HistorialServicio();
        private readonly Color AZUL = Color.FromArgb(25, 80, 150);

        public FrmReporteHistorial(string usuario)
        {
            _usuario = string.IsNullOrWhiteSpace(usuario) ? "(sin sesión)" : usuario;
            Construir();
            Load += (s, e) => CargarReporte();      // evento Load, como indica la guía
        }

        private void Construir()
        {
            Text          = "Reporte de cumplimiento (RDLC)";
            Size          = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterParent;
            Font          = new Font("Segoe UI", 10F);

            // Panel superior (Dock = Top) con el filtro de fechas
            var pnl = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White };

            pnl.Controls.Add(new Label { Text = "Desde:", AutoSize = true, Location = new Point(15, 15) });
            dtpDesde = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short, Width = 120, Location = new Point(70, 11),
                Value = DateTime.Today.AddDays(-30)
            };
            pnl.Controls.Add(new Label { Text = "Hasta:", AutoSize = true, Location = new Point(210, 15) });
            dtpHasta = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short, Width = 120, Location = new Point(265, 11),
                Value = DateTime.Today
            };

            btnVer = new Button
            {
                Text = "📄  Ver reporte", Location = new Point(410, 9), Size = new Size(150, 32),
                BackColor = AZUL, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnVer.FlatAppearance.BorderSize = 0;
            btnVer.Click += (s, e) => CargarReporte();

            pnl.Controls.Add(dtpDesde);
            pnl.Controls.Add(dtpHasta);
            pnl.Controls.Add(btnVer);

            // El visor (Dock = Fill). Se crea por código, sin el asistente "Elegir informe".
            rvReporte = new ReportViewer { Dock = DockStyle.Fill, ShowParameterPrompts = false };

            Controls.Add(rvReporte);   // primero el Fill
            Controls.Add(pnl);         // luego el Top
        }

        private void CargarReporte()
        {
            try
            {
                if (dtpDesde.Value.Date > dtpHasta.Value.Date)
                {
                    MessageBox.Show("La fecha 'Desde' no puede ser mayor que 'Hasta'.", "Reporte",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 1. DATOS (capa Negocio). Las columnas se llaman igual que los campos del .rdlc.
                DataTable tabla = _servicio.ListarPorRango(dtpDesde.Value, dtpHasta.Value);
                var cumplimiento = _servicio.CalcularCumplimiento(tabla);

                // El Estado viene con emojis (✅ Tomado): se quitan para que el PDF/Word los muestre bien
                foreach (DataRow fila in tabla.Rows)
                    fila["Estado"] = Regex.Replace(Convert.ToString(fila["Estado"]), @"[^\w\s]", "").Trim();

                // 2. QUÉ reporte (nombre del recurso incrustado: RootNamespace.Carpeta.Archivo.rdlc)
                rvReporte.LocalReport.ReportEmbeddedResource = "MedicalRemember.Reportes.RptHistorial.rdlc";

                // 3. CON QUÉ datos ("DsHistorial" = nombre del conjunto de datos DENTRO del .rdlc)
                rvReporte.LocalReport.DataSources.Clear();
                rvReporte.LocalReport.DataSources.Add(new ReportDataSource("DsHistorial", tabla));

                // 4. PARÁMETROS (texto que no está en la tabla)
                rvReporte.LocalReport.SetParameters(new[]
                {
                    new ReportParameter("Usuario", _usuario),
                    new ReportParameter("Rango", dtpDesde.Value.ToString("dd/MM/yyyy") + " al " + dtpHasta.Value.ToString("dd/MM/yyyy")),
                    new ReportParameter("Cumplimiento",
                        cumplimiento.porcentaje.ToString("0.0") + " %  (" + cumplimiento.tomados + " de " + cumplimiento.total + " tomas)")
                });

                // 5. Dibujar
                rvReporte.SetDisplayMode(DisplayMode.PrintLayout);
                rvReporte.ZoomMode = ZoomMode.PageWidth;
                rvReporte.RefreshReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + (ex.InnerException != null ? "\n\n" + ex.InnerException.Message : ""),
                    "Medical Remember - Reporte", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

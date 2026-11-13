using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MedicalRemember.Business;

namespace MedicalRemember.Forms
{
    /// <summary>
    /// Pantalla que conecta el sistema C# con el módulo Python (Python\estructuras_medicas.py).
    /// 1) Exporta los datos de SQL Server a CSV (capa Negocio).
    /// 2) Ejecuta el script de Python (estructuras de datos + reporte PDF).
    /// 3) Muestra la salida del script y permite abrir el PDF.
    /// </summary>
    public class FrmModuloPython : Form
    {
        private TextBox txtSalida;
        private Button  btnEjecutar, btnAbrirPdf, btnInstalar;
        private Label   lblEstado;

        private readonly string _usuario;
        private string _rutaPdf;

        private readonly PythonExportServicio _exportador = new PythonExportServicio();
        private readonly Color VERDE = Color.FromArgb(20, 110, 70);

        public FrmModuloPython(string usuario)
        {
            _usuario = string.IsNullOrWhiteSpace(usuario) ? "(sin sesión)" : usuario;
            Construir();
        }

        private void Construir()
        {
            Text          = "Módulo Python - Estructuras de datos";
            Size          = new Size(950, 640);
            MinimumSize   = new Size(800, 500);
            StartPosition = FormStartPosition.CenterParent;
            BackColor     = Color.White;
            Font          = new Font("Segoe UI", 10F);

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 125 };

            pnlTop.Controls.Add(new Label
            {
                Text      = "🐍  Módulo Python: estructuras de datos y reporte PDF",
                Font      = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = VERDE,
                AutoSize  = true,
                Location  = new Point(15, 10)
            });

            pnlTop.Controls.Add(new Label
            {
                Text      = "Exporta los datos del sistema y los procesa con Python: lista, pila, cola, tupla, diccionario, " +
                            "conjunto, función, recursividad y un grafo de interacciones entre medicamentos.",
                ForeColor = Color.DimGray,
                Size      = new Size(900, 40),
                Location  = new Point(17, 45)
            });

            btnEjecutar = CrearBoton("▶  Ejecutar módulo Python", VERDE, new Point(17, 85), 230);
            btnEjecutar.Click += BtnEjecutar_Click;

            btnAbrirPdf = CrearBoton("📄  Abrir reporte PDF", Color.FromArgb(25, 80, 150), new Point(257, 85), 200);
            btnAbrirPdf.Enabled = false;
            btnAbrirPdf.Click += BtnAbrirPdf_Click;

            btnInstalar = CrearBoton("⬇  Instalar reportlab (pip)", Color.FromArgb(100, 50, 180), new Point(467, 85), 230);
            btnInstalar.Click += BtnInstalar_Click;

            lblEstado = new Label { AutoSize = true, ForeColor = Color.DimGray, Location = new Point(710, 92) };

            pnlTop.Controls.Add(btnEjecutar);
            pnlTop.Controls.Add(btnAbrirPdf);
            pnlTop.Controls.Add(btnInstalar);
            pnlTop.Controls.Add(lblEstado);

            txtSalida = new TextBox
            {
                Dock       = DockStyle.Fill,
                Multiline  = true,
                ReadOnly   = true,
                ScrollBars = ScrollBars.Both,
                WordWrap   = false,
                Font       = new Font("Consolas", 9.5F),
                BackColor  = Color.FromArgb(250, 252, 250)
            };

            Controls.Add(txtSalida);
            Controls.Add(pnlTop);
        }

        private Button CrearBoton(string texto, Color color, Point ubicacion, int ancho)
        {
            var b = new Button
            {
                Text      = texto,
                Location  = ubicacion,
                Size      = new Size(ancho, 34),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        // ── Ejecutar: exporta CSV + corre el script ─────────────────────
        private async void BtnEjecutar_Click(object sender, EventArgs e)
        {
            string script = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Python", "estructuras_medicas.py");
            if (!File.Exists(script))
            {
                MessageBox.Show("No se encontró el script de Python:\n" + script +
                    "\n\nRevisa que Python\\estructuras_medicas.py tenga 'Copiar en el directorio de salida = Copiar si es posterior'.",
                    "Módulo Python", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Ocupado(true, "Exportando datos...");
            string estadoFinal = "";
            try
            {
                string carpeta = Path.Combine(Path.GetTempPath(), "MedicalRemember_Python");
                var csv = _exportador.Exportar(carpeta);

                string docs = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MedicalRemember");
                Directory.CreateDirectory(docs);
                string pdf = Path.Combine(docs, "Reporte_Estructuras_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf");

                string argumentos =
                    "--tratamientos " + Q(csv.Tratamientos) +
                    " --hoy "         + Q(csv.Hoy) +
                    " --historial "   + Q(csv.Historial) +
                    " --salida "      + Q(pdf) +
                    " --usuario "     + Q(_usuario);

                lblEstado.Text = "Ejecutando Python...";
                var r = await Task.Run(() => EjecutarPython(script, argumentos));

                // Privacidad: los CSV contienen datos de pacientes (información sensible); se borran al terminar
                foreach (string f in new[] { csv.Tratamientos, csv.Hoy, csv.Historial })
                {
                    try { File.Delete(f); } catch (Exception) { /* si no se puede borrar, no se interrumpe el flujo */ }
                }

                var sb = new StringBuilder();
                sb.AppendLine(r.salida);
                if (!string.IsNullOrWhiteSpace(r.error))
                {
                    sb.AppendLine();
                    sb.AppendLine("--- Mensajes de error de Python ---");
                    sb.AppendLine(r.error);
                }
                txtSalida.Text = sb.ToString().Replace("\r\n", "\n").Replace("\n", Environment.NewLine);

                _rutaPdf = BuscarMarca(r.salida, "PDF_GENERADO=");
                btnAbrirPdf.Enabled = _rutaPdf != null && File.Exists(_rutaPdf);
                estadoFinal = btnAbrirPdf.Enabled ? "✔ PDF generado"
                            : (r.codigo == 0 ? "Listo (sin PDF)" : "✖ Hubo un error");
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo ejecutar el módulo Python:\n\n" + ex.Message,
                    "Módulo Python", MessageBoxButtons.OK, MessageBoxIcon.Error);
                estadoFinal = "✖ Error";
            }
            finally
            {
                Ocupado(false, estadoFinal);
            }
        }

        private async void BtnInstalar_Click(object sender, EventArgs e)
        {
            Ocupado(true, "Instalando reportlab...");
            var r = await Task.Run(() => EjecutarPythonConArgumentos("-m pip install reportlab"));
            txtSalida.Text = (r.salida + Environment.NewLine + r.error).Trim();
            Ocupado(false, r.codigo == 0 ? "✔ reportlab instalado" : "✖ No se pudo instalar");
        }

        private void BtnAbrirPdf_Click(object sender, EventArgs e)
        {
            if (_rutaPdf == null || !File.Exists(_rutaPdf)) return;
            Process.Start(new ProcessStartInfo(_rutaPdf) { UseShellExecute = true });
        }

        // ── Utilidades ──────────────────────────────────────────────────
        private void Ocupado(bool ocupado, string mensaje)
        {
            btnEjecutar.Enabled = !ocupado;
            btnInstalar.Enabled = !ocupado;
            lblEstado.Text      = mensaje;
            UseWaitCursor       = ocupado;
        }

        private static string Q(string valor)
        {
            return "\"" + (valor ?? "").Replace("\"", "") + "\"";
        }

        private static string BuscarMarca(string texto, string marca)
        {
            foreach (string linea in (texto ?? "").Split('\n'))
            {
                string l = linea.Trim();
                if (l.StartsWith(marca)) return l.Substring(marca.Length);
            }
            return null;
        }

        private static (int codigo, string salida, string error) EjecutarPython(string script, string argumentos)
        {
            return EjecutarPythonConArgumentos(Q(script) + " " + argumentos);
        }

        /// <summary>
        /// Prueba "py -3", "python" y "python3" hasta que alguno funcione
        /// (en Windows depende de cómo se instaló Python).
        /// </summary>
        private static (int codigo, string salida, string error) EjecutarPythonConArgumentos(string argumentos)
        {
            string[][] candidatos =
            {
                new[] { "py",      "-3 " },
                new[] { "python",  ""    },
                new[] { "python3", ""    }
            };

            string ultimoError = "";
            foreach (var c in candidatos)
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName               = c[0],
                        Arguments              = c[1] + argumentos,
                        UseShellExecute        = false,
                        CreateNoWindow         = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError  = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding  = Encoding.UTF8
                    };
                    psi.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";

                    using (var p = Process.Start(psi))
                    {
                        Task<string> err = p.StandardError.ReadToEndAsync();
                        string salida = p.StandardOutput.ReadToEnd();
                        p.WaitForExit();
                        string error = err.Result;

                        // El alias de la Microsoft Store devuelve error sin salida: probar el siguiente
                        if (p.ExitCode != 0 && string.IsNullOrWhiteSpace(salida) && error.Contains("Python was not found"))
                            continue;

                        return (p.ExitCode, salida, error);
                    }
                }
                catch (Win32Exception ex)
                {
                    ultimoError = ex.Message;      // no existe ese ejecutable: probar el siguiente
                }
            }

            return (-1, "",
                "No se encontró Python en este equipo (" + ultimoError + ").\n" +
                "Instálalo desde python.org y marca 'Add Python to PATH'.");
        }
    }
}

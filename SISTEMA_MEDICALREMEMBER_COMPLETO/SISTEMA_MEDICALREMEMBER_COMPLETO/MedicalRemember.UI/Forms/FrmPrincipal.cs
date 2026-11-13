using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MedicalRemember.Business;

namespace MedicalRemember.Forms
{
    public class FrmPrincipal : Form
    {
        // ── Controles ──────────────────────────────────────────
        private MenuStrip  menuPrincipal;
        private Panel      pnlMenu;
        private Panel      pnlContenido;
        private Label      lblTituloPagina;
        private Label      lblFechaHora;
        private Panel      pnlCards;
        private DataGridView dgvHoy;
        private Label      lblSubtituloGrid;
        private Button     btnMarcarTomado;
        private Button     btnSalirMenu;
        private Timer      timerAlertas;

        private readonly HorarioServicio _servicio = new HorarioServicio();

        // ── Colores del tema ───────────────────────────────────
        private readonly Color AZUL       = Color.FromArgb(25, 80, 150);
        private readonly Color AZUL_HOVER = Color.FromArgb(50, 110, 190);
        private readonly Color FONDO      = Color.FromArgb(245, 248, 252);
        private readonly Color ROJO       = Color.FromArgb(190, 40, 40);

        public FrmPrincipal()
        {
            InicializarComponentes();
            this.Load += (s, e) => CargarDashboard();
        }

        private void InicializarComponentes()
        {
            // ── Formulario ──
            this.Text            = "Medical Remember";
            this.Size            = new Size(1050, 680);
            this.MinimumSize     = new Size(900, 600);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.BackColor       = FONDO;
            this.Font            = new Font("Segoe UI", 10F);

            // ═══════════════════════════════════════════════════
            // MENU SUPERIOR (Archivo → Cerrar sesión / Salir)
            // ═══════════════════════════════════════════════════
            menuPrincipal = new MenuStrip { BackColor = Color.White };
            var mnuArchivo = new ToolStripMenuItem("Archivo");
            var mnuCerrarSesion = new ToolStripMenuItem("🔓  Cerrar sesión", null, MnuCerrarSesion_Click);
            var mnuSalir = new ToolStripMenuItem("🚪  Salir del programa", null, MnuSalir_Click);
            mnuArchivo.DropDownItems.Add(mnuCerrarSesion);
            mnuArchivo.DropDownItems.Add(new ToolStripSeparator());
            mnuArchivo.DropDownItems.Add(mnuSalir);
            menuPrincipal.Items.Add(mnuArchivo);
            this.Controls.Add(menuPrincipal);
            this.MainMenuStrip = menuPrincipal;

            // ═══════════════════════════════════════════════════
            // PANEL MENU IZQUIERDO
            // ═══════════════════════════════════════════════════
            pnlMenu = new Panel
            {
                Dock      = DockStyle.Left,
                Width     = 230,
                BackColor = AZUL
            };

            var lblLogo = new Label
            {
                Text      = "💊 Medical Remember",
                Font      = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock      = DockStyle.Top,
                Height    = 80,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding   = new Padding(5, 10, 5, 0)
            };
            pnlMenu.Controls.Add(lblLogo);

            var sep = new Label
            {
                Dock      = DockStyle.Top,
                Height    = 1,
                BackColor = Color.FromArgb(60, 110, 180)
            };
            pnlMenu.Controls.Add(sep);

            // Boton Salir, fijo abajo del menu lateral
            btnSalirMenu = new Button
            {
                Text      = "🚪  Salir",
                Dock      = DockStyle.Bottom,
                Height    = 48,
                BackColor = Color.FromArgb(15, 55, 110),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnSalirMenu.FlatAppearance.BorderSize = 0;
            btnSalirMenu.FlatAppearance.MouseOverBackColor = ROJO;
            btnSalirMenu.Click += MnuSalir_Click;
            pnlMenu.Controls.Add(btnSalirMenu);

            // Botones del menu
            string[][] items = {
                new[] { "🏠", "  Inicio"        },
                new[] { "👤", "  Pacientes"      },
                new[] { "💊", "  Medicamentos"   },
                new[] { "📋", "  Tratamientos"   },
                new[] { "🕐", "  Horarios"       },
                new[] { "📊", "  Historial"      },
                new[] { "📄", "  Reporte RDLC"   },
                new[] { "🐍", "  Módulo Python"  },
            };

            var pnlBtns = new FlowLayoutPanel
            {
                Dock         = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding      = new Padding(8, 10, 8, 0)
            };

            foreach (var item in items)
            {
                var btn = new Button
                {
                    Text      = item[0] + item[1],
                    Width     = 210,
                    Height    = 46,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = AZUL,
                    ForeColor = Color.White,
                    Font      = new Font("Segoe UI", 11F),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding   = new Padding(8, 0, 0, 0),
                    Margin    = new Padding(0, 2, 0, 2),
                    Cursor    = Cursors.Hand,
                    Tag       = item[1].Trim()
                };
                btn.FlatAppearance.BorderSize       = 0;
                btn.FlatAppearance.MouseOverBackColor = AZUL_HOVER;
                btn.Click += BtnMenu_Click;
                pnlBtns.Controls.Add(btn);
            }

            pnlMenu.Controls.Add(pnlBtns);

            // ═══════════════════════════════════════════════════
            // PANEL CONTENIDO DERECHO
            // ═══════════════════════════════════════════════════
            pnlContenido = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = FONDO,
                Padding   = new Padding(20, 15, 20, 15)
            };

            var pnlTop = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 65,
                BackColor = Color.White,
                Padding   = new Padding(10, 0, 10, 0)
            };
            pnlTop.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 220, 220)),
                    0, pnlTop.Height - 1, pnlTop.Width, pnlTop.Height - 1);
            };

            lblTituloPagina = new Label
            {
                Text      = "Panel Principal",
                Font      = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = AZUL,
                AutoSize  = true,
                Location  = new Point(12, 10)
            };

            lblFechaHora = new Label
            {
                Text      = DateTime.Now.ToString("dddd, dd 'de' MMMM yyyy"),
                Font      = new Font("Segoe UI", 9F),
                ForeColor = Color.Gray,
                AutoSize  = true,
                Location  = new Point(14, 42)
            };

            pnlTop.Controls.Add(lblTituloPagina);
            pnlTop.Controls.Add(lblFechaHora);

            pnlCards = new Panel
            {
                Dock   = DockStyle.Top,
                Height = 120,
                BackColor = FONDO
            };

            lblSubtituloGrid = new Label
            {
                Text      = "Medicamentos programados para hoy:",
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = AZUL,
                Dock      = DockStyle.Top,
                Height    = 28
            };

            dgvHoy = new DataGridView
            {
                Dock                = DockStyle.Fill,
                BackgroundColor     = Color.White,
                BorderStyle         = BorderStyle.None,
                RowHeadersVisible   = false,
                AllowUserToAddRows  = false,
                ReadOnly            = true,
                SelectionMode       = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font                = new Font("Segoe UI", 10F),
                GridColor           = Color.FromArgb(230, 230, 230),
                MultiSelect         = false,
                CellBorderStyle     = DataGridViewCellBorderStyle.SingleHorizontal
            };
            dgvHoy.DefaultCellStyle.SelectionBackColor = Color.FromArgb(210, 225, 245);
            dgvHoy.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvHoy.ColumnHeadersDefaultCellStyle.BackColor  = AZUL;
            dgvHoy.ColumnHeadersDefaultCellStyle.ForeColor  = Color.White;
            dgvHoy.ColumnHeadersDefaultCellStyle.Font       = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvHoy.ColumnHeadersDefaultCellStyle.Padding    = new Padding(6, 0, 0, 0);
            dgvHoy.EnableHeadersVisualStyles = false;
            dgvHoy.ColumnHeadersHeight       = 36;
            dgvHoy.RowTemplate.Height        = 34;
            dgvHoy.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 252, 255);

            btnMarcarTomado = new Button
            {
                Text      = "✅   Marcar como Tomado",
                Dock      = DockStyle.Bottom,
                Height    = 46,
                BackColor = Color.FromArgb(30, 130, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnMarcarTomado.FlatAppearance.BorderSize = 0;
            btnMarcarTomado.Click += BtnMarcarTomado_Click;

            pnlContenido.Controls.Add(dgvHoy);
            pnlContenido.Controls.Add(btnMarcarTomado);
            pnlContenido.Controls.Add(lblSubtituloGrid);
            pnlContenido.Controls.Add(pnlCards);
            pnlContenido.Controls.Add(pnlTop);

            this.Controls.Add(pnlContenido);
            this.Controls.Add(pnlMenu);

            timerAlertas = new Timer { Interval = 60000 };
            timerAlertas.Tick += Timer_Tick;
            timerAlertas.Start();
        }

        // ── Carga inicial del dashboard (delegado a la capa de Negocio) ──
        private void CargarDashboard()
        {
            CargarTarjetas();
            CargarMedicamentosHoy();
        }

        private void CargarTarjetas()
        {
            pnlCards.Controls.Clear();

            var resumen = _servicio.ObtenerResumen();
            object[] valores = { resumen.pacientes, resumen.medicamentos, resumen.tratamientos, resumen.tomadosHoy };
            string[] titulos = { "Pacientes", "Medicamentos", "Tratamientos", "Tomados Hoy" };
            string[] iconos  = { "👤", "💊", "📋", "✅" };
            Color[]  colores = {
                Color.FromArgb(25,  80, 150),
                Color.FromArgb(0,  140, 120),
                Color.FromArgb(100, 50, 180),
                Color.FromArgb(30, 130,  60)
            };

            int x = 0;
            for (int i = 0; i < 4; i++)
            {
                var card = new Panel
                {
                    Location  = new Point(x, 10),
                    Size      = new Size(190, 96),
                    BackColor = colores[i]
                };

                card.Controls.Add(new Label
                {
                    Text      = iconos[i] + "  " + valores[i],
                    Font      = new Font("Segoe UI", 22F, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize  = true,
                    Location  = new Point(14, 12)
                });
                card.Controls.Add(new Label
                {
                    Text      = titulos[i],
                    Font      = new Font("Segoe UI", 10F),
                    ForeColor = Color.FromArgb(210, 235, 255),
                    AutoSize  = true,
                    Location  = new Point(16, 62)
                });

                pnlCards.Controls.Add(card);
                x += 200;
            }
        }

        private void CargarMedicamentosHoy()
        {
            DataTable dt = _servicio.ListarDeHoy();
            dgvHoy.DataSource = dt;

            if (dgvHoy.Columns.Contains("HorarioID"))
                dgvHoy.Columns["HorarioID"].Visible = false;

            foreach (DataGridViewRow r in dgvHoy.Rows)
            {
                string est = r.Cells["Estado"].Value?.ToString() ?? "";
                if      (est.Contains("Tomado"))   r.DefaultCellStyle.BackColor = Color.FromArgb(220, 255, 220);
                else if (est.Contains("Omitido"))  r.DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
                else if (est.Contains("Tardio"))   r.DefaultCellStyle.BackColor = Color.FromArgb(255, 252, 200);
            }
        }

        private void BtnMarcarTomado_Click(object sender, EventArgs e)
        {
            if (dgvHoy.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecciona un medicamento de la lista.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var fila  = dgvHoy.SelectedRows[0];
            string est = fila.Cells["Estado"].Value?.ToString() ?? "";
            int    hid  = Convert.ToInt32(fila.Cells["HorarioID"].Value);
            string med  = fila.Cells["Medicamento"].Value?.ToString();
            string hora = fila.Cells["Hora"].Value?.ToString();

            if (MessageBox.Show(
                    $"¿Confirmas que tomaste:\n\n💊  {med}  —  {hora}?",
                    "Confirmar toma",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            ResultadoOperacion resultado = _servicio.MarcarTomado(hid, est);
            if (!resultado.Exito)
            {
                MessageBox.Show(resultado.Mensaje, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MessageBox.Show("✅ " + resultado.Mensaje, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            CargarDashboard();
        }

        // ── Timer: alerta automatica ────────────────────────────
        private void Timer_Tick(object sender, EventArgs e)
        {
            lblFechaHora.Text = DateTime.Now.ToString("dddd, dd 'de' MMMM yyyy   HH:mm");

            DataTable dt = _servicio.ListarProximasSinRegistrar();
            foreach (DataRow row in dt.Rows)
            {
                System.Media.SystemSounds.Exclamation.Play();
                MessageBox.Show(
                    "⏰  HORA DE TOMAR MEDICAMENTO\n\n" +
                    $"Paciente:    {row["Paciente"]}\n" +
                    $"💊  {row["Medicamento"]}  —  {row["Dosis"]}\n" +
                    $"Hora:  {row["Hora"]}",
                    "Alerta de Medicacion",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ── Navegacion del menu ─────────────────────────────────
        private void BtnMenu_Click(object sender, EventArgs e)
        {
            string tag = ((Button)sender).Tag?.ToString() ?? "";
            switch (tag)
            {
                case "Pacientes":
                    new FrmPacientes().ShowDialog(this);
                    CargarDashboard();
                    break;
                case "Medicamentos":
                    new FrmMedicamentos().ShowDialog(this);
                    CargarDashboard();
                    break;
                case "Tratamientos":
                    new FrmTratamientos().ShowDialog(this);
                    CargarDashboard();
                    break;
                case "Horarios":
                    new FrmHorarios().ShowDialog(this);
                    CargarMedicamentosHoy();
                    break;
                case "Historial":
                    new FrmHistorial().ShowDialog(this);
                    break;
                case "Reporte RDLC":
                    new FrmReporteHistorial(UsuarioActual()).ShowDialog(this);
                    break;
                case "Módulo Python":
                    new FrmModuloPython(UsuarioActual()).ShowDialog(this);
                    break;
                default:
                    CargarDashboard();
                    break;
            }
        }

        // Nombre del usuario en sesión (se toma del título de la ventana)
        private string UsuarioActual()
        {
            int i = this.Text.IndexOf("Sesión:");
            return i >= 0 ? this.Text.Substring(i + 7).Trim() : "";
        }

        // ── Cerrar sesión: vuelve al Login sin cerrar la aplicación ──
        private void MnuCerrarSesion_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Deseas cerrar la sesión actual?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            this.Hide();

            using (var login = new FrmLogin())
            {
                if (login.ShowDialog() == DialogResult.OK)
                {
                    this.Text = "Medical Remember  -  Sesión: "
                        + login.NombreCompletoUsuario + " (" + login.RolUsuario + ")";
                    this.Show();
                    CargarDashboard();
                }
                else
                {
                    Application.Exit();
                }
            }
        }

        // ── Salir por completo del programa ─────────────────────
        private void MnuSalir_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Deseas salir de Medical Remember?", "Confirmar salida",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timerAlertas?.Stop();
            timerAlertas?.Dispose();
            base.OnFormClosed(e);
        }
    }
}

using System;
using System.Drawing;
using System.Windows.Forms;
using MedicalRemember.Business;
using MedicalRemember.Models;

namespace MedicalRemember.Forms
{
    // ============================================================
    // FrmHorarios
    // ============================================================
    public class FrmHorarios : Form
    {
        private DataGridView dgv;
        private ComboBox cmbTratamiento, cmbDia;
        private DateTimePicker dtpHora;
        private Button btnAgregar, btnEliminar;

        private readonly HorarioServicio _servicio = new HorarioServicio();

        private readonly Color VERDE_OSCURO = Color.FromArgb(20, 110, 70);

        public FrmHorarios()
        {
            Construir();
            CargarCombos();
            CargarGrid();
        }

        private void Construir()
        {
            Text          = "Horarios de Medicación";
            Size          = new Size(950, 580);
            StartPosition = FormStartPosition.CenterParent;
            BackColor     = Color.White;
            Font          = new Font("Segoe UI", 10F);
            MinimizeBox   = false;

            Controls.Add(new Label
            {
                Text      = "🕐  Horarios de Medicación",
                Font      = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = VERDE_OSCURO,
                AutoSize  = true,
                Location  = new Point(20, 15)
            });

            dgv = new DataGridView
            {
                Location            = new Point(20, 55),
                Size                = new Size(565, 480),
                BackgroundColor     = Color.White,
                BorderStyle         = BorderStyle.FixedSingle,
                RowHeadersVisible   = false,
                AllowUserToAddRows  = false,
                ReadOnly            = true,
                SelectionMode       = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font                = new Font("Segoe UI", 9.5F),
                GridColor           = Color.FromArgb(225, 225, 225),
                MultiSelect         = false,
                ColumnHeadersHeight = 34,
                RowTemplate         = { Height = 32 },
                CellBorderStyle     = DataGridViewCellBorderStyle.SingleHorizontal
            };
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 252, 245);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 240, 220);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = VERDE_OSCURO;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            Controls.Add(dgv);

            var grp = new GroupBox
            {
                Text      = "Agregar Horario",
                Location  = new Point(604, 55),
                Size      = new Size(320, 380),
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = VERDE_OSCURO
            };
            Controls.Add(grp);

            int y = 28;
            Etq(grp, "Tratamiento *", 10, y); y += 22;
            cmbTratamiento = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location      = new Point(10, y),
                Size          = new Size(294, 28)
            };
            grp.Controls.Add(cmbTratamiento); y += 42;

            Etq(grp, "Hora de toma *", 10, y); y += 22;
            dtpHora = new DateTimePicker
            {
                Format     = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Location   = new Point(10, y),
                Size       = new Size(294, 28)
            };
            grp.Controls.Add(dtpHora); y += 42;

            Etq(grp, "Día de la semana", 10, y); y += 22;
            cmbDia = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location      = new Point(10, y),
                Size          = new Size(294, 28)
            };
            cmbDia.Items.AddRange(new[] {
                "Todos los días",
                "Lunes", "Martes", "Miércoles",
                "Jueves", "Viernes", "Sábado", "Domingo"
            });
            cmbDia.SelectedIndex = 0;
            grp.Controls.Add(cmbDia); y += 55;

            btnAgregar = BtnH("➕ Agregar Horario", new Point(10, y), new Size(294, 44), VERDE_OSCURO, BtnAgregar_Click);
            grp.Controls.Add(btnAgregar); y += 56;

            btnEliminar = BtnH("🗑  Eliminar Seleccionado", new Point(10, y), new Size(294, 38), Color.FromArgb(190, 40, 40), BtnEliminar_Click);
            grp.Controls.Add(btnEliminar);
        }

        private void Etq(Control p, string t, int x, int y)
        { p.Controls.Add(new Label { Text = t, Font = new Font("Segoe UI", 9F), ForeColor = Color.Gray, Location = new Point(x, y), AutoSize = true }); }

        private Button BtnH(string t, Point loc, Size sz, Color c, EventHandler ev)
        {
            var b = new Button { Text = t, Location = loc, Size = sz, BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0; b.Click += ev; return b;
        }

        private void CargarCombos()
        {
            var dt = _servicio.ListarTratamientosParaCombo();
            cmbTratamiento.DataSource    = dt;
            cmbTratamiento.DisplayMember = "Descripcion";
            cmbTratamiento.ValueMember   = "TratamientoID";
        }

        private void CargarGrid()
        {
            dgv.DataSource = _servicio.Listar();
            if (dgv.Columns.Contains("HorarioID"))
                dgv.Columns["HorarioID"].Visible = false;
        }

        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            if (cmbTratamiento.SelectedValue == null) return;

            int diaIdx = cmbDia.SelectedIndex;
            var h = new Horario
            {
                TratamientoID = (int)cmbTratamiento.SelectedValue,
                HoraToma      = dtpHora.Value.TimeOfDay,
                DiaSemana     = diaIdx == 0 ? (int?)null : diaIdx
            };

            ResultadoOperacion resultado = _servicio.Guardar(h);
            if (!resultado.Exito)
            {
                MessageBox.Show(resultado.Mensaje, "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show("✅ " + resultado.Mensaje, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            cmbDia.SelectedIndex = 0;
            CargarGrid();
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecciona un horario.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["HorarioID"].Value);
            if (MessageBox.Show("¿Eliminar este horario?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            _servicio.Eliminar(id);
            CargarGrid();
        }
    }


    // ============================================================
    // FrmHistorial
    // ============================================================
    public class FrmHistorial : Form
    {
        private DataGridView dgv;
        private DateTimePicker dtpDesde, dtpHasta;
        private Label lblPorcentaje;

        private readonly HistorialServicio _servicio = new HistorialServicio();

        private readonly Color NARANJA = Color.FromArgb(180, 100, 0);

        public FrmHistorial()
        {
            Construir();
            CargarHistorial();
        }

        private void Construir()
        {
            Text          = "Historial de Cumplimiento";
            Size          = new Size(920, 600);
            StartPosition = FormStartPosition.CenterParent;
            BackColor     = Color.White;
            Font          = new Font("Segoe UI", 10F);
            MinimizeBox   = false;

            Controls.Add(new Label
            {
                Text      = "📊  Historial de Cumplimiento",
                Font      = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = NARANJA,
                AutoSize  = true,
                Location  = new Point(20, 15)
            });

            Controls.Add(new Label { Text = "Desde:", AutoSize = true, Location = new Point(20, 60), ForeColor = Color.Gray });
            dtpDesde = new DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(75, 57), Size = new Size(130, 28), Value = DateTime.Today.AddDays(-30) };
            Controls.Add(dtpDesde);

            Controls.Add(new Label { Text = "Hasta:", AutoSize = true, Location = new Point(220, 60), ForeColor = Color.Gray });
            dtpHasta = new DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(275, 57), Size = new Size(130, 28), Value = DateTime.Today };
            Controls.Add(dtpHasta);

            var btnFiltrar = new Button
            {
                Text      = "🔍 Filtrar",
                Location  = new Point(420, 54),
                Size      = new Size(110, 32),
                BackColor = NARANJA,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnFiltrar.FlatAppearance.BorderSize = 0;
            btnFiltrar.Click += (s, e) => CargarHistorial();
            Controls.Add(btnFiltrar);

            lblPorcentaje = new Label
            {
                Text      = "Cumplimiento: --",
                Font      = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 139, 34),
                AutoSize  = true,
                Location  = new Point(548, 60)
            };
            Controls.Add(lblPorcentaje);

            dgv = new DataGridView
            {
                Location            = new Point(20, 98),
                Size                = new Size(865, 455),
                BackgroundColor     = Color.White,
                BorderStyle         = BorderStyle.FixedSingle,
                RowHeadersVisible   = false,
                AllowUserToAddRows  = false,
                ReadOnly            = true,
                SelectionMode       = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font                = new Font("Segoe UI", 9.5F),
                GridColor           = Color.FromArgb(225, 225, 225),
                ColumnHeadersHeight = 34,
                RowTemplate         = { Height = 32 },
                CellBorderStyle     = DataGridViewCellBorderStyle.SingleHorizontal
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = NARANJA;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            Controls.Add(dgv);
        }

        private void CargarHistorial()
        {
            var dt = _servicio.ListarPorRango(dtpDesde.Value, dtpHasta.Value);
            dgv.DataSource = dt;
            if (dgv.Columns.Contains("HistorialID"))
                dgv.Columns["HistorialID"].Visible = false;

            foreach (DataGridViewRow r in dgv.Rows)
            {
                string est = r.Cells["Estado"].Value?.ToString() ?? "";
                if      (est.Contains("Tomado"))   r.DefaultCellStyle.BackColor = Color.FromArgb(220, 255, 220);
                else if (est.Contains("Omitido"))  r.DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
                else if (est.Contains("Tardio"))   r.DefaultCellStyle.BackColor = Color.FromArgb(255, 252, 200);
            }

            var (tomados, total, pct) = _servicio.CalcularCumplimiento(dt);
            if (total > 0)
            {
                lblPorcentaje.Text      = $"Cumplimiento: {pct:F0}%  ({tomados}/{total})";
                lblPorcentaje.ForeColor = pct >= 80
                    ? Color.FromArgb(20, 130, 60)
                    : pct >= 50
                        ? Color.FromArgb(200, 140, 0)
                        : Color.FromArgb(190, 40, 40);
            }
            else
            {
                lblPorcentaje.Text      = "Sin registros en este período";
                lblPorcentaje.ForeColor = Color.Gray;
            }
        }
    }
}

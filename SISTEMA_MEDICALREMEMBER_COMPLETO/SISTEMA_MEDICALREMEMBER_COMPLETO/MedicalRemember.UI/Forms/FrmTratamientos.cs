using System;
using System.Drawing;
using System.Windows.Forms;
using MedicalRemember.Business;
using MedicalRemember.Models;

namespace MedicalRemember.Forms
{
    public class FrmTratamientos : Form
    {
        private DataGridView dgv;
        private ComboBox cmbPaciente, cmbMedicamento;
        private TextBox  txtDosis, txtInstrucciones;
        private DateTimePicker dtpInicio, dtpFin;
        private CheckBox chkSinFin;
        private Button   btnGuardar, btnEliminar;
        private int      idActual = 0;

        private readonly TratamientoServicio _servicio = new TratamientoServicio();

        private readonly Color MORADO = Color.FromArgb(100, 50, 180);
        private readonly Color VERDE  = Color.FromArgb(30, 130, 60);
        private readonly Color ROJO   = Color.FromArgb(190, 40, 40);

        public FrmTratamientos()
        {
            Construir();
            CargarCombos();
            CargarGrid();
        }

        private void Construir()
        {
            Text          = "Tratamientos";
            Size          = new Size(1010, 640);
            StartPosition = FormStartPosition.CenterParent;
            BackColor     = Color.White;
            Font          = new Font("Segoe UI", 10F);
            MinimizeBox   = false;

            Controls.Add(new Label
            {
                Text      = "📋  Gestión de Tratamientos",
                Font      = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = MORADO,
                AutoSize  = true,
                Location  = new Point(20, 15)
            });

            dgv = new DataGridView
            {
                Location            = new Point(20, 55),
                Size                = new Size(590, 530),
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
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 245, 255);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 210, 245);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = MORADO;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            dgv.SelectionChanged += Dgv_SelectionChanged;
            Controls.Add(dgv);

            var grp = new GroupBox
            {
                Text      = "Nuevo Tratamiento",
                Location  = new Point(630, 55),
                Size      = new Size(350, 530),
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = MORADO
            };
            Controls.Add(grp);

            int y = 28;

            E(grp, "Paciente *", 10, y); y += 20;
            cmbPaciente = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(10, y), Size = new Size(325, 28) };
            grp.Controls.Add(cmbPaciente); y += 40;

            E(grp, "Medicamento *", 10, y); y += 20;
            cmbMedicamento = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(10, y), Size = new Size(325, 28) };
            grp.Controls.Add(cmbMedicamento); y += 40;

            E(grp, "Dosis * (ej: 500 mg, 1 tableta)", 10, y); y += 20;
            txtDosis = new TextBox { Location = new Point(10, y), Size = new Size(325, 28) };
            grp.Controls.Add(txtDosis); y += 40;

            E(grp, "Fecha de Inicio *", 10, y); y += 20;
            dtpInicio = new DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(10, y), Size = new Size(325, 28), Value = DateTime.Today };
            grp.Controls.Add(dtpInicio); y += 40;

            E(grp, "Fecha de Fin", 10, y); y += 20;
            dtpFin = new DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(10, y), Size = new Size(220, 28), Value = DateTime.Today.AddMonths(1) };
            chkSinFin = new CheckBox { Text = "Sin fin", Location = new Point(238, y + 4), AutoSize = true };
            chkSinFin.CheckedChanged += (s, ev) => dtpFin.Enabled = !chkSinFin.Checked;
            grp.Controls.Add(dtpFin);
            grp.Controls.Add(chkSinFin); y += 40;

            E(grp, "Instrucciones", 10, y); y += 20;
            txtInstrucciones = new TextBox
            {
                Location   = new Point(10, y),
                Size       = new Size(325, 60),
                Multiline  = true,
                ScrollBars = ScrollBars.Vertical
            };
            grp.Controls.Add(txtInstrucciones); y += 72;

            btnGuardar  = BtnG("💾 Guardar",  new Point(10,  y), new Size(150, 42), VERDE, BtnGuardar_Click);
            btnEliminar = BtnG("🗑 Eliminar", new Point(172, y), new Size(150, 42), ROJO,  BtnEliminar_Click);
            grp.Controls.Add(btnGuardar);
            grp.Controls.Add(btnEliminar);
        }

        private void E(Control p, string t, int x, int y)
        { p.Controls.Add(new Label { Text = t, Font = new Font("Segoe UI", 9F), ForeColor = Color.Gray, Location = new Point(x, y), AutoSize = true }); }

        private Button BtnG(string t, Point loc, Size sz, Color c, EventHandler ev)
        {
            var b = new Button { Text = t, Location = loc, Size = sz, BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0; b.Click += ev; return b;
        }

        // ── Cargar combos y grid (delegado a la capa de Negocio) ─
        private void CargarCombos()
        {
            var dtP = _servicio.ListarPacientesParaCombo();
            cmbPaciente.DataSource    = dtP;
            cmbPaciente.DisplayMember = "Nombre";
            cmbPaciente.ValueMember   = "PacienteID";

            var dtM = _servicio.ListarMedicamentosParaCombo();
            cmbMedicamento.DataSource    = dtM;
            cmbMedicamento.DisplayMember = "Nombre";
            cmbMedicamento.ValueMember   = "MedicamentoID";
        }

        private void CargarGrid()
        {
            dgv.DataSource = _servicio.Listar();
            if (dgv.Columns.Contains("TratamientoID"))
                dgv.Columns["TratamientoID"].Visible = false;
        }

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) return;
            idActual = Convert.ToInt32(dgv.SelectedRows[0].Cells["TratamientoID"].Value);
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (cmbPaciente.SelectedValue == null || cmbMedicamento.SelectedValue == null) return;

            var t = new Tratamiento
            {
                PacienteID    = (int)cmbPaciente.SelectedValue,
                MedicamentoID = (int)cmbMedicamento.SelectedValue,
                Dosis         = txtDosis.Text.Trim(),
                FechaInicio   = dtpInicio.Value,
                FechaFin      = chkSinFin.Checked ? (DateTime?)null : dtpFin.Value,
                Instrucciones = txtInstrucciones.Text.Trim()
            };

            ResultadoOperacion resultado = _servicio.Guardar(t);
            if (!resultado.Exito)
            {
                MessageBox.Show(resultado.Mensaje, "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show("✅ " + resultado.Mensaje, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Limpiar();
            CargarGrid();
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (idActual == 0)
            {
                MessageBox.Show("Selecciona un tratamiento.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("¿Eliminar este tratamiento?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            ResultadoOperacion resultado = _servicio.Eliminar(idActual);
            if (!resultado.Exito)
            {
                MessageBox.Show(resultado.Mensaje, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            idActual = 0;
            CargarGrid();
        }

        private void Limpiar()
        {
            idActual = 0;
            txtDosis.Text = "";
            txtInstrucciones.Text = "";
            chkSinFin.Checked = false;
            dtpInicio.Value = DateTime.Today;
            dtpFin.Value = DateTime.Today.AddMonths(1);
        }
    }
}

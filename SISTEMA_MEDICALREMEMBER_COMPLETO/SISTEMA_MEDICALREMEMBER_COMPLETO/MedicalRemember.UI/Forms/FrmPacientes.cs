using System;
using System.Drawing;
using System.Windows.Forms;
using MedicalRemember.Business;
using MedicalRemember.Models;

namespace MedicalRemember.Forms
{
    public class FrmPacientes : Form
    {
        private DataGridView dgv;
        private TextBox  txtBuscar, txtNombre, txtApellido, txtTelefono, txtCorreo, txtDireccion;
        private DateTimePicker dtpFecha;
        private ComboBox cmbTipo;
        private Button   btnNuevo, btnGuardar, btnEliminar;
        private Label    lblID;
        private int      idActual = 0;

        private readonly PacienteServicio _servicio = new PacienteServicio();

        private readonly Color VERDE = Color.FromArgb(30, 130, 60);
        private readonly Color ROJO  = Color.FromArgb(190, 40, 40);
        private readonly Color AZUL  = Color.FromArgb(25, 80, 150);

        public FrmPacientes()
        {
            Construir();
            CargarGrid();
        }

        private void Construir()
        {
            Text          = "Pacientes";
            Size          = new Size(980, 640);
            StartPosition = FormStartPosition.CenterParent;
            BackColor     = Color.White;
            Font          = new Font("Segoe UI", 10F);
            MinimizeBox   = false;

            Controls.Add(Lbl("👤  Gestión de Pacientes",
                new Font("Segoe UI", 15F, FontStyle.Bold), AZUL,
                new Point(20, 15), true));

            txtBuscar = new TextBox
            {
                Location = new Point(20, 55),
                Size     = new Size(290, 28),
                Font     = new Font("Segoe UI", 10F)
            };
            txtBuscar.TextChanged += (s, e) => CargarGrid(txtBuscar.Text);
            Controls.Add(Lbl("Buscar:", new Font("Segoe UI", 9F), Color.Gray,
                new Point(20, 38), true));
            Controls.Add(txtBuscar);

            btnNuevo = Btn("➕ Nuevo", new Point(330, 53), new Size(130, 30),
                AZUL, BtnNuevo_Click);
            Controls.Add(btnNuevo);

            dgv = new DataGridView
            {
                Location              = new Point(20, 90),
                Size                  = new Size(560, 490),
                BackgroundColor       = Color.White,
                BorderStyle           = BorderStyle.FixedSingle,
                RowHeadersVisible     = false,
                AllowUserToAddRows    = false,
                ReadOnly              = true,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                Font                  = new Font("Segoe UI", 9.5F),
                GridColor             = Color.FromArgb(225, 225, 225),
                MultiSelect           = false,
                ColumnHeadersHeight   = 34,
                RowTemplate           = { Height = 32 },
                CellBorderStyle       = DataGridViewCellBorderStyle.SingleHorizontal
            };
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 252, 255);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(210, 225, 245);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = AZUL;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            dgv.SelectionChanged += Dgv_SelectionChanged;
            Controls.Add(dgv);

            var grp = new GroupBox
            {
                Text      = "Datos del Paciente",
                Location  = new Point(600, 90),
                Size      = new Size(355, 490),
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = AZUL
            };
            Controls.Add(grp);

            lblID = new Label { Text = "0", Visible = false };
            grp.Controls.Add(lblID);

            int y = 28;
            AgregarCampo(grp, "Nombre *",  ref y, out txtNombre);
            AgregarCampo(grp, "Apellido *", ref y, out txtApellido);

            grp.Controls.Add(Lbl("Fecha de Nacimiento *", 10, y, Color.Gray));
            y += 20;
            dtpFecha = new DateTimePicker
            {
                Format   = DateTimePickerFormat.Short,
                Location = new Point(10, y),
                Size     = new Size(330, 28),
                MaxDate  = DateTime.Today,
                Value    = DateTime.Today.AddYears(-30)
            };
            grp.Controls.Add(dtpFecha);
            y += 38;

            AgregarCampo(grp, "Teléfono",  ref y, out txtTelefono);
            AgregarCampo(grp, "Correo",    ref y, out txtCorreo);
            AgregarCampo(grp, "Dirección", ref y, out txtDireccion);

            grp.Controls.Add(Lbl("Tipo de Paciente", 10, y, Color.Gray));
            y += 20;
            cmbTipo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location      = new Point(10, y),
                Size          = new Size(330, 28)
            };
            cmbTipo.Items.AddRange(new[] { "Adulto Mayor", "Paciente Crónico", "Otro" });
            cmbTipo.SelectedIndex = 2;
            grp.Controls.Add(cmbTipo);
            y += 44;

            btnGuardar  = Btn("💾 Guardar",  new Point(10,  y), new Size(152, 40), VERDE, BtnGuardar_Click);
            btnEliminar = Btn("🗑 Eliminar", new Point(175, y), new Size(152, 40), ROJO,  BtnEliminar_Click);
            grp.Controls.Add(btnGuardar);
            grp.Controls.Add(btnEliminar);
        }

        private void AgregarCampo(Control parent, string titulo, ref int y, out TextBox txt)
        {
            parent.Controls.Add(Lbl(titulo, 10, y, Color.Gray));
            y += 20;
            txt = new TextBox { Location = new Point(10, y), Size = new Size(330, 28) };
            parent.Controls.Add(txt);
            y += 38;
        }

        private Label Lbl(string texto, Font fuente, Color color, Point loc, bool autosize)
            => new Label { Text = texto, Font = fuente, ForeColor = color, Location = loc, AutoSize = autosize };

        private Label Lbl(string texto, int x, int y, Color color)
            => new Label { Text = texto, Font = new Font("Segoe UI", 9F), ForeColor = color, Location = new Point(x, y), AutoSize = true };

        private Button Btn(string texto, Point loc, Size sz, Color color, EventHandler click)
        {
            var b = new Button
            {
                Text      = texto,
                Location  = loc,
                Size      = sz,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += click;
            return b;
        }

        // ── Cargar grid (delegado a la capa de Negocio) ─────────
        private void CargarGrid(string filtro = "")
        {
            dgv.DataSource = _servicio.Listar(filtro);
            if (dgv.Columns.Contains("PacienteID"))
                dgv.Columns["PacienteID"].Visible = false;
        }

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) return;
            idActual = Convert.ToInt32(dgv.SelectedRows[0].Cells["PacienteID"].Value);

            Paciente p = _servicio.ObtenerPorId(idActual);
            if (p == null) return;

            txtNombre.Text    = p.Nombre;
            txtApellido.Text  = p.Apellido;
            txtTelefono.Text  = p.Telefono;
            txtCorreo.Text    = p.Correo;
            txtDireccion.Text = p.Direccion;
            dtpFecha.Value    = p.FechaNacimiento;
            cmbTipo.SelectedIndex = p.TipoPaciente >= 1 && p.TipoPaciente <= 3 ? p.TipoPaciente - 1 : 2;
        }

        // ── Guardar (validación en la capa de Negocio) ──────────
        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            var p = new Paciente
            {
                PacienteID      = idActual,
                Nombre          = txtNombre.Text.Trim(),
                Apellido        = txtApellido.Text.Trim(),
                FechaNacimiento = dtpFecha.Value,
                Telefono        = txtTelefono.Text.Trim(),
                Correo          = txtCorreo.Text.Trim(),
                Direccion       = txtDireccion.Text.Trim(),
                TipoPaciente    = cmbTipo.SelectedIndex + 1
            };

            ResultadoOperacion resultado = _servicio.Guardar(p);

            if (!resultado.Exito)
            {
                MessageBox.Show(resultado.Mensaje, "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show("✅ " + resultado.Mensaje, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Limpiar();
            CargarGrid();
        }

        // ── Eliminar ─────────────────────────────────────────────
        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (idActual == 0)
            {
                MessageBox.Show("Selecciona un paciente.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("¿Eliminar este paciente?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            ResultadoOperacion resultado = _servicio.Eliminar(idActual);
            if (!resultado.Exito)
            {
                MessageBox.Show(resultado.Mensaje, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Limpiar();
            CargarGrid();
        }

        private void BtnNuevo_Click(object sender, EventArgs e) => Limpiar();

        private void Limpiar()
        {
            idActual          = 0;
            txtNombre.Text    = "";
            txtApellido.Text  = "";
            txtTelefono.Text  = "";
            txtCorreo.Text    = "";
            txtDireccion.Text = "";
            dtpFecha.Value    = DateTime.Today.AddYears(-30);
            cmbTipo.SelectedIndex = 2;
        }
    }
}

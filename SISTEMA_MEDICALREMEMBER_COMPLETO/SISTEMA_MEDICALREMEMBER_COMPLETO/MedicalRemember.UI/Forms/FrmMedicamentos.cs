using System;
using System.Drawing;
using System.Windows.Forms;
using MedicalRemember.Business;
using MedicalRemember.Models;

namespace MedicalRemember.Forms
{
    public class FrmMedicamentos : Form
    {
        private DataGridView dgv;
        private TextBox  txtNombre, txtDescripcion;
        private ComboBox cmbUnidad, cmbVia;
        private Button   btnGuardar, btnEliminar, btnNuevo;
        private int      idActual = 0;

        private readonly MedicamentoServicio _servicio = new MedicamentoServicio();

        private readonly Color VERDE = Color.FromArgb(30, 130, 60);
        private readonly Color ROJO  = Color.FromArgb(190, 40, 40);
        private readonly Color TEAL  = Color.FromArgb(0, 130, 110);

        public FrmMedicamentos()
        {
            Construir();
            CargarGrid();
        }

        private void Construir()
        {
            Text          = "Medicamentos";
            Size          = new Size(900, 620);
            StartPosition = FormStartPosition.CenterParent;
            BackColor     = Color.White;
            Font          = new Font("Segoe UI", 10F);
            MinimizeBox   = false;

            Controls.Add(new Label
            {
                Text      = "💊  Gestión de Medicamentos",
                Font      = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = TEAL,
                AutoSize  = true,
                Location  = new Point(20, 15)
            });

            dgv = new DataGridView
            {
                Location            = new Point(20, 55),
                Size                = new Size(530, 500),
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
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 252, 250);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 240, 235);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = TEAL;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            dgv.SelectionChanged += Dgv_SelectionChanged;
            Controls.Add(dgv);

            var bNuevo = new Button
            {
                Text      = "➕ Nuevo",
                Location  = new Point(560, 55),
                Size      = new Size(130, 32),
                BackColor = TEAL,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            bNuevo.FlatAppearance.BorderSize = 0;
            bNuevo.Click += (s, e) => Limpiar();
            Controls.Add(bNuevo);

            var grp = new GroupBox
            {
                Text      = "Datos del Medicamento",
                Location  = new Point(560, 95),
                Size      = new Size(310, 460),
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TEAL
            };
            Controls.Add(grp);

            int y = 28;
            Etiqueta(grp, "Nombre *", 10, y); y += 20;
            txtNombre = new TextBox { Location = new Point(10, y), Size = new Size(285, 28) };
            grp.Controls.Add(txtNombre); y += 40;

            Etiqueta(grp, "Descripción  (para qué sirve)", 10, y); y += 20;
            txtDescripcion = new TextBox
            {
                Location  = new Point(10, y),
                Size      = new Size(285, 70),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            grp.Controls.Add(txtDescripcion); y += 82;

            Etiqueta(grp, "Unidad de medida", 10, y); y += 20;
            cmbUnidad = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location      = new Point(10, y),
                Size          = new Size(285, 28)
            };
            cmbUnidad.Items.AddRange(new[] { "mg", "ml", "tableta", "cápsula", "gotas", "UI", "g" });
            cmbUnidad.SelectedIndex = 0;
            grp.Controls.Add(cmbUnidad); y += 50;

            Etiqueta(grp, "Vía de administración", 10, y); y += 20;
            cmbVia = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location      = new Point(10, y),
                Size          = new Size(285, 28)
            };
            cmbVia.Items.AddRange(new[] { "Oral", "Inyectable", "Sublingual", "Tópica", "Otra" });
            cmbVia.SelectedIndex = 0;
            grp.Controls.Add(cmbVia); y += 50;

            btnGuardar  = BtnControl("💾 Guardar",  new Point(10,  y), new Size(130, 40), VERDE, BtnGuardar_Click);
            btnEliminar = BtnControl("🗑 Eliminar", new Point(155, y), new Size(130, 40), ROJO,  BtnEliminar_Click);
            grp.Controls.Add(btnGuardar);
            grp.Controls.Add(btnEliminar);
        }

        private void Etiqueta(Control p, string t, int x, int y)
        {
            p.Controls.Add(new Label { Text = t, Font = new Font("Segoe UI", 9F), ForeColor = Color.Gray, Location = new Point(x, y), AutoSize = true });
        }

        private Button BtnControl(string t, Point loc, Size sz, Color c, EventHandler ev)
        {
            var b = new Button { Text = t, Location = loc, Size = sz, BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0;
            b.Click += ev;
            return b;
        }

        // ── Cargar grid (delegado a la capa de Negocio) ─────────
        private void CargarGrid()
        {
            dgv.DataSource = _servicio.Listar();
            if (dgv.Columns.Contains("MedicamentoID"))
                dgv.Columns["MedicamentoID"].Visible = false;
        }

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) return;
            idActual = Convert.ToInt32(dgv.SelectedRows[0].Cells["MedicamentoID"].Value);

            Medicamento m = _servicio.ObtenerPorId(idActual);
            if (m == null) return;

            txtNombre.Text      = m.Nombre;
            txtDescripcion.Text = m.Descripcion;
            cmbUnidad.Text      = m.Unidad;
            cmbVia.Text         = m.ViaAdministracion;
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            var m = new Medicamento
            {
                MedicamentoID     = idActual,
                Nombre            = txtNombre.Text.Trim(),
                Descripcion       = txtDescripcion.Text.Trim(),
                Unidad            = cmbUnidad.Text,
                ViaAdministracion = cmbVia.Text
            };

            ResultadoOperacion resultado = _servicio.Guardar(m);
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
                MessageBox.Show("Selecciona un medicamento.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("¿Eliminar este medicamento?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            ResultadoOperacion resultado = _servicio.Eliminar(idActual);
            if (!resultado.Exito)
            {
                MessageBox.Show(resultado.Mensaje, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Limpiar();
            CargarGrid();
        }

        private void Limpiar()
        {
            idActual = 0;
            txtNombre.Text = "";
            txtDescripcion.Text = "";
            cmbUnidad.SelectedIndex = 0;
            cmbVia.SelectedIndex = 0;
        }
    }
}

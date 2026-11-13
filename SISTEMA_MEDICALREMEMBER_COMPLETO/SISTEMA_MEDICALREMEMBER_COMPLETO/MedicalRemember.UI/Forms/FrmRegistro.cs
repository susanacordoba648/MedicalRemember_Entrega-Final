using System;
using System.Drawing;
using System.Windows.Forms;
using MedicalRemember.Business;
using MedicalRemember.Models;

namespace MedicalRemember.Forms
{
    public class FrmRegistro : Form
    {
        private TextBox txtNombreCompleto;
        private TextBox txtUsuario;
        private TextBox txtContrasena;
        private TextBox txtConfirmar;
        private TextBox txtMejorAmigo;
        private TextBox txtColorFavorito;
        private ComboBox cmbRol;
        private CheckBox chkVerContrasena;
        private Label lblError;
        private Button btnRegistrar;
        private Button btnCancelar;

        private readonly UsuarioServicio _servicio = new UsuarioServicio();

        public string UsuarioRegistrado { get; private set; }

        private readonly Color VERDE = Color.FromArgb(30, 130, 60);
        private readonly Color VERDE_OSC = Color.FromArgb(20, 100, 45);
        private readonly Color ROJO = Color.FromArgb(190, 40, 40);
        private readonly Color NARANJA = Color.FromArgb(180, 100, 0);

        public FrmRegistro()
        {
            Construir();
        }

        private void Construir()
        {
            Text = "Medical Remember - Crear Cuenta";
            Size = new Size(430, 780);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10F);
            AutoScroll = true;

            // Panel superior verde
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 95, BackColor = VERDE };
            pnlTop.Controls.Add(new Label
            {
                Text = "👤  Crear Nueva Cuenta",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(60, 18)
            });
            pnlTop.Controls.Add(new Label
            {
                Text = "Completa todos los campos para registrarte",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(210, 245, 210),
                AutoSize = true,
                Location = new Point(85, 55)
            });
            Controls.Add(pnlTop);

            // Panel formulario
            var pnlForm = new Panel
            {
                Location = new Point(40, 108),
                Size = new Size(345, 640),
                AutoScroll = false
            };

            int y = 0;

            pnlForm.Controls.Add(Etiqueta("Nombre Completo *", y)); y += 20;
            txtNombreCompleto = Campo(y); pnlForm.Controls.Add(txtNombreCompleto); y += 42;

            pnlForm.Controls.Add(Etiqueta("Nombre de Usuario *  (sin espacios)", y)); y += 20;
            txtUsuario = Campo(y); pnlForm.Controls.Add(txtUsuario); y += 42;

            pnlForm.Controls.Add(Etiqueta("Contrasena *  (minimo 4 caracteres)", y)); y += 20;
            txtContrasena = Campo(y);
            txtContrasena.UseSystemPasswordChar = true;
            pnlForm.Controls.Add(txtContrasena); y += 42;

            pnlForm.Controls.Add(Etiqueta("Confirmar Contrasena *", y)); y += 20;
            txtConfirmar = Campo(y);
            txtConfirmar.UseSystemPasswordChar = true;
            pnlForm.Controls.Add(txtConfirmar); y += 42;

            chkVerContrasena = new CheckBox
            {
                Text = "Mostrar contrasenas",
                Location = new Point(0, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Gray
            };
            chkVerContrasena.CheckedChanged += (s, e) =>
            {
                txtContrasena.UseSystemPasswordChar = !chkVerContrasena.Checked;
                txtConfirmar.UseSystemPasswordChar = !chkVerContrasena.Checked;
            };
            pnlForm.Controls.Add(chkVerContrasena); y += 36;

            pnlForm.Controls.Add(Etiqueta("Tipo de usuario", y)); y += 20;
            cmbRol = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(0, y),
                Size = new Size(345, 28),
                Font = new Font("Segoe UI", 10.5F)
            };
            cmbRol.Items.AddRange(new[] { "Cuidador", "Administrador" });
            cmbRol.SelectedIndex = 0;
            pnlForm.Controls.Add(cmbRol); y += 48;

            var pnlSep = new Panel
            {
                Location = new Point(0, y),
                Size = new Size(345, 32),
                BackColor = Color.FromArgb(255, 248, 220)
            };
            pnlSep.Controls.Add(new Label
            {
                Text = "🔐  Preguntas de Seguridad (para recuperar tu cuenta)",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = NARANJA,
                AutoSize = true,
                Location = new Point(6, 8)
            });
            pnlForm.Controls.Add(pnlSep); y += 38;

            pnlForm.Controls.Add(Etiqueta("¿Cuál es el nombre de tu mejor amigo? *", y)); y += 20;
            txtMejorAmigo = Campo(y); pnlForm.Controls.Add(txtMejorAmigo); y += 42;

            pnlForm.Controls.Add(Etiqueta("¿Cuál es tu color favorito? *", y)); y += 20;
            txtColorFavorito = Campo(y); pnlForm.Controls.Add(txtColorFavorito); y += 48;

            lblError = new Label
            {
                Text = "",
                ForeColor = ROJO,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                AutoSize = false,
                Size = new Size(345, 22),
                Location = new Point(0, y),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlForm.Controls.Add(lblError); y += 28;

            btnRegistrar = new Button
            {
                Text = "✅  Crear Cuenta",
                Location = new Point(0, y),
                Size = new Size(345, 44),
                BackColor = VERDE,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRegistrar.FlatAppearance.BorderSize = 0;
            btnRegistrar.FlatAppearance.MouseOverBackColor = VERDE_OSC;
            btnRegistrar.Click += BtnRegistrar_Click;
            pnlForm.Controls.Add(btnRegistrar); y += 52;

            btnCancelar = new Button
            {
                Text = "Cancelar",
                Location = new Point(0, y),
                Size = new Size(345, 34),
                BackColor = Color.White,
                ForeColor = Color.Gray,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F),
                Cursor = Cursors.Hand
            };
            btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
            btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            pnlForm.Controls.Add(btnCancelar);

            Controls.Add(pnlForm);
            AcceptButton = btnRegistrar;
        }

        private Label Etiqueta(string texto, int y) => new Label
        {
            Text = texto,
            Font = new Font("Segoe UI", 9F),
            ForeColor = Color.Gray,
            AutoSize = true,
            Location = new Point(0, y)
        };

        private TextBox Campo(int y) => new TextBox
        {
            Location = new Point(0, y),
            Size = new Size(345, 28),
            Font = new Font("Segoe UI", 10.5F)
        };

        // ── Registrar (delega validación y guardado en la capa de Negocio) ──
        private void BtnRegistrar_Click(object sender, EventArgs e)
        {
            lblError.ForeColor = ROJO;
            lblError.Text = "";

            var nuevo = new Usuario
            {
                NombreCompleto      = txtNombreCompleto.Text.Trim(),
                NombreUsuario       = txtUsuario.Text.Trim(),
                Rol                 = cmbRol.SelectedItem.ToString(),
                RespuestaMejorAmigo = txtMejorAmigo.Text,
                RespuestaColorFav   = txtColorFavorito.Text
            };

            ResultadoOperacion resultado;
            try
            {
                resultado = _servicio.Registrar(nuevo, txtContrasena.Text, txtConfirmar.Text);
            }
            catch (Exception ex)
            {
                lblError.Text = "❌ Error de conexión: " + ex.Message;
                return;
            }

            if (!resultado.Exito)
            {
                lblError.Text = "⚠ " + resultado.Mensaje;
                return;
            }

            UsuarioRegistrado = nuevo.NombreUsuario;
            MessageBox.Show(
                "✅ Cuenta creada exitosamente.\n\n" +
                "Usuario: " + nuevo.NombreUsuario + "\n\n" +
                "Ya puedes iniciar sesion con tu cuenta.",
                "Registro Exitoso",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}

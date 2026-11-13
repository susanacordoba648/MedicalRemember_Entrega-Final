using System;
using System.Drawing;
using System.Windows.Forms;
using MedicalRemember.Business;
using MedicalRemember.Models;

namespace MedicalRemember.Forms
{
    public class FrmLogin : Form
    {
        private TextBox txtUsuario;
        private TextBox txtContrasena;
        private Label lblError;
        private Label lblOlvidaste;       // Aparece tras 2 intentos fallidos
        private Button btnIngresar;
        private Button btnRegistrarse;
        private Button btnSalir;
        private CheckBox chkVerContrasena;

        private int intentosFallidos = 0;   // Contador de intentos

        // Capa de Negocio
        private readonly UsuarioServicio _servicio = new UsuarioServicio();

        public string NombreCompletoUsuario { get; private set; }
        public string RolUsuario { get; private set; }
        public int UsuarioID { get; private set; }

        private readonly Color AZUL = Color.FromArgb(25, 80, 150);
        private readonly Color AZUL_OSC = Color.FromArgb(15, 55, 110);
        private readonly Color VERDE = Color.FromArgb(30, 130, 60);
        private readonly Color ROJO = Color.FromArgb(190, 40, 40);
        private readonly Color NARANJA = Color.FromArgb(180, 100, 0);

        public FrmLogin()
        {
            Construir();
        }

        private void Construir()
        {
            Text = "Medical Remember - Iniciar Sesion";
            Size = new Size(420, 610);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10F);

            // Panel superior azul
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 160, BackColor = AZUL };
            pnlTop.Controls.Add(new Label
            {
                Text = "💊",
                Font = new Font("Segoe UI", 38F),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(168, 14)
            });
            pnlTop.Controls.Add(new Label
            {
                Text = "Medical Remember",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(95, 92)
            });
            pnlTop.Controls.Add(new Label
            {
                Text = "Sistema de Organizacion de Medicacion",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(200, 225, 255),
                AutoSize = true,
                Location = new Point(68, 124)
            });
            Controls.Add(pnlTop);

            // Panel formulario
            var pnlForm = new Panel { Location = new Point(40, 178), Size = new Size(335, 400) };

            // Campo Usuario
            pnlForm.Controls.Add(Etq("Usuario", 0));
            txtUsuario = new TextBox
            {
                Location = new Point(0, 20),
                Size = new Size(335, 28),
                Font = new Font("Segoe UI", 11F)
            };
            pnlForm.Controls.Add(txtUsuario);

            // Campo Contrasena
            pnlForm.Controls.Add(Etq("Contrasena", 62));
            txtContrasena = new TextBox
            {
                Location = new Point(0, 82),
                Size = new Size(335, 28),
                Font = new Font("Segoe UI", 11F),
                UseSystemPasswordChar = true
            };
            txtContrasena.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnIngresar_Click(s, e); };
            pnlForm.Controls.Add(txtContrasena);

            // Mostrar contrasena
            chkVerContrasena = new CheckBox
            {
                Text = "Mostrar contrasena",
                Location = new Point(0, 118),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Gray
            };
            chkVerContrasena.CheckedChanged += (s, e) =>
                txtContrasena.UseSystemPasswordChar = !chkVerContrasena.Checked;
            pnlForm.Controls.Add(chkVerContrasena);

            // Label de error
            lblError = new Label
            {
                Text = "",
                ForeColor = ROJO,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                AutoSize = false,
                Size = new Size(335, 22),
                Location = new Point(0, 148),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlForm.Controls.Add(lblError);

            // Link "¿Olvidaste tu contrasena?" — oculto hasta 2 intentos fallidos
            lblOlvidaste = new Label
            {
                Text = "¿Olvidaste tu contrasena? Haz clic aqui para recuperarla",
                ForeColor = NARANJA,
                Font = new Font("Segoe UI", 9F, FontStyle.Underline | FontStyle.Bold),
                AutoSize = false,
                Size = new Size(335, 20),
                Location = new Point(0, 174),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                Visible = false    // Se hace visible despues de 2 intentos
            };
            lblOlvidaste.Click += LblOlvidaste_Click;
            pnlForm.Controls.Add(lblOlvidaste);

            // Boton Ingresar
            btnIngresar = new Button
            {
                Text = "Ingresar",
                Location = new Point(0, 200),
                Size = new Size(335, 44),
                BackColor = AZUL,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnIngresar.FlatAppearance.BorderSize = 0;
            btnIngresar.FlatAppearance.MouseOverBackColor = AZUL_OSC;
            btnIngresar.Click += BtnIngresar_Click;
            pnlForm.Controls.Add(btnIngresar);

            // Boton Registrarse
            btnRegistrarse = new Button
            {
                Text = "¿No tienes cuenta?  Registrarse",
                Location = new Point(0, 252),
                Size = new Size(335, 38),
                BackColor = Color.White,
                ForeColor = VERDE,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRegistrarse.FlatAppearance.BorderColor = VERDE;
            btnRegistrarse.FlatAppearance.BorderSize = 1;
            btnRegistrarse.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 255, 245);
            btnRegistrarse.Click += BtnRegistrarse_Click;
            pnlForm.Controls.Add(btnRegistrarse);

            // Boton Salir (cierra completamente la aplicacion desde el login)
            btnSalir = new Button
            {
                Text = "🚪  Salir del programa",
                Location = new Point(0, 298),
                Size = new Size(335, 34),
                BackColor = Color.White,
                ForeColor = Color.Gray,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F),
                Cursor = Cursors.Hand
            };
            btnSalir.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
            btnSalir.Click += BtnSalir_Click;
            pnlForm.Controls.Add(btnSalir);

            Controls.Add(pnlForm);
            AcceptButton = btnIngresar;
        }

        private Label Etq(string texto, int y) =>
            new Label
            {
                Text = texto,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(0, y)
            };

        // ── Iniciar sesion (delega en la capa de Negocio) ───────
        private void BtnIngresar_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            string usuario = txtUsuario.Text.Trim();
            string clave = txtContrasena.Text;

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(clave))
            {
                lblError.Text = "⚠ Ingresa usuario y contrasena.";
                return;
            }

            Usuario u;
            try
            {
                u = _servicio.IniciarSesion(usuario, clave);
            }
            catch (Exception ex)
            {
                lblError.Text = "❌ Error de conexión: " + ex.Message;
                return;
            }

            if (u == null)
            {
                intentosFallidos++;
                txtContrasena.Text = "";
                txtContrasena.Focus();

                if (intentosFallidos >= 2)
                {
                    lblError.Text = "❌ Contrasena incorrecta. (" + intentosFallidos + " intentos)";
                    lblOlvidaste.Visible = true;
                }
                else
                {
                    lblError.Text = "❌ Usuario o contrasena incorrectos.  Intento " + intentosFallidos + " de 2.";
                }
                return;
            }

            // Login correcto — reiniciar contador
            intentosFallidos = 0;
            lblOlvidaste.Visible = false;
            UsuarioID = u.UsuarioID;
            NombreCompletoUsuario = u.NombreCompleto;
            RolUsuario = u.Rol;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // ── Abrir recuperacion de contrasena ────────────────────
        private void LblOlvidaste_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmRecuperarContrasena())
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    intentosFallidos = 0;
                    lblOlvidaste.Visible = false;
                    txtContrasena.Text = "";
                    lblError.ForeColor = VERDE;
                    lblError.Text = "✅ Contrasena cambiada. Ya puedes iniciar sesion.";
                    txtContrasena.Focus();
                }
            }
        }

        // ── Abrir registro ──────────────────────────────────────
        private void BtnRegistrarse_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmRegistro())
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    txtUsuario.Text = frm.UsuarioRegistrado;
                    txtContrasena.Text = "";
                    intentosFallidos = 0;
                    lblOlvidaste.Visible = false;
                    lblError.ForeColor = VERDE;
                    lblError.Text = "✅ Cuenta creada. Ya puedes iniciar sesion.";
                    txtContrasena.Focus();
                }
            }
        }

        // ── Salir del programa desde la pantalla de Login ───────
        private void BtnSalir_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Deseas salir de Medical Remember?", "Confirmar salida",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.DialogResult = DialogResult.Cancel;
                Application.Exit();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // FrmLogin
            // 
            this.ClientSize = new System.Drawing.Size(278, 244);
            this.Name = "FrmLogin";
            this.Load += new System.EventHandler(this.FrmLogin_Load);
            this.ResumeLayout(false);

        }

        private void FrmLogin_Load(object sender, EventArgs e)
        {

        }
    }
}

using System;
using System.Drawing;
using System.Windows.Forms;
using MedicalRemember.Business;
using MedicalRemember.Models;

namespace MedicalRemember.Forms
{
    public class FrmRecuperarContrasena : Form
    {
        // Paso 1: buscar usuario
        private Panel    pnlPaso1;
        private TextBox  txtUsuarioBuscar;
        private Button   btnBuscar;
        private Label    lblErrorPaso1;

        // Paso 2: responder preguntas
        private Panel    pnlPaso2;
        private Label    lblNombreUsuario;
        private TextBox  txtRespAmigo;
        private TextBox  txtRespColor;
        private Button   btnVerificar;
        private Label    lblErrorPaso2;

        // Paso 3: nueva contrasena
        private Panel    pnlPaso3;
        private TextBox  txtNuevaClave;
        private TextBox  txtConfirmarClave;
        private CheckBox chkVerClave;
        private Button   btnCambiar;
        private Label    lblErrorPaso3;

        // Datos internos
        private Usuario usuarioEncontrado;

        private readonly UsuarioServicio _servicio = new UsuarioServicio();

        private readonly Color AZUL    = Color.FromArgb(25, 80, 150);
        private readonly Color NARANJA = Color.FromArgb(180, 100, 0);
        private readonly Color VERDE   = Color.FromArgb(30, 130, 60);
        private readonly Color ROJO    = Color.FromArgb(190, 40, 40);

        public FrmRecuperarContrasena()
        {
            Construir();
        }

        private void Construir()
        {
            Text            = "Medical Remember - Recuperar Contrasena";
            Size            = new Size(430, 520);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            MinimizeBox     = false;
            BackColor       = Color.White;
            Font            = new Font("Segoe UI", 10F);

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = NARANJA };
            pnlTop.Controls.Add(new Label
            {
                Text = "🔐  Recuperar Contrasena",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true, Location = new Point(65, 16)
            });
            pnlTop.Controls.Add(new Label
            {
                Text = "Verifica tu identidad para restablecer tu contrasena",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(255, 235, 200),
                AutoSize = true, Location = new Point(55, 54)
            });
            Controls.Add(pnlTop);

            ConstruirPaso1();
            ConstruirPaso2();
            ConstruirPaso3();

            MostrarPaso(1);
        }

        // ── PASO 1: Buscar usuario ──────────────────────────────
        private void ConstruirPaso1()
        {
            pnlPaso1 = new Panel { Location = new Point(40, 108), Size = new Size(345, 200) };

            pnlPaso1.Controls.Add(new Label
            {
                Text = "PASO 1 de 3  —  Ingresa tu nombre de usuario",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = NARANJA, AutoSize = true, Location = new Point(0, 0)
            });

            pnlPaso1.Controls.Add(new Label
            {
                Text = "Nombre de Usuario:",
                Font = new Font("Segoe UI", 9F), ForeColor = Color.Gray,
                AutoSize = true, Location = new Point(0, 38)
            });

            txtUsuarioBuscar = new TextBox
            {
                Location = new Point(0, 58), Size = new Size(345, 28),
                Font = new Font("Segoe UI", 11F)
            };
            txtUsuarioBuscar.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnBuscar_Click(s, e); };
            pnlPaso1.Controls.Add(txtUsuarioBuscar);

            lblErrorPaso1 = new Label
            {
                Text = "", ForeColor = ROJO,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                AutoSize = false, Size = new Size(345, 22),
                Location = new Point(0, 96), TextAlign = ContentAlignment.MiddleCenter
            };
            pnlPaso1.Controls.Add(lblErrorPaso1);

            btnBuscar = new Button
            {
                Text = "Buscar Usuario  →",
                Location = new Point(0, 126), Size = new Size(345, 42),
                BackColor = NARANJA, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBuscar.FlatAppearance.BorderSize = 0;
            btnBuscar.Click += BtnBuscar_Click;
            pnlPaso1.Controls.Add(btnBuscar);

            Controls.Add(pnlPaso1);
        }

        // ── PASO 2: Responder preguntas de seguridad ────────────
        private void ConstruirPaso2()
        {
            pnlPaso2 = new Panel { Location = new Point(40, 108), Size = new Size(345, 310), Visible = false };

            pnlPaso2.Controls.Add(new Label
            {
                Text = "PASO 2 de 3  —  Preguntas de Seguridad",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = NARANJA, AutoSize = true, Location = new Point(0, 0)
            });

            lblNombreUsuario = new Label
            {
                Text = "Usuario: ",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = AZUL, AutoSize = true, Location = new Point(0, 28)
            };
            pnlPaso2.Controls.Add(lblNombreUsuario);

            var pnlAviso = new Panel
            {
                Location  = new Point(0, 52),
                Size      = new Size(345, 28),
                BackColor = Color.FromArgb(255, 248, 220)
            };
            pnlAviso.Controls.Add(new Label
            {
                Text = "⚠  Las respuestas no distinguen mayusculas/minusculas",
                Font = new Font("Segoe UI", 8F),
                ForeColor = NARANJA, AutoSize = true, Location = new Point(5, 7)
            });
            pnlPaso2.Controls.Add(pnlAviso);

            pnlPaso2.Controls.Add(new Label
            {
                Text = "¿Cuál es el nombre de tu mejor amigo?",
                Font = new Font("Segoe UI", 9F), ForeColor = Color.Gray,
                AutoSize = true, Location = new Point(0, 92)
            });
            txtRespAmigo = new TextBox
            {
                Location = new Point(0, 112), Size = new Size(345, 28),
                Font = new Font("Segoe UI", 11F)
            };
            pnlPaso2.Controls.Add(txtRespAmigo);

            pnlPaso2.Controls.Add(new Label
            {
                Text = "¿Cuál es tu color favorito?",
                Font = new Font("Segoe UI", 9F), ForeColor = Color.Gray,
                AutoSize = true, Location = new Point(0, 152)
            });
            txtRespColor = new TextBox
            {
                Location = new Point(0, 172), Size = new Size(345, 28),
                Font = new Font("Segoe UI", 11F)
            };
            pnlPaso2.Controls.Add(txtRespColor);

            lblErrorPaso2 = new Label
            {
                Text = "", ForeColor = ROJO,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                AutoSize = false, Size = new Size(345, 22),
                Location = new Point(0, 210), TextAlign = ContentAlignment.MiddleCenter
            };
            pnlPaso2.Controls.Add(lblErrorPaso2);

            btnVerificar = new Button
            {
                Text = "Verificar Respuestas  →",
                Location = new Point(0, 238), Size = new Size(345, 42),
                BackColor = NARANJA, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnVerificar.FlatAppearance.BorderSize = 0;
            btnVerificar.Click += BtnVerificar_Click;
            pnlPaso2.Controls.Add(btnVerificar);

            Controls.Add(pnlPaso2);
        }

        // ── PASO 3: Nueva contrasena ────────────────────────────
        private void ConstruirPaso3()
        {
            pnlPaso3 = new Panel { Location = new Point(40, 108), Size = new Size(345, 300), Visible = false };

            pnlPaso3.Controls.Add(new Label
            {
                Text = "PASO 3 de 3  —  Nueva Contrasena",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = VERDE, AutoSize = true, Location = new Point(0, 0)
            });

            var pnlOk = new Panel
            {
                Location  = new Point(0, 28),
                Size      = new Size(345, 28),
                BackColor = Color.FromArgb(220, 255, 220)
            };
            pnlOk.Controls.Add(new Label
            {
                Text = "✅  Identidad verificada. Ingresa tu nueva contrasena.",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = VERDE, AutoSize = true, Location = new Point(5, 7)
            });
            pnlPaso3.Controls.Add(pnlOk);

            pnlPaso3.Controls.Add(new Label
            {
                Text = "Nueva Contrasena *  (minimo 4 caracteres)",
                Font = new Font("Segoe UI", 9F), ForeColor = Color.Gray,
                AutoSize = true, Location = new Point(0, 70)
            });
            txtNuevaClave = new TextBox
            {
                Location = new Point(0, 90), Size = new Size(345, 28),
                Font = new Font("Segoe UI", 11F),
                UseSystemPasswordChar = true
            };
            pnlPaso3.Controls.Add(txtNuevaClave);

            pnlPaso3.Controls.Add(new Label
            {
                Text = "Confirmar Nueva Contrasena *",
                Font = new Font("Segoe UI", 9F), ForeColor = Color.Gray,
                AutoSize = true, Location = new Point(0, 130)
            });
            txtConfirmarClave = new TextBox
            {
                Location = new Point(0, 150), Size = new Size(345, 28),
                Font = new Font("Segoe UI", 11F),
                UseSystemPasswordChar = true
            };
            pnlPaso3.Controls.Add(txtConfirmarClave);

            chkVerClave = new CheckBox
            {
                Text = "Mostrar contrasenas", Location = new Point(0, 188),
                AutoSize = true, Font = new Font("Segoe UI", 9F), ForeColor = Color.Gray
            };
            chkVerClave.CheckedChanged += (s, e) =>
            {
                txtNuevaClave.UseSystemPasswordChar    = !chkVerClave.Checked;
                txtConfirmarClave.UseSystemPasswordChar = !chkVerClave.Checked;
            };
            pnlPaso3.Controls.Add(chkVerClave);

            lblErrorPaso3 = new Label
            {
                Text = "", ForeColor = ROJO,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                AutoSize = false, Size = new Size(345, 22),
                Location = new Point(0, 216), TextAlign = ContentAlignment.MiddleCenter
            };
            pnlPaso3.Controls.Add(lblErrorPaso3);

            btnCambiar = new Button
            {
                Text = "🔑  Guardar Nueva Contrasena",
                Location = new Point(0, 244), Size = new Size(345, 44),
                BackColor = VERDE, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCambiar.FlatAppearance.BorderSize = 0;
            btnCambiar.Click += BtnCambiar_Click;
            pnlPaso3.Controls.Add(btnCambiar);

            Controls.Add(pnlPaso3);
        }

        private void MostrarPaso(int paso)
        {
            pnlPaso1.Visible = (paso == 1);
            pnlPaso2.Visible = (paso == 2);
            pnlPaso3.Visible = (paso == 3);
        }

        // ── Click BUSCAR USUARIO ────────────────────────────────
        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            lblErrorPaso1.Text = "";
            string usuario = txtUsuarioBuscar.Text.Trim();

            if (string.IsNullOrWhiteSpace(usuario))
            {
                lblErrorPaso1.Text = "⚠ Ingresa tu nombre de usuario.";
                txtUsuarioBuscar.Focus();
                return;
            }

            usuarioEncontrado = _servicio.BuscarParaRecuperar(usuario);

            if (usuarioEncontrado == null)
            {
                lblErrorPaso1.Text = "❌ No se encontro ese usuario.";
                return;
            }

            lblNombreUsuario.Text = "Usuario: " + usuario + "  (" + usuarioEncontrado.NombreCompleto + ")";

            MostrarPaso(2);
            txtRespAmigo.Focus();
        }

        // ── Click VERIFICAR RESPUESTAS ──────────────────────────
        private void BtnVerificar_Click(object sender, EventArgs e)
        {
            lblErrorPaso2.Text = "";

            if (string.IsNullOrWhiteSpace(txtRespAmigo.Text) || string.IsNullOrWhiteSpace(txtRespColor.Text))
            {
                lblErrorPaso2.Text = "⚠ Responde ambas preguntas.";
                return;
            }

            bool correctas = _servicio.VerificarRespuestas(usuarioEncontrado, txtRespAmigo.Text, txtRespColor.Text);

            if (!correctas)
            {
                lblErrorPaso2.Text = "❌ Las respuestas no son correctas.";
                return;
            }

            MostrarPaso(3);
            txtNuevaClave.Focus();
        }

        // ── Click GUARDAR NUEVA CONTRASENA ──────────────────────
        private void BtnCambiar_Click(object sender, EventArgs e)
        {
            lblErrorPaso3.Text = "";

            ResultadoOperacion resultado = _servicio.CambiarContrasena(
                usuarioEncontrado.UsuarioID, txtNuevaClave.Text, txtConfirmarClave.Text);

            if (!resultado.Exito)
            {
                lblErrorPaso3.Text = "⚠ " + resultado.Mensaje;
                if (txtNuevaClave.Text != txtConfirmarClave.Text) txtConfirmarClave.Text = "";
                return;
            }

            MessageBox.Show(
                "✅ Contrasena restablecida exitosamente.\n\n" +
                "Ya puedes iniciar sesion con tu nueva contrasena.",
                "Contrasena Cambiada",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}

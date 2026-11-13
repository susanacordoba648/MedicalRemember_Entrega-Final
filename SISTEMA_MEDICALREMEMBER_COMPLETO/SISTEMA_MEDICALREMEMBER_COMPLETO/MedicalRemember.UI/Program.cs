using System;
using System.Windows.Forms;
using MedicalRemember.Business;
using MedicalRemember.Forms;

namespace MedicalRemember
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!SistemaServicio.ProbarConexion())
            {
                MessageBox.Show(
                    "No se pudo conectar a SQL Server.\n\n" +
                    "Verifica que:\n" +
                    "1. SQL Server este iniciado\n" +
                    "2. La base de datos MedicalRememberDB exista\n" +
                    "3. La cadena de conexion en App.config sea correcta\n\n" +
                    "Ejecuta primero Database\\MedicalRememberDB.sql en SSMS.",
                    "Medical Remember - Sin conexion",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // ── Mostrar Login primero ──
            using (var login = new FrmLogin())
            {
                if (login.ShowDialog() == DialogResult.OK)
                {
                    // Login correcto → abrir el sistema
                    var frmPrincipal = new FrmPrincipal();
                    frmPrincipal.Text = "Medical Remember  -  Sesión: "
                        + login.NombreCompletoUsuario + " (" + login.RolUsuario + ")";
                    Application.Run(frmPrincipal);
                }
                // Si el usuario cierra el login (o presiona "Salir"), el programa termina aquí.
            }
        }
    }
}

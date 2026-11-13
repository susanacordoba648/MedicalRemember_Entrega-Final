using MedicalRemember.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalRemember.Api.Data
{
    // Puente entre las clases (Models) y las tablas de SQL Server (como en la clase del profe).
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Paciente>              Pacientes     { get; set; }
        public DbSet<Medicamento>           Medicamentos  { get; set; }
        public DbSet<Tratamiento>           Tratamientos  { get; set; }
        public DbSet<Horario>               Horarios      { get; set; }
        public DbSet<HistorialCumplimiento> Historial     { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            // Estas tablas tienen triggers en la base de datos: se lo decimos a EF para que
            // guarde los cambios sin usar la clausula OUTPUT (que SQL Server no permite con triggers).
            mb.Entity<Paciente>().ToTable("Pacientes", t => t.HasTrigger("TR_Pacientes_NoEliminar"));
            mb.Entity<Tratamiento>().ToTable("Tratamientos", t => t.HasTrigger("TR_Tratamientos_ValidarFechas"));
            mb.Entity<HistorialCumplimiento>().ToTable("HistorialCumplimiento", t => t.HasTrigger("TR_Historial_MarcarTardio"));
        }
    }
}

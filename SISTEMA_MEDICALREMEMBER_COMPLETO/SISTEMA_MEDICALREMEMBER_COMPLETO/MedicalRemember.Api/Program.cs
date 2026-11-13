using MedicalRemember.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();                                   // Activa los controladores
builder.Services.AddDbContext<AppDbContext>(opt =>                   // Registra el DbContext (EF Core)
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Conexion")));   // cadena en appsettings.json
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();                                    // Swagger: pagina para probar la API

var app = builder.Build();

app.UseSwagger();                                                    // Swagger siempre disponible (para la defensa)
app.UseSwaggerUI();
app.UseAuthorization();
app.MapControllers();                                                // Conecta las rutas con los controladores
app.Run();

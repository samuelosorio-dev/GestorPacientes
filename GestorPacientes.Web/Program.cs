using FluentValidation;
using GestorPacientes.Application.DTOs.Paciente;
using GestorPacientes.Application.Interfaces;
using GestorPacientes.Application.Mappings;
using GestorPacientes.Application.UseCases;
using GestorPacientes.Application.Validators;
using GestorPacientes.Domain.Interfaces;
using GestorPacientes.Infrastructure.Context;
using GestorPacientes.Infrastructure.Repositories;
using Mapster;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<IPacienteRepository, PacienteRepository>();
builder.Services.AddScoped<IPacienteService, PacienteService>();


var mapsterConfig = TypeAdapterConfig.GlobalSettings;
mapsterConfig.Scan(typeof(PacienteMapsterConfig).Assembly);
builder.Services.AddSingleton(mapsterConfig);


builder.Services.AddScoped<IValidator<CrearPacienteDto>, CrearPacienteValidator>();
builder.Services.AddScoped<IValidator<ActualizarPacienteDto>, ActualizarPacienteValidator>();

// OutputCache
builder.Services.AddOutputCache(opciones =>
{
    opciones.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(3600);
});

// CORS
builder.Services.AddCors(opciones =>
{
    opciones.AddDefaultPolicy(opcionesCORS =>
        opcionesCORS.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.UseOutputCache();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pacientes}/{action=Index}/{id?}");


app.Run();

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
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//INYECCIONES

builder.Services.AddScoped<IPacienteRepository, PacienteRepository>();
builder.Services.AddScoped<IPacienteService, PacienteService>();

//Mapster
var mapsterConfig = TypeAdapterConfig.GlobalSettings;
mapsterConfig.Scan(typeof(PacienteMapsterConfig).Assembly);
builder.Services.AddSingleton(mapsterConfig);

// FluentValidation
builder.Services.AddScoped<IValidator<CrearPacienteDto>, CrearPacienteValidator>();

builder.Services.AddOutputCache(opciones => {
    opciones.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(60);
});

builder.Services.AddCors(opciones => {
    opciones.AddDefaultPolicy(opcionesCORS =>
    {
        opcionesCORS.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseOutputCache();

app.UseAuthorization();

app.MapControllers();

app.Run();



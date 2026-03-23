using GestorPacientes.Application.DTOs.Paciente;
using GestorPacientes.Domain.Entities;
using Mapster;
using System;
using System.Collections.Generic;
using System.Text;

namespace GestorPacientes.Application.Mappings
{
    public class PacienteMapsterConfig:IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Paciente, PacienteDto>()
                .Map(dest => dest.Documento, org => org.NumeroDocumento)
                .Map(dest => dest.Nombre, org => org.NombreCompleto)
                .Map(dest => dest.Edad, org => org.CalcularEdad())
                .Map(dest => dest.Email, org => org.Email);

            config.NewConfig<Paciente, PacienteEdicionModalDto>()
                .Map(dest => dest.Documento, org => org.NumeroDocumento)
                .Map(dest => dest.NombreCompleto, org => org.NombreCompleto)
                .Map(dest => dest.FechaNacimiento, org => org.FechaNacimiento)
                .Map(dest => dest.Email, org => org.Email);

            config.NewConfig<Paciente, PacienteListadoDto>()
                .Map(dest => dest.Id, org => org.Id)
                .Map(dest => dest.Documento, org => org.NumeroDocumento)
                .Map(dest => dest.Nombre, org => org.NombreCompleto)
                .Map(dest => dest.Edad, org => org.CalcularEdad())
                .Map(dest => dest.Email, org => org.Email);
        }
    }
}

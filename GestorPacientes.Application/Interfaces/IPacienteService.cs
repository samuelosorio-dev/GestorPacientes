using GestorPacientes.Application.DTOs.Paciente;
using GestorPacientes.Application.DTOs.Paginacion;
using System;
using System.Collections.Generic;
using System.Text;

namespace GestorPacientes.Application.Interfaces
{
    public interface IPacienteService
    {
        Task<PaginacionResponseDto<PacienteListadoDto>> GetPaginadoAsync(PaginacionDto paginacion, string? termino);
        Task<byte[]> ExportarExcelAsync(string? termino);
        Task<PacienteDto> CrearPacienteAsync(CrearPacienteDto dto);
        Task ActualizarPacienteAsync(int id, ActualizarPacienteDto dto);
        Task DesactivarPacienteAsync(int id);
        Task<PacienteEdicionModalDto?> GetByIdAsync(int id);
    }
}

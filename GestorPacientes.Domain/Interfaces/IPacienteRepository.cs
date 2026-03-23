using GestorPacientes.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GestorPacientes.Domain.Interfaces
{
    public interface IPacienteRepository
    {
        Task<IEnumerable<Paciente>> GetPaginadoAsync(int pagina, int recordsPorPagina, string? termino);
        Task<int> ContarAsync(string? termino);
        Task<IEnumerable<Paciente>> GetExportarAsync(string? termino);
        Task<bool> ExisteActivoPorDocumentoAsync(string documento);
        Task<Paciente?> GetByIdAsync(int id);
        Task<Paciente> AddAsync(Paciente paciente);
        Task UpdateAsync(Paciente paciente);
    }
}

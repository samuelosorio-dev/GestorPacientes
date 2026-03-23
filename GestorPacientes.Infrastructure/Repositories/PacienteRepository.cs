using Microsoft.EntityFrameworkCore;
using GestorPacientes.Application.DTOs.Paginacion;
using GestorPacientes.Domain.Entities;
using GestorPacientes.Domain.Interfaces;
using GestorPacientes.Infrastructure.Context;
using GestorPacientes.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace GestorPacientes.Infrastructure.Repositories
{
    public class PacienteRepository:IPacienteRepository
    {
        private readonly ApplicationDbContext _context;

        public PacienteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        private IQueryable<Paciente> GetPacientesQuery(string? termino=null) 
        {
            var query = _context.Pacientes.Where(p=>p.EstaActivo).AsQueryable();

            if (!string.IsNullOrWhiteSpace(termino))
                query = query.Where(p =>
                    p.NombreCompleto.Contains(termino) ||
                    p.NumeroDocumento.Contains(termino));

            return query.OrderBy(p=>p.NombreCompleto);
        }

        public async Task<IEnumerable<Paciente>> GetPaginadoAsync(int pagina, int recordsPorPagina, string? termino)
        {
            var paginacion = new PaginacionDto { Pagina = pagina, RecordsPorPagina = recordsPorPagina };

            return await GetPacientesQuery(termino).Paginar(paginacion).ToListAsync();
        }

        public async Task<int> ContarAsync(string? termino)
        {            
            return await GetPacientesQuery(termino).CountAsync();
        }

        public async Task<IEnumerable<Paciente>> GetExportarAsync(string? termino)
        {
            return await GetPacientesQuery(termino).ToListAsync();
        }

        public async Task<Paciente?> GetByIdAsync(int id)
        {
            return await _context.Pacientes.FindAsync(id);
        }

        public async Task<bool> ExisteActivoPorDocumentoAsync(string documento)
        {
            return await _context.Pacientes
                .AnyAsync(p => p.NumeroDocumento == documento && p.EstaActivo);
        }

        public async Task<Paciente> AddAsync(Paciente paciente)
        {
            await _context.Pacientes.AddAsync(paciente);
            await _context.SaveChangesAsync();
            return paciente;
        }
        public async Task UpdateAsync(Paciente paciente)
        {
            _context.Pacientes.Update(paciente);
            await _context.SaveChangesAsync();
        }

    }
}

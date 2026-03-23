using ClosedXML.Excel;
using GestorPacientes.Application.DTOs.Paciente;
using GestorPacientes.Application.DTOs.Paginacion;
using GestorPacientes.Application.Interfaces;
using GestorPacientes.Domain.Entities;
using GestorPacientes.Domain.Exceptions;
using GestorPacientes.Domain.Interfaces;
using Mapster;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace GestorPacientes.Application.UseCases
{
    public class PacienteService:IPacienteService
    {
        private readonly IPacienteRepository _repository;

        public PacienteService(IPacienteRepository repository)
        {
            _repository = repository;
        }

        public async Task<PacienteEdicionModalDto?> GetByIdAsync(int id)
        {
            var paciente = await _repository.GetByIdAsync(id);
            return paciente?.Adapt<PacienteEdicionModalDto>();
        }

        public async Task<PaginacionResponseDto<PacienteListadoDto>> GetPaginadoAsync(PaginacionDto paginacion, string? termino)
        {
            var pacientes = await _repository.GetPaginadoAsync(paginacion.Pagina,paginacion.RecordsPorPagina,
                termino);

            var total = await _repository.ContarAsync(termino);

            return new PaginacionResponseDto<PacienteListadoDto>
            {
                Data = pacientes.Adapt<List<PacienteListadoDto>>(),
                TotalRegistros = total,
                TotalPaginas = (int)Math.Ceiling((double)total / paginacion.RecordsPorPagina),
                PaginaActual = paginacion.Pagina
            };
        }

        public async Task<byte[]> ExportarExcelAsync(string? termino)
        {
            var pacientes = await _repository.GetExportarAsync(termino);

            using var workbook = new XLWorkbook();
            var hoja = workbook.Worksheets.Add("Pacientes");

            // Encabezados
            hoja.Cell(1, 1).Value = "Documento";
            hoja.Cell(1, 2).Value = "Nombre Completo";
            hoja.Cell(1, 3).Value = "Edad";
            hoja.Cell(1, 4).Value = "Email";

            // Estilo encabezados
            var encabezado = hoja.Range("A1:D1");
            encabezado.Style.Font.Bold = true;
            encabezado.Style.Fill.BackgroundColor = XLColor.FromHtml("#059669");
            encabezado.Style.Font.FontColor = XLColor.White;

            // Datos
            var lista = pacientes.ToList();
            for (int i = 0; i < lista.Count; i++)
            {
                var p = lista[i];
                hoja.Cell(i + 2, 1).Value = p.NumeroDocumento;
                hoja.Cell(i + 2, 2).Value = p.NombreCompleto;
                hoja.Cell(i + 2, 3).Value = p.CalcularEdad();
                hoja.Cell(i + 2, 4).Value = p.Email;               
            }

            hoja.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<PacienteDto> CrearPacienteAsync(CrearPacienteDto dto)
        {
            var existe = await _repository.ExisteActivoPorDocumentoAsync(dto.NumeroDocumento);
            if (existe)
                throw new ExcepcionNegocio("Ya existe un paciente activo con ese número de documento.");

            var paciente = Paciente.Crear(
                dto.NumeroDocumento,
                dto.NombreCompleto,
                dto.FechaNacimiento,
                dto.Email);

            await _repository.AddAsync(paciente);

            return paciente.Adapt<PacienteDto>();
        }

        public async Task ActualizarPacienteAsync(int id, ActualizarPacienteDto dto)
        {
            var paciente = await _repository.GetByIdAsync(id);
            if (paciente is null)
                throw new ExcepcionNegocio("Paciente no encontrado.");

            paciente.ActualizarDatos(dto.NombreCompleto, dto.FechaNacimiento, dto.Email);
            await _repository.UpdateAsync(paciente);
        }

        public async Task DesactivarPacienteAsync(int id)
        {
            var paciente = await _repository.GetByIdAsync(id);
            if (paciente is null)
                throw new ExcepcionNegocio("Paciente no encontrado.");

            paciente.Desactivar();
            await _repository.UpdateAsync(paciente);
        }
    }
}

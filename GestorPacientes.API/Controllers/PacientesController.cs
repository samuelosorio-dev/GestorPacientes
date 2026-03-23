using FluentValidation;
using GestorPacientes.Application.DTOs.Paciente;
using GestorPacientes.Application.DTOs.Paginacion;
using GestorPacientes.Application.Interfaces;
using GestorPacientes.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace GestorPacientes.API.Controllers
{
    [ApiController]
    [Route("api/pacientes")]
    public class PacientesController:ControllerBase
    {
        private readonly IPacienteService _service;
        private readonly IValidator<CrearPacienteDto> _validator;
        private readonly IOutputCacheStore _outputCacheStore;
        private const string cacheTag = "pacientes";


        public PacientesController(IPacienteService service, IValidator<CrearPacienteDto> validator, IOutputCacheStore outputCacheStore)
        {
            _service = service;
            _validator = validator;
            _outputCacheStore = outputCacheStore;
        }

        [HttpGet("buscarPacientes")]
        [OutputCache(Tags = [cacheTag])]
        public async Task<IActionResult> GetPaginado([FromQuery] PaginacionDto paginacion, [FromQuery] string? termino)
        {
            var resultado = await _service.GetPaginadoAsync(paginacion, termino);
            return Ok(resultado);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearPacienteDto dto)
        {
            var validacion = await _validator.ValidateAsync(dto);
            if (!validacion.IsValid)
                return BadRequest(validacion.Errors.Select(e => e.ErrorMessage));

            try
            {
                var paciente = await _service.CrearPacienteAsync(dto);
                return CreatedAtAction(nameof(GetPaginado), paciente);
            }
            catch (ExcepcionNegocio ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

    }
}

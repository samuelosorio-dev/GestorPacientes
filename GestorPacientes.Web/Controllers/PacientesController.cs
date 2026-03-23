using FluentValidation;
using GestorPacientes.Application.DTOs.Paciente;
using GestorPacientes.Application.DTOs.Paginacion;
using GestorPacientes.Application.Interfaces;
using GestorPacientes.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace GestorPacientes.Web.Controllers
{
    public class PacientesController:Controller
    {
        private readonly IPacienteService _service;
        private readonly IValidator<CrearPacienteDto> _validator;
        private readonly IValidator<ActualizarPacienteDto> _validatorActualizar;
        private readonly IOutputCacheStore _outputCacheStore;
        private const string cacheTag = "pacientes";

        public PacientesController(IPacienteService service, IValidator<CrearPacienteDto> validator, IValidator<ActualizarPacienteDto> validatorActualizar, IOutputCacheStore outputCacheStore)
        {
            _service = service;
            _validator = validator;
            _validatorActualizar = validatorActualizar;
            _outputCacheStore = outputCacheStore;
        }

        public IActionResult Index() => View();

        [HttpGet]
        [OutputCache(Tags = [cacheTag])]
        public async Task<IActionResult> ListarPaginado([FromQuery] PaginacionDto paginacion, [FromQuery] string? termino)
        {
            var resultado = await _service.GetPaginadoAsync(paginacion, termino);
            return Json(resultado);
        }

        [HttpGet]
        public async Task<IActionResult> ExportarExcel([FromQuery] string? termino)
        {
            var archivo = await _service.ExportarExcelAsync(termino);
            var nombreArchivo = string.IsNullOrWhiteSpace(termino)
                ? "pacientes.xlsx"
                : $"pacientes_{termino}.xlsx";

            return File(archivo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var paciente = await _service.GetByIdAsync(id);
            if (paciente is null) return NotFound();
            return Json(paciente);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearPacienteDto dto)
        {
            if (dto is null)
                return BadRequest(new[] { "El cuerpo de la petición no puede estar vacío." });

            var validacion = await _validator.ValidateAsync(dto);
            
            if (!validacion.IsValid)
                return BadRequest(validacion.Errors.Select(e => e.ErrorMessage));

            try
            {
                var paciente = await _service.CrearPacienteAsync(dto);
                await _outputCacheStore.EvictByTagAsync(cacheTag, default);
                return Ok(paciente);
            }
            catch (ExcepcionNegocio ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarPacienteDto dto)
        {
            if (dto is null) return BadRequest(new[] { "El cuerpo de la petición no puede estar vacío." });

            var validacion = await _validatorActualizar.ValidateAsync(dto);
            if (!validacion.IsValid)
                return BadRequest(validacion.Errors.Select(e => e.ErrorMessage));

            try
            {
                await _service.ActualizarPacienteAsync(id, dto);
                await _outputCacheStore.EvictByTagAsync(cacheTag, default);
                return Ok();
            }
            catch (ExcepcionNegocio ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Desactivar(int id)
        {
            try
            {
                await _service.DesactivarPacienteAsync(id);
                await _outputCacheStore.EvictByTagAsync(cacheTag, default);
                return Ok();
            }
            catch (ExcepcionNegocio ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }
    }
}

using FluentValidation;
using GestorPacientes.Application.DTOs.Paciente;
using System;
using System.Collections.Generic;
using System.Text;

namespace GestorPacientes.Application.Validators
{
    public class ActualizarPacienteValidator:AbstractValidator<ActualizarPacienteDto>
    {
        public ActualizarPacienteValidator()
        {
            RuleFor(x => x.NombreCompleto)
                .NotEmpty().WithMessage("El nombre completo es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es obligatorio.")
                .EmailAddress().WithMessage("El email no tiene un formato válido.");

            RuleFor(x => x.FechaNacimiento)
                .NotEmpty().WithMessage("La fecha de nacimiento es obligatoria.")
                .LessThan(DateTime.Today).WithMessage("La fecha de nacimiento no es válida.");
        }
    }
}

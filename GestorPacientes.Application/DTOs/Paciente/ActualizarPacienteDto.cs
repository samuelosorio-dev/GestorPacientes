using System;
using System.Collections.Generic;
using System.Text;

namespace GestorPacientes.Application.DTOs.Paciente
{
    public class ActualizarPacienteDto
    {
        public string NombreCompleto { get; set; } = null!;
        public DateTime FechaNacimiento { get; set; }
        public string Email { get; set; } = null!;
    }
}

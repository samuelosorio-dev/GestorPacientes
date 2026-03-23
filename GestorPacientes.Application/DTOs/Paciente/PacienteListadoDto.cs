using System;
using System.Collections.Generic;
using System.Text;

namespace GestorPacientes.Application.DTOs.Paciente
{
    public class PacienteListadoDto
    {
        public int Id { get; set; }
        public string Documento { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public int Edad { get; set; }
        public string Email { get; set; } = null!;
    }
}

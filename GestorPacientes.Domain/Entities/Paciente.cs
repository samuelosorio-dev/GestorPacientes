using GestorPacientes.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace GestorPacientes.Domain.Entities
{
    public class Paciente
    {
        public int Id { get; private set; }
        public string NumeroDocumento { get; private set; } = null!;
        public string NombreCompleto { get; private set; } = null!;
        public DateTime FechaNacimiento { get; private set; }
        public string Email { get; private set; } = string.Empty;
        public bool EstaActivo { get; private set; } = true;

        private Paciente() { }

        public static Paciente Crear(string numeroDocumento, string nombreCompleto, DateTime fechaNacimiento, string email)
        {
            if (string.IsNullOrWhiteSpace(numeroDocumento))
                throw new ExcepcionNegocio("El número de documento es obligatorio.");
            if (string.IsNullOrWhiteSpace(nombreCompleto))
                throw new ExcepcionNegocio("El nombre es obligatorio.");
            if (!string.IsNullOrWhiteSpace(email) && !email.Contains("@"))
                throw new ExcepcionNegocio("El email no es válido.");
            if (fechaNacimiento >= DateTime.Today)
                throw new ExcepcionNegocio("La fecha de nacimiento no es válida.");

            return new Paciente
            {
                NumeroDocumento = numeroDocumento,
                NombreCompleto = nombreCompleto,
                FechaNacimiento = fechaNacimiento,
                Email = email,
                EstaActivo = true
            };
        }

        public int CalcularEdad()
        {
            var hoy = DateTime.Today;
            var edad = hoy.Year - FechaNacimiento.Year;
            if (FechaNacimiento.Date > hoy.AddYears(-edad)) edad--;
            return edad;
        }

        public void ActualizarDatos(string nombreCompleto, DateTime fechaNacimiento, string email)
        {
            if (string.IsNullOrWhiteSpace(nombreCompleto))
                throw new ExcepcionNegocio("El nombre es obligatorio.");
            if (!string.IsNullOrWhiteSpace(email) && !email.Contains("@"))
                throw new ExcepcionNegocio("El email no es válido.");
            if (fechaNacimiento >= DateTime.Today)
                throw new ExcepcionNegocio("La fecha de nacimiento no es válida.");

            NombreCompleto=nombreCompleto;
            FechaNacimiento=fechaNacimiento;
            Email=email;
        }

        public void Desactivar()
        {
            if (!EstaActivo)
                throw new ExcepcionNegocio("El paciente ya está inactivo.");
            EstaActivo = false;
        }
    }
}

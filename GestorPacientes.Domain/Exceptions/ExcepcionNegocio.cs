using System;
using System.Collections.Generic;
using System.Text;

namespace GestorPacientes.Domain.Exceptions
{
    public class ExcepcionNegocio:System.Exception
    {
        public ExcepcionNegocio(string mensaje) : base(mensaje) { }
    }
}

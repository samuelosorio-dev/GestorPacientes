using System;
using System.Collections.Generic;
using System.Text;

namespace GestorPacientes.Application.DTOs.Paginacion
{
    public class PaginacionDto
    {
        public int Pagina { get; set; } = 1;
        private int recordsPorPagina = 5;
        private readonly int cantidadMaxima = 10;

        public int RecordsPorPagina
        {
            get => recordsPorPagina;
            set => recordsPorPagina = value > cantidadMaxima ? cantidadMaxima : value;
        }
    }
}

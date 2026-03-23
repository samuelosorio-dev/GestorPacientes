using System;
using System.Collections.Generic;
using System.Text;

namespace GestorPacientes.Application.DTOs.Paginacion
{
    public class PaginacionResponseDto<T>
    {
        public List<T> Data { get; set; } = [];
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
        public int PaginaActual { get; set; }
    }
}

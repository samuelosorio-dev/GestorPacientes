using GestorPacientes.Application.DTOs.Paginacion;
using System;
using System.Collections.Generic;
using System.Text;

namespace GestorPacientes.Infrastructure.Extensions
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> Paginar<T>(this IQueryable<T> queryable, PaginacionDto paginacion)
        {
            return queryable
                .Skip((paginacion.Pagina - 1) * paginacion.RecordsPorPagina)
                .Take(paginacion.RecordsPorPagina);
        }
    }
}

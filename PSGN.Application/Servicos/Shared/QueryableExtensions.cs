using Microsoft.EntityFrameworkCore;

namespace PSGN.Application.Servicos.Shared
{
    // Paginação de qualquer consulta (conta o total e traz só a página pedida). Usado pelo AlunoService
    public static class QueryableExtensions
    {
        private const int TamanhoMaximo = 50;

        public static async Task<PagedResult<T>> PaginarAsync<T>(this IQueryable<T> consulta, int pagina, int tamanhoPagina)
        {
            tamanhoPagina = Math.Clamp(tamanhoPagina, 1, TamanhoMaximo);
            var total = await consulta.CountAsync();
            var totalPaginas = Math.Max(1, (int)Math.Ceiling((double)total / tamanhoPagina));
            pagina = Math.Clamp(pagina, 1, totalPaginas);

            var itens = await consulta
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return new PagedResult<T>
            {
                TotalItems = total,
                TamanhoPagina = tamanhoPagina,
                PaginaAtual = pagina,
                Items = itens
            };
        }
    }
}

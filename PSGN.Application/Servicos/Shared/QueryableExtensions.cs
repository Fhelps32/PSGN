using Microsoft.EntityFrameworkCore; // CountAsync, ToListAsync

namespace PSGN.Application.Servicos.Shared // pasta nova, igual à "Servicos/Shared" do PSG
{
    // REVISÃO: extensão de paginação portada do PSG (lá chamava-se Paginar com um PaginationRequest; aqui recebe os dois números direto, que é tudo o que usa)
    public static class QueryableExtensions // métodos de extensão sobre IQueryable
    {
        private const int TamanhoMaximo = 50; // teto do tamanho da página, para uma URL como ?TamanhoPagina=100000 não trazer a tabela inteira

        public static async Task<PagedResult<T>> PaginarAsync<T>(this IQueryable<T> consulta, int pagina, int tamanhoPagina) // consulta já filtrada e ordenada
        {
            tamanhoPagina = Math.Clamp(tamanhoPagina, 1, TamanhoMaximo); // força o tamanho para dentro dos limites
            var total = await consulta.CountAsync(); // 1º SELECT COUNT(*): quantos registros no total
            var totalPaginas = Math.Max(1, (int)Math.Ceiling((double)total / tamanhoPagina)); // quantas páginas (no mínimo 1, mesmo vazia)
            pagina = Math.Clamp(pagina, 1, totalPaginas); // página fora do intervalo (?Pagina=999 ou 0) vira a última/primeira

            var itens = await consulta // 2º SELECT: só os registros da página
                .Skip((pagina - 1) * tamanhoPagina) // pula as páginas anteriores
                .Take(tamanhoPagina) // pega só o tamanho de uma página
                .ToListAsync(); // executa e traz a lista de entidades

            return new PagedResult<T> // monta o resultado
            { // inicializador
                TotalItems = total, // total de registros
                TamanhoPagina = tamanhoPagina, // tamanho usado
                PaginaAtual = pagina, // página devolvida (já corrigida)
                Items = itens // registros da página
            }; // fim do resultado
        } // fim do PaginarAsync
    } // fim da classe
} // fim do namespace

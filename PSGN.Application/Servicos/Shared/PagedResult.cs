namespace PSGN.Application.Servicos.Shared
{
    // Página de resultados (itens + totais). Os itens são entidades do domínio; usado pelo AlunoService e pela página de Alunos
    public class PagedResult<T>
    {
        public int TotalItems { get; set; }
        public int TamanhoPagina { get; set; }
        public int PaginaAtual { get; set; } = 1;
        public List<T> Items { get; set; } = new List<T>();

        public int TotalPaginas => TamanhoPagina <= 0 ? 0 : (int)Math.Ceiling((double)TotalItems / TamanhoPagina);
    }
}

namespace PSGN.Application.Servicos.Shared // REVISÃO: pasta nova, igual à "Servicos/Shared" do PSG
{
    // REVISÃO: resultado paginado, portado do PSG. É só um recipiente: os Items continuam sendo entidades do domain (nunca DTO),
    // seguindo a arquitetura do PSGN; o controller é quem transforma cada item em ViewModel
    public class PagedResult<T> // T = o tipo da entidade listada (ex.: Aluno)
    {
        public int TotalItems { get; set; } // quantos registros existem no total, já considerando os filtros
        public int TamanhoPagina { get; set; } // quantos registros cabem numa página
        public int PaginaAtual { get; set; } = 1; // página devolvida (começa em 1)
        public List<T> Items { get; set; } = new List<T>(); // os registros desta página

        public int TotalPaginas => TamanhoPagina <= 0 ? 0 : (int)Math.Ceiling((double)TotalItems / TamanhoPagina); // total de páginas (0 quando não há registros)
    } // fim da classe
} // fim do namespace

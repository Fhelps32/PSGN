namespace PSGN.Presentation.ViewModels // REVISÃO: arquivo novo; mesmo namespace dos outros ViewModels, que o _ViewImports.cshtml já importa em todas as views
{
    // Modelos das partials compartilhadas do design system (Views/Shared/_EmptyState, _Alert, _Kpi e _Paginacao).
    // Só carregam o que a partial precisa para desenhar: nenhuma regra de negócio

    public class EstadoVazioViewModel // modelo da partial _EmptyState (lista vazia, sem resultados)
    {
        public string Icone { get; set; } = "bi-inbox"; // classe do ícone Bootstrap Icons
        public string Titulo { get; set; } = string.Empty; // o que aconteceu (curto)
        public string? Descricao { get; set; } // o que fazer a seguir (opcional)
    } // fim do EstadoVazioViewModel

    public class AlertaViewModel // modelo da partial _Alert (aviso fixo na página)
    {
        public string Tipo { get; set; } = "success"; // "success", "danger", "warning" ou "info" (REVISÃO: "info" é novo, usa o sky da identidade)
        public string Mensagem { get; set; } = string.Empty; // texto do aviso
        public bool Fechavel { get; set; } = true; // mostra o botão de fechar?
    } // fim do AlertaViewModel

    public class IndicadorViewModel // modelo da partial _Kpi (número grande que também é um link)
    {
        public string Rotulo { get; set; } = string.Empty; // o que está sendo contado
        public int Valor { get; set; } // o número
        public string Icone { get; set; } = string.Empty; // classe do ícone Bootstrap Icons
        public string Tom { get; set; } = "accent"; // REVISÃO: tom do tile do ícone, só visual (autorizado pelo usuário): "accent" (sky) ou "highlight" (coral). Não carrega nenhuma regra de negócio
        public string Controller { get; set; } = string.Empty; // para onde o indicador leva
        public string Action { get; set; } = "Index"; // action de destino
    } // fim do IndicadorViewModel

    public class PaginacaoViewModel // modelo da partial _Paginacao
    {
        public int PaginaAtual { get; set; } = 1; // página que está na tela (começa em 1)
        public int TotalPaginas { get; set; } = 1; // quantas páginas existem
        public int TotalItens { get; set; } // quantos registros existem no total (já com os filtros)
        public int ItensPorPagina { get; set; } // tamanho da página
        public string Rotulo { get; set; } = "itens"; // plural do que está sendo listado ("alunos")
        public Dictionary<string, string> Rota { get; set; } = new(); // filtros e ordenação ativos; vão em cada link de página para não se perderem

        public bool TemAnterior => PaginaAtual > 1; // existe página anterior?
        public bool TemProxima => PaginaAtual < TotalPaginas; // existe próxima página?
        public int PrimeiroItem => TotalItens == 0 ? 0 : (PaginaAtual - 1) * ItensPorPagina + 1; // posição do primeiro registro mostrado
        public int UltimoItem => Math.Min(PaginaAtual * ItensPorPagina, TotalItens); // posição do último registro mostrado
    } // fim do PaginacaoViewModel
} // fim do namespace

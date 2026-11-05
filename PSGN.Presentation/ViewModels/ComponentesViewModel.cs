namespace PSGN.Presentation.ViewModels
{
    // Modelos das partials do design system (_EmptyState, _Alert, _Kpi e _Paginacao)

    public class EstadoVazioViewModel
    {
        public string Icone { get; set; } = "bi-inbox";
        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
    }

    public class AlertaViewModel
    {
        public string Tipo { get; set; } = "success";
        public string Mensagem { get; set; } = string.Empty;
        public bool Fechavel { get; set; } = true;
    }

    public class IndicadorViewModel
    {
        public string Rotulo { get; set; } = string.Empty;
        public int Valor { get; set; }
        public string Icone { get; set; } = string.Empty;
        public string Tom { get; set; } = "accent";
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = "Index";
    }

    public class PaginacaoViewModel
    {
        public int PaginaAtual { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;
        public int TotalItens { get; set; }
        public int ItensPorPagina { get; set; }
        public string Rotulo { get; set; } = "itens";
        public Dictionary<string, string> Rota { get; set; } = new();

        public bool TemAnterior => PaginaAtual > 1;
        public bool TemProxima => PaginaAtual < TotalPaginas;
        public int PrimeiroItem => TotalItens == 0 ? 0 : (PaginaAtual - 1) * ItensPorPagina + 1;
        public int UltimoItem => Math.Min(PaginaAtual * ItensPorPagina, TotalItens);
    }
}

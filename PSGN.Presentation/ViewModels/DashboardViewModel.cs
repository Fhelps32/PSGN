using Microsoft.AspNetCore.Mvc.Rendering;

// REVISÃO: o namespace veio do PSG (PSG.Presentation.Models.DashBoard). Troquei para o mesmo da HomeViewModel, que já é
// importado em todas as views pelo _ViewImports.cshtml
namespace PSGN.Presentation.ViewModels
{
    // REVISÃO: removidos deste ViewModel, a pedido do usuário, tudo o que dependia da antiga situação das inscrições:
    // AlunosCancelados (AlunoCanceladoItem), SituacaoInscricoes (SituacaoInscricoesItem), CursoMaiorCancelamento e CursoMaiorReprovacao (CursoImportanciaItem).
    // Os "alunos cancelados" voltam a ser tratados no futuro, quando o domain guardar o cancelamento e a data dele.
    public class DashboardViewModel // REVISÃO: era "DashBoardViewModel" (B maiúsculo), diferente do nome do arquivo e do controller; agora todos usam "Dashboard"
    {
        public LineGraphSection LineGraphSection { get; set; } = new LineGraphSection();
        public SectorGraphSection SectorGraphSection { get; set; } = new SectorGraphSection();
        public int TotalAlunos { get; set; }
        public int TotalCursos { get; set; }
    }

    public class LineGraphSection
    {
        public List<ValueLineGraph> ValueLineGraph { get; set; } = new List<ValueLineGraph>();
        public List<SelectListItem> Tempo { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Cursos { get; set; } = new List<SelectListItem>();
    }

    public class ValueLineGraph
    {
        public int QuantidadeAlunos { get; set; }
        public DateTime Data { get; set; }
    }

    public class SectorGraphSection
    {
        public List<ValueSectorGraph> ValueSectorGraph { get; set; } = new List<ValueSectorGraph>();
        public List<SelectListItem> Cursos { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Tempo { get; set; } = new List<SelectListItem>();
    }

    public class ValueSectorGraph
    {
        public int QuantidadeAlunos { get; set; }
        public string Curso { get; set; } = string.Empty;
        public string? Modulo { get; set; }
    }
}

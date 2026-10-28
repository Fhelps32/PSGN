using Microsoft.AspNetCore.Mvc.Rendering;

namespace PSG.Presentation.Models.DashBoard
{
    public class DashBoardViewModel
    {
        public LineGraphSection LineGraphSection { get; set; } = new LineGraphSection();
        public SectorGraphSection SectorGraphSection { get; set; } = new SectorGraphSection();
        public List<AlunoCanceladoItem> AlunosCancelados { get; set; } = new List<AlunoCanceladoItem>();
        public List<SituacaoInscricoesItem> SituacaoInscricoes { get; set; } = new List<SituacaoInscricoesItem>();
        public CursoImportanciaItem? CursoMaiorCancelamento { get; set; }
        public CursoImportanciaItem? CursoMaiorReprovacao { get; set; }
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

    public class AlunoCanceladoItem
    {
        public string Nome { get; set; } = string.Empty;
        public string Curso { get; set; } = string.Empty;
        public string Modulo { get; set; } = string.Empty;
        public DateTime DataCancelamento { get; set; }
    }
    public class SituacaoInscricoesItem
    {
        public string Curso { get; set; } = string.Empty;
        public int Aprovados { get; set; }
        public int EmAndamento { get; set; }
        public int Reprovados { get; set; }
        public int Cancelados { get; set; }
        public int Total => Aprovados + EmAndamento + Reprovados + Cancelados;
    }

    public class CursoImportanciaItem
    {
        public string Curso { get; set; } = string.Empty;
        public int Taxa { get; set; }
        public int QuantidadeAlunosImportancia { get; set; }
        public int QuantidadeAlunosTotal { get; set; }
    }
}
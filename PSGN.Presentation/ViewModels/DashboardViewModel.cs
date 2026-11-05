using Microsoft.AspNetCore.Mvc.Rendering;

namespace PSGN.Presentation.ViewModels
{
    // ViewModel do Dashboard (DashboardController -> Views/Dashboard/Index)
    public class DashboardViewModel
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

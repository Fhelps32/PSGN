using Microsoft.AspNetCore.Mvc.Rendering;

namespace PSGN.Presentation.ViewModels
{
    public class AlunosViewModel
    {
        public List<SelectListItem> Modulos { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Cursos { get; set; } = new List<SelectListItem>();
        public int TotalAlunos { get; set; }
        public List<AlunosAlunoItem> Alunos { get; set; } = new List<AlunosAlunoItem>();
    }

    public class AlunosAlunoItem
    {
        public string Matricula { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Curso { get; set; } = string.Empty;
        public string ModuloAtual { get; set; } = string.Empty;

        public List<AlunosModuloItem> Modulos { get; set; } = new List<AlunosModuloItem>();
    }

    public class AlunosModuloItem
    {
        public string Nome { get; set; } = string.Empty;
        public DateTime DataInscricao { get; set; }
        public DateTime? DataFim { get; set; }
        public double NotaFinal { get; set; }
    }
}

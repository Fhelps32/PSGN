using Microsoft.AspNetCore.Mvc.Rendering;

namespace PSGN.Presentation.ViewModels
{
    public class AlunosViewModel
    {
        public List<SelectListItem> Modulos { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Cursos { get; set; } = new List<SelectListItem>();
        public int TotalAlunos { get; set; } // REVISÃO: agora é dinâmico: é a quantidade de alunos que o filtro devolveu (Alunos.Count), não o total do banco

        // REVISÃO: os três filtros abaixo foram acrescentados (aprovados pelo usuário) para a tela lembrar o que foi escolhido depois do GET do formulário
        public string? Nome { get; set; } // parte do nome do aluno digitada no filtro (null = sem filtro)
        public int? CursoId { get; set; } // curso escolhido no filtro (null = todos)
        public int? ModuloId { get; set; } // módulo escolhido no filtro (null = todos)

        // REVISÃO: ordenação e paginação (autorizadas pelo usuário no redesign); as views usam estes campos para marcar a coluna ordenada e montar a paginação
        public string OrdenarPor { get; set; } = "nome"; // coluna ordenada: "nome" ou "curso"
        public bool Decrescente { get; set; } // true = Z-A
        public PaginacaoViewModel Paginacao { get; set; } = new PaginacaoViewModel(); // dados da paginação (página atual, total, links)

        public List<AlunosAlunoItem> Alunos { get; set; } = new List<AlunosAlunoItem>();
    }

    public class AlunosAlunoItem
    {
        public string Matricula { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Curso { get; set; } = string.Empty;
        public string ModuloAtual { get; set; } = string.Empty; // vazio quando o aluno não tem nenhum módulo em aberto

        public List<AlunosModuloItem> Modulos { get; set; } = new List<AlunosModuloItem>();
    }

    public class AlunosModuloItem
    {
        public string Nome { get; set; } = string.Empty;
        public DateTime DataInscricao { get; set; }
        public DateTime? DataFim { get; set; }
        public double? NotaFinal { get; set; } // REVISÃO: era "double". Sem tentativa corrigida não existe nota; com "double" a tela mostraria 0,0 como se o aluno tivesse tirado zero
    }
}

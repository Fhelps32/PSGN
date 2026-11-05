using Microsoft.AspNetCore.Mvc.Rendering;
using PSGN.Domain.Enum;

namespace PSGN.Presentation.ViewModels
{
    // ViewModels da página de Alunos (AlunosController -> Views/Alunos/Index)
    public class AlunosViewModel
    {
        public List<SelectListItem> Modulos { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Cursos { get; set; } = new List<SelectListItem>();
        public int TotalAlunos { get; set; }

        // Filtros escolhidos (voltam preenchidos no formulário)
        public string? Nome { get; set; }
        public int? CursoId { get; set; }
        public int? ModuloId { get; set; }
        public PaginacaoViewModel Paginacao { get; set; } = new PaginacaoViewModel();

        public List<AlunosAlunoItem> Alunos { get; set; } = new List<AlunosAlunoItem>();
        public AlunosDetalheItem? Selecionado { get; set; }
    }

    // Linha da lista da esquerda
    public class AlunosAlunoItem
    {
        public int IdAluno { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Curso { get; set; } = string.Empty;
        public string ModuloAtual { get; set; } = string.Empty;
        public bool Selecionado { get; set; }
    }

    // Painel da direita: aluno selecionado
    public class AlunosDetalheItem
    {
        public int IdAluno { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Curso { get; set; } = string.Empty;
        public int ModulosConcluidos { get; set; }
        public int TotalModulosCurso { get; set; }
        public AlunosModuloItem? ModuloAtual { get; set; }
        public List<AlunosModuloItem> Historico { get; set; } = new List<AlunosModuloItem>();
    }

    // Uma inscrição (módulo) do aluno, com todas as tentativas
    public class AlunosModuloItem
    {
        public int IdAlunoModulo { get; set; }
        public string Nome { get; set; } = string.Empty;
        public DateTime DataInscricao { get; set; }
        public DateTime? DataFim { get; set; }
        public double? NotaFinal { get; set; }
        public bool Aprovado { get; set; }
        public bool Atual { get; set; }
        public bool PodeAbrirTentativa { get; set; }
        public int TentativasReprovadas { get; set; }
        public List<AlunosTentativaItem> Tentativas { get; set; } = new List<AlunosTentativaItem>();
        public AlunosTentativaItem? TentativaAberta => Tentativas.FirstOrDefault(t => t.Aberta);
    }

    // Uma tentativa, já com a situação e as ações permitidas calculadas pelo domínio
    public class AlunosTentativaItem
    {
        public int IdTentativa { get; set; }
        public int Numero { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataLimite { get; set; }
        public DateTime? DataFim { get; set; }
        public bool Aberta { get; set; }
        public bool PrazoVencido { get; set; }
        public int DiasParaOPrazo { get; set; }
        public DateTime? DataProva { get; set; }
        public double? Nota { get; set; }
        public EnumSituacaoTentativa Situacao { get; set; }
        public bool PodeAprovar { get; set; }
        public bool PodeReprovar { get; set; }
        public bool PodeReprovarPorInatividade { get; set; }
    }
}

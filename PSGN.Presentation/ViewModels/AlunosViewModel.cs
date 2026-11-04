using Microsoft.AspNetCore.Mvc.Rendering;
using PSGN.Domain.Enum; // REVISÃO: EnumSituacaoTentativa, calculado pela regra do domínio

namespace PSGN.Presentation.ViewModels
{
    // REVISÃO: ViewModel refeito para o layout mestre-detalhe (como a página de Alunos do PSG): à esquerda a lista paginada de alunos;
    // à direita o aluno selecionado, com o módulo atual e o histórico de todos os módulos e de todas as tentativas de cada um
    public class AlunosViewModel
    {
        public List<SelectListItem> Modulos { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Cursos { get; set; } = new List<SelectListItem>();
        public int TotalAlunos { get; set; } // dinâmico: quantos alunos o filtro encontrou

        // Filtros escolhidos (a tela lembra o que foi pedido depois do GET do formulário)
        public string? Nome { get; set; } // parte do nome OU da matrícula
        public int? CursoId { get; set; } // curso escolhido no filtro (null = todos)
        public int? ModuloId { get; set; } // módulo escolhido no filtro (null = todos)
        public PaginacaoViewModel Paginacao { get; set; } = new PaginacaoViewModel(); // dados da paginação (página atual, total, links)

        public List<AlunosAlunoItem> Alunos { get; set; } = new List<AlunosAlunoItem>(); // itens da lista da esquerda
        public AlunosDetalheItem? Selecionado { get; set; } // REVISÃO: aluno mostrado à direita (null quando a lista está vazia)
    }

    public class AlunosAlunoItem // item da lista da esquerda
    {
        public int IdAluno { get; set; } // REVISÃO: para o link que seleciona o aluno
        public string Matricula { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Curso { get; set; } = string.Empty;
        public string ModuloAtual { get; set; } = string.Empty; // vazio quando o aluno ainda não entrou em nenhum módulo
        public bool Selecionado { get; set; } // REVISÃO: é o aluno mostrado à direita?
    }

    public class AlunosDetalheItem // REVISÃO: aluno selecionado (coluna da direita)
    {
        public int IdAluno { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Curso { get; set; } = string.Empty;
        public int ModulosConcluidos { get; set; } // módulos com tentativa aprovada
        public int TotalModulosCurso { get; set; } // quantos módulos o curso tem
        public AlunosModuloItem? ModuloAtual { get; set; } // o último módulo em que entrou (null se não entrou em nenhum)
        public List<AlunosModuloItem> Historico { get; set; } = new List<AlunosModuloItem>(); // todos os módulos em que entrou, do mais recente ao mais antigo
    }

    public class AlunosModuloItem // um módulo do histórico do aluno (uma inscrição)
    {
        public int IdAlunoModulo { get; set; } // REVISÃO: para a ação "Abrir nova tentativa"
        public string Nome { get; set; } = string.Empty;
        public DateTime DataInscricao { get; set; }
        public DateTime? DataFim { get; set; } // conclusão do módulo (preenchida quando uma tentativa é aprovada)
        public double? NotaFinal { get; set; } // nota da tentativa aprovada; sem aprovação, a da tentativa mais recente com nota
        public bool Aprovado { get; set; } // REVISÃO: alguma tentativa aprovada?
        public bool Atual { get; set; } // REVISÃO: é o módulo atual?
        public bool PodeAbrirTentativa { get; set; } // REVISÃO: não aprovado e sem tentativa em curso
        public int TentativasReprovadas { get; set; } // REVISÃO: quantas foram reprovadas (pela nota ou por inatividade)
        public List<AlunosTentativaItem> Tentativas { get; set; } = new List<AlunosTentativaItem>(); // REVISÃO: todas as tentativas, na ordem em que foram abertas
        public AlunosTentativaItem? TentativaAberta => Tentativas.FirstOrDefault(t => t.Aberta); // a tentativa em curso, se houver
    }

    public class AlunosTentativaItem // REVISÃO: uma tentativa (período entre a abertura e a decisão do administrador)
    {
        public int IdTentativa { get; set; } // para as ações do administrador
        public int Numero { get; set; } // 1ª, 2ª, 3ª... tentativa no módulo
        public DateTime DataInicio { get; set; } // abertura
        public DateTime DataLimite { get; set; } // abertura + 40 dias
        public DateTime? DataFim { get; set; } // quando o administrador fechou (null = aberta)
        public bool Aberta { get; set; } // ainda em curso?
        public bool PrazoVencido { get; set; } // aberta e passou do prazo (só um aviso)
        public int DiasParaOPrazo { get; set; } // positivo = faltam; negativo = vencido há
        public DateTime? DataProva { get; set; } // quando o aluno fez a prova no Moodle (null = não fez)
        public double? Nota { get; set; } // nota da prova
        public EnumSituacaoTentativa Situacao { get; set; } // calculada pelo domínio
        public bool PodeAprovar { get; set; } // ações permitidas, calculadas pelo domínio
        public bool PodeReprovar { get; set; }
        public bool PodeReprovarPorInatividade { get; set; }
    }
}



namespace PSGN.Domain
{
    public class AlunoModulo
    {
        public int IdAlunoModulo { get; set; }
        public int IdAluno { get; set; }
        public int IdModulo { get; set; }
        public DateTime DataInscricao { get; set; }
        public DateTime? DataConclusao { get; set; }

        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public bool Status { get; set; } = true;

        #region Relações
        public Aluno Aluno { get; set; }
        public Modulo Modulo { get; set; }
        public ICollection<Tentativa> Tentativas { get; set; } = new List<Tentativa>();
        #endregion

        private AlunoModulo() { }
        public AlunoModulo(Aluno aluno, Modulo modulo)
        {
            Aluno = aluno ?? throw new ArgumentNullException(nameof(aluno));
            Modulo = modulo ?? throw new ArgumentNullException(nameof(modulo));
            DataInscricao = DateTime.Now;
        }

        public AlunoModulo(Aluno aluno, Modulo modulo, DateTime dataInscricao)
        {
            Aluno = aluno ?? throw new ArgumentNullException(nameof(aluno));
            Modulo = modulo ?? throw new ArgumentNullException(nameof(modulo));
            DataInscricao = dataInscricao;
        }

        // REVISÃO: método novo, usado pelo Dashboard. Calcula a situação do aluno neste módulo a partir dos dados que já existem.
        // Fica no domain (e não no controller) porque é regra de negócio: qualquer tela que precisar da situação usa a mesma regra.
        // ATENÇÃO: precisa que Aluno e Tentativas tenham sido carregados (Include), senão a regra enxerga coleções vazias.
        public Enum.EnumSituacaoInscricao ObterSituacao(double notaMinimaAprovacao) // recebe a nota mínima de fora, porque ela é política da instituição e não do aluno
        {
            if (!Status || !Aluno.Status) // a inscrição ou o próprio aluno foi desativado
                return Enum.EnumSituacaoInscricao.Cancelado; // então a situação é cancelado, independente das notas

            if (Tentativas.Any(t => t.Nota >= notaMinimaAprovacao)) // alguma tentativa atingiu a nota mínima? (Nota null nunca passa nessa comparação)
                return Enum.EnumSituacaoInscricao.Aprovado; // basta uma para estar aprovado

            if (Tentativas.Any(t => t.DataFimAttempt is not null && t.Nota is not null)) // existe tentativa terminada e corrigida (e, pela regra de cima, abaixo da média)?
                return Enum.EnumSituacaoInscricao.Reprovado; // então fez a prova e não passou

            return Enum.EnumSituacaoInscricao.EmAndamento; // sem tentativa terminada: ainda está cursando o módulo
        } // fim do ObterSituacao
    }
}

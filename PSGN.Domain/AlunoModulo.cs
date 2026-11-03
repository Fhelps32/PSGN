

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

        // REVISÃO: o ObterSituacao (Aprovado/Reprovado/Em andamento/Cancelado) foi REMOVIDO: a classificação antiga da situação das inscrições não existe mais no PSGN
        // REVISÃO: regra definida pelo usuário: a nota final do módulo é a nota do questionário final, que fica em Tentativa.Nota
        // ATENÇÃO: precisa que as Tentativas tenham sido carregadas (Include), senão a regra enxerga a coleção vazia
        public double? ObterNotaFinal() => Tentativas.Max(t => t.Nota); // sem tentativa (ou sem nota corrigida) devolve null; com várias tentativas vale a MAIOR nota, igual ao método de avaliação padrão do questionário do Moodle
    }
}

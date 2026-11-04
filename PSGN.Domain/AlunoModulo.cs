

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
        // REVISÃO (regra de tentativas): ao entrar no módulo o aluno abre uma tentativa; reprovado, o administrador abre uma nova; com uma tentativa
        // aprovada (nota >= 7) o módulo fica concluído e ele passa para o próximo. ATENÇÃO: tudo abaixo precisa das Tentativas carregadas (Include)

        public IEnumerable<Tentativa> TentativasEmOrdem => Tentativas // histórico na ordem em que as tentativas foram abertas
            .OrderBy(t => t.DataInicioTentativa) // a mais antiga primeiro
            .ThenBy(t => t.IdTentativa); // desempate: a criada primeiro

        public Tentativa? TentativaAberta => Tentativas.FirstOrDefault(t => t.Aberta); // a tentativa em curso (no máximo uma por vez)

        public bool Aprovado => Tentativas.Any(t => t.Situacao == Enum.EnumSituacaoTentativa.Aprovada); // alguma tentativa foi aprovada: módulo concluído

        public bool PodeAbrirTentativa => !Aprovado && TentativaAberta is null; // só abre se o módulo não foi aprovado e não há outra em curso

        // Nota final do módulo: a da tentativa aprovada; sem aprovação, a da tentativa mais recente que tem nota (null se nenhuma tem)
        public double? ObterNotaFinal() => Tentativas.FirstOrDefault(t => t.Situacao == Enum.EnumSituacaoTentativa.Aprovada)?.Nota
            ?? TentativasEmOrdem.LastOrDefault(t => t.Nota is not null)?.Nota;

        public Tentativa AbrirTentativa(DateTime inicio) // abre uma tentativa com o prazo padrão (40 dias) e devolve a nova
        {
            if (!PodeAbrirTentativa) throw new InvalidOperationException(Aprovado
                ? "Este módulo já foi aprovado; não é possível abrir outra tentativa." // módulo concluído
                : "Já existe uma tentativa aberta neste módulo."); // só uma tentativa em curso por vez
            var tentativa = new Tentativa(this, inicio, Tentativa.PrazoPadrao); // nova tentativa, ligada a esta inscrição
            Tentativas.Add(tentativa); // entra na coleção: o EF grava junto com o AlunoModulo
            return tentativa; // devolve para quem chamou saber qual foi aberta
        } // fim do AbrirTentativa

        public void Concluir(DateTime agora) => DataConclusao = agora; // marca o módulo como concluído (chamado quando uma tentativa é aprovada)
    }
}

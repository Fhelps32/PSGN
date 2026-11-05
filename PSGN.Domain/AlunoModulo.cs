

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

        // Regras de tentativas da inscrição (usadas pelo TentativaService e pela página de Alunos). Exigem as Tentativas carregadas (Include)
        public IEnumerable<Tentativa> TentativasEmOrdem => Tentativas
            .OrderBy(t => t.DataInicioTentativa)
            .ThenBy(t => t.IdTentativa);

        public Tentativa? TentativaAberta => Tentativas.FirstOrDefault(t => t.Aberta);

        public bool Aprovado => Tentativas.Any(t => t.Situacao == Enum.EnumSituacaoTentativa.Aprovada);

        public bool PodeAbrirTentativa => !Aprovado && TentativaAberta is null;

        // Nota da tentativa aprovada; sem aprovação, a da tentativa mais recente com nota
        public double? ObterNotaFinal() => Tentativas.FirstOrDefault(t => t.Situacao == Enum.EnumSituacaoTentativa.Aprovada)?.Nota
            ?? TentativasEmOrdem.LastOrDefault(t => t.Nota is not null)?.Nota;

        // Abre uma nova tentativa com o prazo padrão (lança exceção se já aprovado ou se já houver uma aberta)
        public Tentativa AbrirTentativa(DateTime inicio)
        {
            if (!PodeAbrirTentativa) throw new InvalidOperationException(Aprovado
                ? "Este módulo já foi aprovado; não é possível abrir outra tentativa."
                : "Já existe uma tentativa aberta neste módulo.");
            var tentativa = new Tentativa(this, inicio, Tentativa.PrazoPadrao);
            Tentativas.Add(tentativa);
            return tentativa;
        }

        public void Concluir(DateTime agora) => DataConclusao = agora;
    }
}

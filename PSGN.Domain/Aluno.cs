

namespace PSGN.Domain
{
    public class Aluno
    {
        public int IdAluno { get; set; }
        public int IdUsuario { get; set; }
        public int IdCurso { get; set; }
        public bool Egresso { get; set; }
        public DateTime DataMatricula { get; set; } = DateTime.Now;

        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public bool Status { get; set; } = true;

        #region Relações
        public Usuario Usuario { get; set; }
        public Curso Curso { get; set; }
        public ICollection<AlunoModulo> Modulos { get; set; } = new List<AlunoModulo>();
        #endregion

        // REVISÃO: regra definida pelo usuário: o módulo atual é a inscrição mais recente que ainda não tem DataConclusao
        // ATENÇÃO: precisa que os Modulos (inscrições) tenham sido carregados (Include), senão devolve sempre null
        public AlunoModulo? ObterModuloAtual() => Modulos // parte das inscrições do aluno
            .Where(am => am.DataConclusao is null) // só as que ainda não foram concluídas
            .OrderByDescending(am => am.DataInscricao) // a inscrição mais recente primeiro
            .ThenByDescending(am => am.IdAlunoModulo) // desempate: a sincronização grava vários módulos com a mesma data, então vale o último criado
            .FirstOrDefault(); // pega a primeira ou null se o aluno não tiver módulo em aberto

        private Aluno() { }
        public Aluno(Usuario usuario, Curso curso, bool egresso)
        {
            Usuario = usuario ?? throw new ArgumentNullException(nameof(usuario));
            Curso = curso ?? throw new ArgumentNullException(nameof(curso));
            Egresso = egresso;
        }
    }
}

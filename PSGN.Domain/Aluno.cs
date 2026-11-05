

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

        // Módulo atual = a última inscrição do aluno. Usado no AlunoService e na página de Alunos (exige Modulos carregados)
        public AlunoModulo? ObterModuloAtual() => Modulos
            .OrderByDescending(am => am.DataInscricao)
            .ThenByDescending(am => am.IdAlunoModulo)
            .FirstOrDefault();

        private Aluno() { }
        public Aluno(Usuario usuario, Curso curso, bool egresso)
        {
            Usuario = usuario ?? throw new ArgumentNullException(nameof(usuario));
            Curso = curso ?? throw new ArgumentNullException(nameof(curso));
            Egresso = egresso;
        }
    }
}

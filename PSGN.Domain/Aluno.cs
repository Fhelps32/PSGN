

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

        // REVISÃO: regra atualizada pelo usuário: o módulo atual é o ÚLTIMO módulo em que o aluno entrou (pode ter tentativas reprovadas). Quando ele
        // é aprovado (nota >= 7), passa para o próximo módulo, e essa nova inscrição vira o módulo atual. Antes ignorava as inscrições concluídas
        // ATENÇÃO: precisa que os Modulos (inscrições) tenham sido carregados (Include), senão devolve sempre null
        public AlunoModulo? ObterModuloAtual() => Modulos // parte das inscrições do aluno
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

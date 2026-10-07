using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private Aluno() { }
        public Aluno(Usuario usuario, Curso curso, bool egresso)
        {
            Usuario = usuario ?? throw new ArgumentNullException(nameof(usuario));
            Curso = curso ?? throw new ArgumentNullException(nameof(curso));
            Egresso = egresso;
        }
    }
}

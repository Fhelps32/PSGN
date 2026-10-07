using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Domain
{
    public class Modulo
    {
        public int IdModulo { get; set; }
        public int IdCurso { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int Numero { get; set; }
        public string IdnumberMoodle { get; set; } = string.Empty;
        public int IdCmidMoodle { get; set; }

        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public bool Status { get; set; } = true;

        #region Relações
        public Curso Curso { get; set; }
        public ICollection<ProfessorModulo> Professores { get; set; } = new List<ProfessorModulo>();
        public ICollection<AlunoModulo> Alunos { get; set; } = new List<AlunoModulo>(); 
        #endregion

        private Modulo() { }
        public Modulo(string nome, int numero, Curso curso)
        {
            Nome = nome ?? throw new ArgumentNullException(nameof(nome));
            Numero = numero;
            Curso = curso ?? throw new ArgumentNullException(nameof(curso));
        }
    }
}

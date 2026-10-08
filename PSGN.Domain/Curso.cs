using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Domain
{
    public class Curso
    {
        public int IdCurso { get; set; }
        public int IdCoordenador { get; set; }
        public int IdMoodle { get; set; } //id que vem do moodle, para poder sincronizar os cursos do moodle com o sistema
        public string Nome { get; set; } = string.Empty;
        public string Sigla { get; set; } = string.Empty;
        public int PerLetivo { get; set; }

        public DateTime DataCadastro { get; set; }
        public bool Status { get; set; }

        #region Relações
        public Usuario Coordenador { get; set; }
        public ICollection<Modulo> Modulos { get; set; } = new List<Modulo>();
        public ICollection<Aluno> Alunos { get; set; } = new List<Aluno>();
        #endregion

        private Curso() { }
        public Curso(string nome, string sigla, Usuario usuarioCoordenador)
        {
            Nome = nome ?? throw new ArgumentNullException(nameof(nome));
            Sigla = sigla ?? throw new ArgumentNullException(nameof(sigla));
            Coordenador = usuarioCoordenador ?? throw new ArgumentNullException(nameof(usuarioCoordenador));
        }
    }
}

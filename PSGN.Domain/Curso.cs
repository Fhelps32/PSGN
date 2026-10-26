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
        public int IdMoodle { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Sigla { get; set; } = string.Empty;
        public int PerLetivo { get; set; } //REVISÃO: tirei o 51 fixo; agora o período letivo vem do NSG pelo construtor

        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public bool Status { get; set; } = true;

        #region Relações
        public Usuario Coordenador { get; set; }
        public ICollection<Modulo> Modulos { get; set; } = new List<Modulo>();
        public ICollection<Aluno> Alunos { get; set; } = new List<Aluno>();
        #endregion

        private Curso() { }
        //tem que saber como vai implementar esse perLetivo aí
        public Curso(string nome, string sigla, int idMoodle, int perLetivo, Usuario usuarioCoordenador)
        {
            Nome = nome ?? throw new ArgumentNullException(nameof(nome));
            Sigla = sigla ?? throw new ArgumentNullException(nameof(sigla));
            Coordenador = usuarioCoordenador ?? throw new ArgumentNullException(nameof(usuarioCoordenador));
            IdMoodle = idMoodle;
            PerLetivo = perLetivo;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Domain
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;

        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public bool Status { get; set; } = true;

        private Usuario() { }
        public Usuario(string matricula, string nome)
        {
            Matricula = matricula ?? throw new ArgumentNullException(nameof(matricula));
            Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Domain
{
    public class ProfessorModulo
    {
        public int IdProfessorModulo { get; set; }
        public int IdUsuario { get; set; }
        public int IdModulo { get; set; }

        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public bool Status { get; set; } = true;

        #region Relações
        public Usuario Usuario { get; set; }
        public Modulo Modulo { get; set; }
        #endregion

        private ProfessorModulo() { }

        public ProfessorModulo(Usuario usuario, Modulo modulo)
        {
            Usuario = usuario ?? throw new ArgumentNullException(nameof(usuario));
            IdModulo = modulo?.IdModulo ?? throw new ArgumentNullException(nameof(modulo));
        }
    }
}

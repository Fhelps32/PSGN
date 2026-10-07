using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Domain
{
    public class Tentativa
    {
        //attempt == moodle
        public int IdTentativa { get; set; }
        public int IdAlunoModulo { get; set; }
        public int IdAttempt { get; set; } 
        public DateTime DataInicioTentativa { get; set; }
        public DateTime? DataFimTentativa { get; set; }
        public TimeSpan Prazo { get; set; }
        public DateTime? DataInicioAttempt { get; set; }
        public DateTime? DataFimAttempt { get; set; }
        public double? Nota { get; set; }
        public string Observacao { get; set; } = string.Empty;

        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public bool Status { get; set; } = true;

        #region Relações
        public AlunoModulo AlunoModulo { get; set; }
        #endregion

        private Tentativa() { }
        public Tentativa(AlunoModulo alunoModulo, DateTime dataInicioTentativa, TimeSpan prazo)
        {
            AlunoModulo = alunoModulo ?? throw new ArgumentNullException(nameof(alunoModulo));
            if (dataInicioTentativa == default)
                throw new ArgumentException("Data de início da tentativa não pode ser a data padrão.", nameof(dataInicioTentativa));
            DataInicioTentativa = dataInicioTentativa;
            Prazo = prazo;
        }

    }
}

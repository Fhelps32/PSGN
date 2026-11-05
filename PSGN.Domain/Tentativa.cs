using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSGN.Domain.Enum;

namespace PSGN.Domain
{
    /// <summary>
    /// Período de uma tentativa do aluno num módulo: abre ao entrar no módulo (ou pelo administrador), recebe a prova final do Moodle
    /// e é fechada pelo administrador (aprovada, reprovada ou reprovada por inatividade). O prazo de 40 dias só avisa, não fecha sozinho.
    /// Usada pelo TentativaService (sincronização e ações) e pela página de Alunos.
    /// </summary>
    public class Tentativa
    {
        public const double NotaMinimaAprovacao = 7.0;
        public static readonly TimeSpan PrazoPadrao = TimeSpan.FromDays(40);

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

        // Estado calculado (não é gravado; ignorado no TentativaMapping)
        public bool Aberta => DataFimTentativa is null;
        public bool TemProva => IdAttempt != 0;
        public DateTime DataLimite => DataInicioTentativa + Prazo;
        public bool PrazoVencido(DateTime agora) => Aberta && agora > DataLimite;

        public EnumSituacaoTentativa Situacao => (Aberta, TemProva, Nota) switch
        {
            (true, false, _) => EnumSituacaoTentativa.AguardandoProva,
            (true, true, null) => EnumSituacaoTentativa.ProvaEmCorrecao,
            (true, true, >= NotaMinimaAprovacao) => EnumSituacaoTentativa.AguardandoAprovacao,
            (true, true, _) => EnumSituacaoTentativa.AguardandoReprovacao,
            (false, _, >= NotaMinimaAprovacao) => EnumSituacaoTentativa.Aprovada,
            (false, _, not null) => EnumSituacaoTentativa.Reprovada,
            _ => EnumSituacaoTentativa.ReprovadaPorInatividade
        };

        public bool PodeAprovar => Situacao == EnumSituacaoTentativa.AguardandoAprovacao;
        public bool PodeReprovar => Situacao == EnumSituacaoTentativa.AguardandoReprovacao;
        public bool PodeReprovarPorInatividade => Situacao == EnumSituacaoTentativa.AguardandoProva;

        private Tentativa() { }
        public Tentativa(AlunoModulo alunoModulo, DateTime dataInicioTentativa, TimeSpan prazo)
        {
            AlunoModulo = alunoModulo ?? throw new ArgumentNullException(nameof(alunoModulo));
            if (dataInicioTentativa == default)
                throw new ArgumentException("Data de início da tentativa não pode ser a data padrão.", nameof(dataInicioTentativa));
            DataInicioTentativa = dataInicioTentativa;
            Prazo = prazo;
        }

        // Liga a prova do Moodle à tentativa aberta; vale a mais recente. Devolve false se nada mudou. Usado pelo TentativaService na sincronização
        public bool RegistrarProva(int idAttempt, DateTime? inicio, DateTime? fim, double? nota)
        {
            if (!Aberta) return false;
            if (TemProva && IdAttempt != idAttempt && inicio is not null && DataInicioAttempt is not null && inicio < DataInicioAttempt)
                return false;
            if (IdAttempt == idAttempt && DataInicioAttempt == inicio && DataFimAttempt == fim && Nota == nota)
                return false;
            IdAttempt = idAttempt;
            DataInicioAttempt = inicio;
            DataFimAttempt = fim;
            Nota = nota;
            return true;
        }

        // Ações do administrador (TentativaService, a partir da página de Alunos). Lançam InvalidOperationException se a situação não permitir
        public void Aprovar(DateTime agora)
        {
            if (!PodeAprovar) throw new InvalidOperationException("Só é possível aprovar uma tentativa aberta com nota igual ou maior que 7.");
            DataFimTentativa = agora;
            AlunoModulo.Concluir(agora);
        }

        public void Reprovar(DateTime agora)
        {
            if (!PodeReprovar) throw new InvalidOperationException("Só é possível reprovar pela nota uma tentativa aberta com nota abaixo de 7.");
            DataFimTentativa = agora;
        }

        public void ReprovarPorInatividade(DateTime agora)
        {
            if (!PodeReprovarPorInatividade) throw new InvalidOperationException("Só é possível reprovar por inatividade uma tentativa aberta e sem prova.");
            DataFimTentativa = agora;
        }
    }
}

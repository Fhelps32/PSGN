using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSGN.Domain.Enum; // REVISÃO: EnumSituacaoTentativa

namespace PSGN.Domain
{
    // REVISÃO: regra de tentativas definida pelo usuário. A tentativa é um PERÍODO controlado pelo PSGN:
    //  - abre quando o aluno entra no módulo (ou quando o administrador abre uma nova): DataInicioTentativa, com prazo de 40 dias;
    //  - recebe a prova do Moodle (IdAttempt, DataInicioAttempt, DataFimAttempt, Nota); se houver mais de uma, vale a mais recente;
    //  - é fechada pelo administrador (DataFimTentativa): aprovada (nota >= 7), reprovada (nota < 7) ou reprovada por inatividade (sem prova).
    // O prazo NÃO é aplicado automaticamente: ele só avisa o administrador.
    public class Tentativa
    {
        public const double NotaMinimaAprovacao = 7.0; // REVISÃO: nota mínima para passar para o próximo módulo
        public static readonly TimeSpan PrazoPadrao = TimeSpan.FromDays(40); // REVISÃO: prazo de cada tentativa, contado da abertura

        //attempt == moodle
        public int IdTentativa { get; set; }
        public int IdAlunoModulo { get; set; }
        public int IdAttempt { get; set; } // id da prova no Moodle; 0 = o aluno ainda não fez a prova nesta tentativa
        public DateTime DataInicioTentativa { get; set; } // REVISÃO: abertura da tentativa (entrada no módulo ou nova tentativa aberta pelo administrador)
        public DateTime? DataFimTentativa { get; set; } // REVISÃO: quando o administrador fechou (aprovou ou reprovou); null = aberta
        public TimeSpan Prazo { get; set; } // REVISÃO: duração do prazo (40 dias); gravado em ticks (ver TentativaMapping)
        public DateTime? DataInicioAttempt { get; set; } // início da prova no Moodle
        public DateTime? DataFimAttempt { get; set; } // fim da prova no Moodle
        public double? Nota { get; set; } // nota da prova (null = sem prova ou ainda não corrigida)
        public string Observacao { get; set; } = string.Empty;

        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public bool Status { get; set; } = true;

        #region Relações
        public AlunoModulo AlunoModulo { get; set; }
        #endregion

        // ---------- REVISÃO: estado calculado (nada disso é gravado no banco) ----------
        public bool Aberta => DataFimTentativa is null; // ainda não foi fechada pelo administrador
        public bool TemProva => IdAttempt != 0; // a prova do Moodle já foi ligada a esta tentativa
        public DateTime DataLimite => DataInicioTentativa + Prazo; // até quando vai o prazo
        public bool PrazoVencido(DateTime agora) => Aberta && agora > DataLimite; // aberta e passou do prazo (só um aviso para o administrador)

        public EnumSituacaoTentativa Situacao => (Aberta, TemProva, Nota) switch // em que ponto do ciclo a tentativa está
        {
            (true, false, _) => EnumSituacaoTentativa.AguardandoProva, // aberta e sem prova
            (true, true, null) => EnumSituacaoTentativa.ProvaEmCorrecao, // aberta, prova sem nota
            (true, true, >= NotaMinimaAprovacao) => EnumSituacaoTentativa.AguardandoAprovacao, // aberta, nota suficiente
            (true, true, _) => EnumSituacaoTentativa.AguardandoReprovacao, // aberta, nota abaixo de 7
            (false, _, >= NotaMinimaAprovacao) => EnumSituacaoTentativa.Aprovada, // fechada com nota suficiente
            (false, _, not null) => EnumSituacaoTentativa.Reprovada, // fechada com nota abaixo de 7
            _ => EnumSituacaoTentativa.ReprovadaPorInatividade // fechada sem nota: o aluno não fez a prova
        }; // fim do switch

        public bool PodeAprovar => Situacao == EnumSituacaoTentativa.AguardandoAprovacao; // só com nota >= 7 e ainda aberta
        public bool PodeReprovar => Situacao == EnumSituacaoTentativa.AguardandoReprovacao; // só com nota < 7 e ainda aberta
        public bool PodeReprovarPorInatividade => Situacao == EnumSituacaoTentativa.AguardandoProva; // só aberta e sem prova

        private Tentativa() { }
        public Tentativa(AlunoModulo alunoModulo, DateTime dataInicioTentativa, TimeSpan prazo)
        {
            AlunoModulo = alunoModulo ?? throw new ArgumentNullException(nameof(alunoModulo));
            if (dataInicioTentativa == default)
                throw new ArgumentException("Data de início da tentativa não pode ser a data padrão.", nameof(dataInicioTentativa));
            DataInicioTentativa = dataInicioTentativa;
            Prazo = prazo;
        }

        // ---------- REVISÃO: comportamentos (as regras ficam aqui, não no controller nem no service) ----------

        // Liga (ou troca) a prova do Moodle desta tentativa. Só vale com a tentativa aberta: fechada, o resultado já foi decidido e não muda mais
        public bool RegistrarProva(int idAttempt, DateTime? inicio, DateTime? fim, double? nota) // devolve true se algo mudou
        {
            if (!Aberta) return false; // tentativa fechada: a prova não pode mais alterar o resultado
            if (TemProva && IdAttempt != idAttempt && inicio is not null && DataInicioAttempt is not null && inicio < DataInicioAttempt)
                return false; // é outra prova, mais ANTIGA que a já registrada: vale a mais recente, então ignora
            if (IdAttempt == idAttempt && DataInicioAttempt == inicio && DataFimAttempt == fim && Nota == nota)
                return false; // nada mudou desde a última sincronização
            IdAttempt = idAttempt; // id da prova no Moodle
            DataInicioAttempt = inicio; // início da prova
            DataFimAttempt = fim; // fim da prova (null se ainda em andamento)
            Nota = nota; // nota (null se ainda não corrigida)
            return true; // houve mudança
        } // fim do RegistrarProva

        public void Aprovar(DateTime agora) // o administrador aprova: o aluno pode passar para o próximo módulo
        {
            if (!PodeAprovar) throw new InvalidOperationException("Só é possível aprovar uma tentativa aberta com nota igual ou maior que 7."); // regra da nota mínima
            DataFimTentativa = agora; // fecha a tentativa
            AlunoModulo.Concluir(agora); // o módulo fica concluído na mesma data
        } // fim do Aprovar

        public void Reprovar(DateTime agora) // o administrador reprova pela nota (abaixo de 7)
        {
            if (!PodeReprovar) throw new InvalidOperationException("Só é possível reprovar pela nota uma tentativa aberta com nota abaixo de 7."); // regra da nota
            DataFimTentativa = agora; // fecha a tentativa
        } // fim do Reprovar

        public void ReprovarPorInatividade(DateTime agora) // o administrador reprova porque o aluno não fez a prova
        {
            if (!PodeReprovarPorInatividade) throw new InvalidOperationException("Só é possível reprovar por inatividade uma tentativa aberta e sem prova."); // com prova, a decisão é pela nota
            DataFimTentativa = agora; // fecha a tentativa (sem nota = reprovada por inatividade)
        } // fim do ReprovarPorInatividade
    }
}

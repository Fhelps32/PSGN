namespace PSGN.Domain.Enum
{
    // Situação de uma tentativa, calculada em Tentativa.Situacao (não é gravada). Usada nas regras do domínio e nas etiquetas da página de Alunos
    public enum EnumSituacaoTentativa
    {
        AguardandoProva,
        ProvaEmCorrecao,
        AguardandoAprovacao,
        AguardandoReprovacao,
        Aprovada,
        Reprovada,
        ReprovadaPorInatividade
    }
}
